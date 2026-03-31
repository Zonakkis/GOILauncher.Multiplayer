import { useEffect, useState } from 'react';
import MainMenu from './components/MainMenu';
import './App.css';

// 预留的组件占位，后续我们分别实现它们
function ChatBox() {
  return (
    <div className="chat-placeholder" style={{ padding: 20, color: 'white' }}>
      Chat Box UI Loaded
    </div>
  );
}

function PlayerList() {
  return (
    <div className="players-placeholder" style={{ padding: 20, color: 'white' }}>
      Player List UI Loaded
    </div>
  );
}

// 为Typescript扩充全局 Window 对象
declare global {
  interface Window {
    unityConnect?: (ip: string, playerName: string) => void;
  }
}

function App() {
  const [uiMode, setUiMode] = useState<string>('main');

  useEffect(() => {
    // 从 URL 参数中读取需要显示的模块模式
    // 比如：file:///path/to/index.html?ui=chat
    const params = new URLSearchParams(window.location.search);
    const mode = params.get('ui');
    
    // 如果没有参数，也允许通过 hash 来读取 (以便支持 index.html#chat)
    const hash = window.location.hash.replace('#', '');
    
    if (mode) {
      setUiMode(mode);
    } else if (hash) {
      setUiMode(hash);
    }
  }, []);

  // 根据启动参数，在不同的 Browser 实例中渲染完全独立的功能区
  if (uiMode === 'chat') {
    return <ChatBox />;
  }

  if (uiMode === 'players' || uiMode === 'playerlist') {
    return <PlayerList />;
  }

  // 默认返回主菜单
  return (
    <div className="overlay-container panel-interactive">
      <MainMenu />
    </div>
  );
}

export default App;
