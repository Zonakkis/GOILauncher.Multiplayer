'use strict';
/*
    面板的客户端逻辑。这里只管三件事：房间选择（URL 查询串是唯一状态源）、
    日志增量拉取、以及 morph 会毁掉的那点浏览器状态（details 展开、聊天滚动跟随）。
    其余一切渲染都在服务端的 Razor 里 —— 没有第二套渲染器。
*/
(function () {

    // ============================================================
    // 房间选择。状态放查询串，不放 JS 变量：刷新/书签/多开标签页都一致。
    // ============================================================

    /** 轮询时把当前选中房间带上。用 configRequest 而不是 hx-vals 的 js: 表达式 ——
     *  htmx 会把不以 "{" 开头的 js: 表达式包成 {expr}，`js:f()` 会变成方法简写 `{f()}`
     *  而语法错误，每次触发都抛异常导致请求发不出去。普通 JS 没有这个陷阱。 */
    document.body.addEventListener('htmx:configRequest', function (e) {
        if (!e.detail.elt || e.detail.elt.id !== 'poller') return;
        var room = new URLSearchParams(location.search).get('room');
        if (room !== null) e.detail.parameters.room = room;
    });

    function currentRoom() {
        return new URLSearchParams(location.search).get('room');
    }

    function railItems() {
        return document.querySelectorAll('.room-item');
    }

    function highlightRoom() {
        var want = currentRoom() || '0';
        railItems().forEach(function (el) {
            el.classList.toggle('active', el.dataset.room === want);
        });
    }

    function selectRoom(id) {
        var url = new URL(location.href);
        url.searchParams.set('room', String(id));
        history.replaceState(null, '', url);
        highlightRoom();                       // 立刻高亮，不等下一帧
        var poller = document.getElementById('poller');
        if (poller) htmx.trigger(poller, 'dash:refresh');
    }

    document.addEventListener('click', function (e) {
        var el = e.target.closest && e.target.closest('.room-item');
        if (!el) return;
        e.preventDefault();
        selectRoom(el.dataset.room);
    });

    // ============================================================
    // 日志：/api/logs 是带游标的增量接口，保留 Minimal API 那一侧。
    // 不走 morph —— 日志是追加式的，整块替换会把滚动位置和用户正在读的行一起搅乱。
    // ============================================================

    var LOG_MAX = 600;
    var log = { seq: 0, paused: false, level: 'ALL', query: '', rows: [], hidden: 0 };

    function logBox() { return document.getElementById('logLines'); }

    function passes(r) {
        if (log.level === 'ERROR') {
            if (r.level !== 'ERROR' && r.level !== 'FATAL') return false;
        } else if (log.level !== 'ALL' && r.level !== log.level) {
            return false;
        }
        if (log.query) {
            var hay = (r.time + ' ' + r.level + ' ' + r.logger + ' ' + r.message).toLowerCase();
            if (hay.indexOf(log.query) === -1) return false;
        }
        return true;
    }

    /** 全部用 textContent —— 玩家内容永远不经过 innerHTML。 */
    function lineEl(r) {
        var row = document.createElement('div');
        row.className = 'logline lv-' + r.level;

        var t = document.createElement('span');
        t.className = 't';
        t.textContent = r.time;

        var m = document.createElement('span');
        m.className = 'm';
        var g = document.createElement('span');
        g.className = 'lg';
        g.textContent = r.logger + ' ';
        m.appendChild(g);
        m.appendChild(document.createTextNode(r.message));

        row.appendChild(t);
        row.appendChild(m);
        return row;
    }

    function renderAll() {
        var box = logBox();
        if (!box) return;
        var frag = document.createDocumentFragment();
        log.hidden = 0;
        for (var i = 0; i < log.rows.length; i++) {
            if (passes(log.rows[i])) frag.appendChild(lineEl(log.rows[i]));
            else log.hidden++;
        }
        box.textContent = '';
        box.appendChild(frag);
        box.scrollTop = box.scrollHeight;
        updateLogFoot();
    }

    function appendLogs(rows) {
        var box = logBox();
        if (!box) return;
        var nearBottom = box.scrollHeight - box.scrollTop - box.clientHeight < 40;
        var frag = document.createDocumentFragment();
        for (var i = 0; i < rows.length; i++) {
            log.rows.push(rows[i]);
            if (passes(rows[i])) frag.appendChild(lineEl(rows[i]));
            else log.hidden++;
        }
        if (log.rows.length > LOG_MAX) log.rows.splice(0, log.rows.length - LOG_MAX);
        box.appendChild(frag);
        while (box.childElementCount > LOG_MAX) box.removeChild(box.firstChild);
        if (!log.paused && nearBottom) box.scrollTop = box.scrollHeight;
        updateLogFoot();
    }

    function updateLogFoot() {
        var status = document.getElementById('logStatus');
        if (status) status.textContent = log.paused ? '已暂停' : '已连接';
        var note = document.getElementById('logNote');
        if (note) {
            var parts = [];
            if (log.hidden > 0) parts.push('已过滤 ' + log.hidden + ' 条');
            parts.push('滚动缓冲 ' + log.rows.length + ' / ' + LOG_MAX);
            note.textContent = parts.join(' · ');
        }
    }

    function setLogStatus(text) {
        var status = document.getElementById('logStatus');
        if (status) status.textContent = text;
    }

    function pollLogs() {
        if (log.paused) return;
    fetch('/api/v1/logs?after=' + log.seq + '&limit=600')
            .then(function (r) { return r.json(); })
            .then(function (p) {
                // reset = 请求的位置已被环形缓冲淘汰。服务端返回的已经是它保留的最老一段，
                // 所以清空后直接追加，不要重头再拉。
                if (p.reset) {
                    log.rows = []; log.hidden = 0;
                    var box = logBox();
                    if (box) box.textContent = '';
                }
                if (p.rows && p.rows.length) appendLogs(p.rows);
                log.seq = p.nextSeq;
                updateLogFoot();
            })
            .catch(function () { setLogStatus('无法连接'); });
    }

    function togglePause() {
        log.paused = !log.paused;
        var btn = document.getElementById('logPause');
        if (btn) {
            btn.classList.toggle('on', log.paused);
            btn.textContent = log.paused ? '继续' : '暂停';
        }
        if (!log.paused) pollLogs();
        updateLogFoot();
    }

    var pauseBtn = document.getElementById('logPause');
    if (pauseBtn) pauseBtn.addEventListener('click', togglePause);

    var levels = document.getElementById('logLevels');
    if (levels) {
        levels.addEventListener('click', function (e) {
            var b = e.target.closest && e.target.closest('button[data-lv]');
            if (!b) return;
            log.level = b.dataset.lv;
            levels.querySelectorAll('button').forEach(function (x) {
                x.classList.toggle('on', x === b);
            });
            renderAll();
        });
    }

    var search = document.getElementById('logSearch');
    if (search) {
        search.addEventListener('input', function () {
            log.query = this.value.trim().toLowerCase();
            renderAll();
        });
    }

    // ============================================================
    // 键盘。快捷键要能被发现，所以左栏直接印着数字，底部不再藏暗号。
    // ============================================================

    document.addEventListener('keydown', function (e) {
        var t = e.target;
        if (t && (t.tagName === 'INPUT' || t.tagName === 'TEXTAREA' || t.isContentEditable)) {
            if (e.key === 'Escape') t.blur();
            return;
        }
        if (e.ctrlKey || e.altKey || e.metaKey) return;

        if (e.key === 'l' || e.key === 'L') { togglePause(); return; }
        if (e.key === '/') {
            e.preventDefault();
            if (search) search.focus();
            return;
        }
        if (e.key === 'Escape') { selectRoom(0); return; }
        if (e.key >= '1' && e.key <= '9') {
            var items = railItems();
            var idx = parseInt(e.key, 10) - 1;
            if (items[idx]) { e.preventDefault(); selectRoom(items[idx].dataset.room); }
        }
    });

    // ============================================================
    // morph 会抹掉浏览器自己的状态，这里在换之前存档、换之后还原。
    // ============================================================

    var chatFollow = true;

    function chatEl() { return document.getElementById('chatScroll'); }

    function nearBottom(el) {
        return !el || el.scrollHeight - el.scrollTop - el.clientHeight < 40;
    }

    function updateChatJump() {
        var btn = document.getElementById('chatJump');
        var el = chatEl();
        if (!btn) return;
        btn.hidden = nearBottom(el);
    }

    var openBeforeSwap = {};
    var roomBeforeSwap = null;

    document.body.addEventListener('htmx:beforeSwap', function () {
        var el = chatEl();
        chatFollow = nearBottom(el);
        roomBeforeSwap = currentRoom();
        openBeforeSwap = {};
        document.querySelectorAll('details.conn[open]').forEach(function (d) {
            openBeforeSwap[d.dataset.pid] = 1;
        });
    });

    document.body.addEventListener('htmx:afterSettle', function () {
        var el = chatEl();
        if (el && chatFollow) el.scrollTop = el.scrollHeight;

        // 换了房间就不能照搬展开状态：pid 在新房间里指的是另一个人。
        if (currentRoom() === roomBeforeSwap) {
            document.querySelectorAll('details.conn').forEach(function (d) {
                if (openBeforeSwap[d.dataset.pid]) d.open = true;
            });
        }
        highlightRoom();
        updateChatJump();
    });

    document.addEventListener('scroll', function (e) {
        if (e.target && e.target.id === 'chatScroll') {
            chatFollow = nearBottom(e.target);
            updateChatJump();
        }
    }, true);

    document.addEventListener('click', function (e) {
        if (e.target && e.target.id === 'chatJump') {
            var el = chatEl();
            if (el) el.scrollTop = el.scrollHeight;
            chatFollow = true;
            updateChatJump();
        }
    });

    // ============================================================
    // 面板自己失联时要说出来，否则服务端渲染的面板在服务器挂掉后只会静默停留。
    // ============================================================

    function setOffline(on) {
        var banner = document.getElementById('offline');
        if (banner) banner.hidden = !on;
    }

    document.body.addEventListener('htmx:afterRequest', function (e) {
        if (e.detail && e.detail.elt && e.detail.elt.id === 'poller') setOffline(!e.detail.successful);
    });

    // ============================================================
    // 启动
    // ============================================================

    highlightRoom();
    var el = chatEl();
    if (el) el.scrollTop = el.scrollHeight;
    updateChatJump();
    updateLogFoot();
    pollLogs();
    setInterval(pollLogs, 1000);

})();
