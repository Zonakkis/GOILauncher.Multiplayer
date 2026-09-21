import { useEffect, useRef } from "react";
import { Pause, Play, Search, ScrollText } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import type { LogEntryDto } from "@/api/contracts";
import type { LogLevel } from "@/hooks/use-logs";
import { cn } from "@/lib/utils";

interface LogPanelProps {
  rows: LogEntryDto[];
  hiddenCount: number;
  totalCount: number;
  paused: boolean;
  level: LogLevel;
  query: string;
  connectionState: "connecting" | "connected" | "error";
  setLevel: (level: LogLevel) => void;
  setQuery: (query: string) => void;
  togglePause: () => void;
}

export function LogPanel({
  rows,
  hiddenCount,
  totalCount,
  paused,
  level,
  query,
  connectionState,
  setLevel,
  setQuery,
  togglePause,
}: LogPanelProps) {
  const viewportRef = useRef<HTMLDivElement>(null);
  const following = useRef(true);

  useEffect(() => {
    const viewport = viewportRef.current;
    if (viewport && following.current && !paused) viewport.scrollTop = viewport.scrollHeight;
  }, [paused, rows.length]);

  const status = paused
    ? "已暂停"
    : connectionState === "connected"
      ? "已连接"
      : connectionState === "error"
        ? "无法连接"
        : "连接中…";

  return (
    <aside className="log-panel">
      <div className="log-header">
        <span className="heading-with-icon"><ScrollText className="size-3.5" />日志</span>
        <ToggleGroup
          type="single"
          value={level}
          onValueChange={(value) => {
            if (value) setLevel(value as LogLevel);
          }}
          aria-label="日志级别"
        >
          <ToggleGroupItem value="ALL">全部</ToggleGroupItem>
          <ToggleGroupItem value="INFO">信息</ToggleGroupItem>
          <ToggleGroupItem value="WARN">警告</ToggleGroupItem>
          <ToggleGroupItem value="ERROR">错误</ToggleGroupItem>
        </ToggleGroup>
        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              type="button"
              variant={paused ? "secondary" : "ghost"}
              size="icon-sm"
              onClick={togglePause}
              aria-label={paused ? "继续日志" : "暂停日志"}
            >
              {paused ? <Play /> : <Pause />}
            </Button>
          </TooltipTrigger>
          <TooltipContent>{paused ? "继续日志" : "暂停日志"}</TooltipContent>
        </Tooltip>
      </div>

      <div className="log-search-wrap">
        <Search className="log-search-icon" aria-hidden="true" />
        <Input
          type="search"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder="过滤文本"
          aria-label="过滤日志文本"
          autoComplete="off"
          spellCheck={false}
          className="pl-8"
        />
      </div>

      <div
        ref={viewportRef}
        className="log-scroll"
        onScroll={() => {
          const viewport = viewportRef.current;
          if (!viewport) return;
          following.current = viewport.scrollHeight - viewport.scrollTop - viewport.clientHeight < 40;
        }}
      >
        {rows.map((row) => (
          <div key={row.seq} className={cn("log-line", `log-level-${row.level.toLowerCase()}`)}>
            <span className="log-time">{row.time}</span>
            <span className="log-message"><span>{row.logger}</span>{row.message}</span>
          </div>
        ))}
      </div>

      <div className="log-footer">
        <span>{status}</span>
        <span>
          {hiddenCount > 0 ? `已过滤 ${hiddenCount} 条 · ` : ""}滚动缓冲 {totalCount} / 600
        </span>
      </div>
    </aside>
  );
}
