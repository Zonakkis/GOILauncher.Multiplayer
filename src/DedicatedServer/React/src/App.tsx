import { useQuery } from "@tanstack/react-query";
import { LoaderCircle } from "lucide-react";
import { fetchSnapshot } from "@/api/client";
import { ChatPanel } from "@/components/dashboard/chat-panel";
import { ConnectionPanel } from "@/components/dashboard/connection-panel";
import { LogPanel } from "@/components/dashboard/log-panel";
import { OfflineBanner } from "@/components/dashboard/offline-banner";
import { RoomRail } from "@/components/dashboard/room-rail";
import { TopBar } from "@/components/dashboard/top-bar";
import { useLogs } from "@/hooks/use-logs";
import { useRoomSelection } from "@/hooks/use-room-selection";
import { buildDashboard } from "@/lib/dashboard";

export default function App() {
  const { roomId, selectRoom } = useRoomSelection();
  const logs = useLogs();

  const snapshotQuery = useQuery({
    queryKey: ["observation-snapshot"],
    queryFn: ({ signal }) => fetchSnapshot(signal),
    refetchInterval: 1000,
    retry: 1,
  });

  const dashboard = snapshotQuery.data ? buildDashboard(snapshotQuery.data, roomId) : null;

  return (
    <>
      {snapshotQuery.isError ? <OfflineBanner /> : null}
      <div className="app-shell">
        <TopBar status={dashboard?.status ?? null} />
        <RoomRail rooms={dashboard?.rail ?? []} onSelect={selectRoom} />

        <main className="dashboard-main">
          {!snapshotQuery.data ? (
            <div className="loading-state">
              <LoaderCircle className="loading-icon" />
              <span>{snapshotQuery.isError ? "等待服务器恢复…" : "正在载入观测快照…"}</span>
            </div>
          ) : (
            <>
              <ConnectionPanel
                selected={dashboard?.selected ?? null}
                connections={dashboard?.connections ?? []}
                isRunning={snapshotQuery.data.server.isRunning}
              />
              {dashboard?.selected ? (
                <ChatPanel
                  selectedRoomId={dashboard.selected.id}
                  messages={dashboard.chat}
                  emptyText={dashboard.chatEmptyText}
                />
              ) : null}
            </>
          )}
        </main>

        <LogPanel
          rows={logs.visibleRows}
          hiddenCount={logs.hiddenCount}
          totalCount={logs.rows.length}
          paused={logs.paused}
          level={logs.level}
          query={logs.query}
          connectionState={logs.connectionState}
          setLevel={logs.setLevel}
          setQuery={logs.setQuery}
          togglePause={logs.togglePause}
        />
      </div>
    </>
  );
}
