import { useEffect, useState } from "react";
import { ChevronRight, Circle, Handshake, LockKeyhole, Play, Users } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible";
import type { ConnectionRowView, RoomItemView } from "@/lib/dashboard";
import { cn } from "@/lib/utils";

interface ConnectionPanelProps {
  selected: RoomItemView | null;
  connections: ConnectionRowView[];
  isRunning: boolean;
}

export function ConnectionPanel({ selected, connections, isRunning }: ConnectionPanelProps) {
  const [openPlayers, setOpenPlayers] = useState<Set<number>>(() => new Set());

  useEffect(() => {
    setOpenPlayers(new Set());
  }, [selected?.id]);

  if (!selected) {
    return (
      <section className="connection-panel empty-state">
        <div className="empty-state-mark">GOI</div>
        <h2>{isRunning ? "暂无房间" : "服务器未运行"}</h2>
        <p>
          {isRunning
            ? "服务器已经运行，等待客户端完成握手后这里会出现大厅。"
            : "观测快照是空的。服务器启动后这里会自动恢复，不需要刷新页面。"}
        </p>
      </section>
    );
  }

  return (
    <section className="connection-panel">
      <div className="room-summary">
        <div className="room-title-row">
          <h2>{selected.name}</h2>
          {selected.hasPassword ? (
            <Badge variant="warning">
              <LockKeyhole />
              有密码
            </Badge>
          ) : null}
          {selected.ownerName ? <Badge variant="outline">房主 {selected.ownerName}</Badge> : null}
          <Badge variant="secondary">{selected.capacityText} 人</Badge>
        </div>
        <span className="connection-count">
          <Users className="size-3.5" aria-hidden="true" />
          {connections.length} 个连接
        </span>
      </div>

      {connections.length === 0 ? (
        <div className="empty-inline">
          {selected.isUnassigned ? "没有未入房的连接。" : "本房间暂时没有人。"}
        </div>
      ) : (
        <div className="connection-scroll">
          <div className="connection-grid connection-header" aria-hidden="true">
            <span>状态</span>
            <span>玩家</span>
            <span>平台</span>
            <span>端点</span>
            <span className="align-right">延迟</span>
            <span className="align-right">丢包</span>
            <span className="align-right">错误</span>
          </div>

          {connections.map((connection) => {
            const open = openPlayers.has(connection.playerId);
            return (
              <Collapsible
                key={connection.playerId}
                open={open}
                onOpenChange={(nextOpen) => {
                  setOpenPlayers((current) => {
                    const next = new Set(current);
                    if (nextOpen) next.add(connection.playerId);
                    else next.delete(connection.playerId);
                    return next;
                  });
                }}
                className="connection-item"
              >
                <CollapsibleTrigger asChild>
                  <button type="button" className="connection-grid connection-row">
                    <span className="connection-state">
                      {!connection.hasHandshaked ? (
                        <span className="state-handshake">
                          <Handshake className="size-3.5" />
                          握手中
                        </span>
                      ) : connection.isInGame ? (
                        <Play className="state-in-game" aria-label="游戏中" />
                      ) : (
                        <Circle className="state-idle" aria-label="已连接，未进入游戏" />
                      )}
                    </span>
                    <span className="player-name" title={connection.displayName}>
                      {connection.displayName}<small>#{connection.playerId}</small>
                    </span>
                    <span className="muted-cell">{connection.platformLabel}</span>
                    <span className="endpoint-cell" title={connection.endPoint}>{connection.endPoint}</span>
                    <span className={cn("number-cell", `latency-${connection.latency}`)}>{connection.latencyText}</span>
                    <span className={cn("loss-cell", connection.lossWarn && "loss-warn")}>
                      {connection.lossPercent !== null ? (
                        <span className="loss-track"><i style={{ width: `${connection.lossBarPercent}%` }} /></span>
                      ) : null}
                      <span className="number-cell">{connection.lossText}</span>
                    </span>
                    <span className={cn("number-cell error-cell", connection.networkErrorCount > 0 && "error-cell-bad")}>
                      {connection.networkErrorCount > 0 ? connection.networkErrorCount : "-"}
                    </span>
                    <ChevronRight className={cn("row-chevron", open && "row-chevron-open")} aria-hidden="true" />
                  </button>
                </CollapsibleTrigger>

                <CollapsibleContent className="connection-details">
                  <dl>
                    <dt>玩家 ID</dt><dd>{connection.playerId}</dd>
                    <dt>端到端</dt><dd>{connection.endPoint}</dd>
                    <dt>延迟</dt><dd>{connection.latencyText}</dd>
                    <dt>发包 / 收包</dt><dd>{connection.packetsSent} / {connection.packetsReceived}</dd>
                    <dt>发送 / 接收字节</dt><dd>{connection.bytesSent} B / {connection.bytesReceived} B</dd>
                    <dt>丢包数 / 丢包率</dt><dd>{connection.packetLoss} / {connection.packetLossPercent}</dd>
                    <dt>错误计数</dt><dd>{connection.networkErrorCount}</dd>
                  </dl>
                  {!connection.hasStats ? (
                    <p className="detail-note">此连接还没有传输统计样本（LiteNetLib 统计未开启或刚连上）。</p>
                  ) : null}
                </CollapsibleContent>
              </Collapsible>
            );
          })}
        </div>
      )}
    </section>
  );
}
