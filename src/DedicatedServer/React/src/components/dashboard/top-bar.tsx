import { Activity, Clock3, DoorOpen, HeartPulse, Network, TriangleAlert, Users } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { StatusView } from "@/lib/dashboard";
import { cn } from "@/lib/utils";

interface TopBarProps {
  status: StatusView | null;
}

export function TopBar({ status }: TopBarProps) {
  const state = !status?.isRunning ? "stopped" : status.isStalled ? "stalled" : "running";
  const stateText = !status?.isRunning ? "未运行" : status.isStalled ? "疑似卡顿" : "运行中";

  return (
    <header className={cn("top-bar", `top-bar-${state}`)}>
      <div className="brand-lockup">
        <span className="status-dot" />
        <span className="brand-name">GOILauncher.Multiplayer</span>
        <span className="server-state">{stateText}</span>
      </div>

      <div className="metrics-strip">
        <Metric icon={Network} value={status?.port ?? "-"} label="游戏端口" />
        <Metric icon={Users} value={status?.onlineCount ?? "-"} label="在线" />
        <Metric icon={DoorOpen} value={status?.roomCount ?? "-"} label="房间" />
        <Metric icon={Clock3} value={status?.uptime ?? "-"} label="运行时长" title={status ? `服务器启动于 ${status.startedAt}` : undefined} />
        <Metric
          icon={HeartPulse}
          value={status ? `${status.pollGapMs} ms` : "-"}
          label="心跳间隔"
          tone={status?.isStalled ? "bad" : "neutral"}
          title="最近一次 Poll 循环里间隔的最大值"
        />
        <Metric
          icon={TriangleAlert}
          value={status?.totalErrors ?? "-"}
          label="错误"
          tone={status && status.totalErrors > 0 ? "bad" : "ok"}
          title="无法归属到具体连接的错误 + 各连接的收发错误"
        />
      </div>

      <div className="top-bar-meta">
        <Badge variant="outline">只读</Badge>
        <span title="所有时间均为服务器本地时间">
          {status ? `${status.serverNow} · ${status.timeZoneLabel}` : "--:--:--"}
        </span>
      </div>
    </header>
  );
}

function Metric({
  icon: Icon,
  value,
  label,
  title,
  tone = "neutral",
}: {
  icon: typeof Activity;
  value: string | number;
  label: string;
  title?: string;
  tone?: "neutral" | "ok" | "bad";
}) {
  return (
    <div className="metric" title={title}>
      <Icon className="metric-icon" aria-hidden="true" />
      <span className={cn("metric-value", tone === "ok" && "metric-ok", tone === "bad" && "metric-bad")}>{value}</span>
      <span className="metric-label">{label}</span>
    </div>
  );
}
