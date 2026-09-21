import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { fetchLogs } from "@/api/client";
import type { LogEntryDto } from "@/api/contracts";

export const LOG_BUFFER_LIMIT = 600;
export type LogLevel = "ALL" | "INFO" | "WARN" | "ERROR";
export type LogConnectionState = "connecting" | "connected" | "error";

export function useLogs() {
  const [rows, setRows] = useState<LogEntryDto[]>([]);
  const [paused, setPaused] = useState(false);
  const [level, setLevel] = useState<LogLevel>("ALL");
  const [query, setQuery] = useState("");
  const [connectionState, setConnectionState] = useState<LogConnectionState>("connecting");
  const sequence = useRef(0);

  useEffect(() => {
    if (paused) return;

    const controller = new AbortController();
    let stopped = false;
    let timer: number | undefined;

    async function poll() {
      try {
        const batch = await fetchLogs(sequence.current, controller.signal);
        if (stopped) return;

        if (batch.reset) setRows([]);
        if (batch.rows.length > 0) {
          setRows((current) => {
            const base = batch.reset ? [] : current;
            return [...base, ...batch.rows].slice(-LOG_BUFFER_LIMIT);
          });
        }

        sequence.current = batch.nextSeq;
        setConnectionState("connected");
      } catch (error) {
        if (!stopped && !(error instanceof DOMException && error.name === "AbortError")) {
          setConnectionState("error");
        }
      } finally {
        if (!stopped) timer = window.setTimeout(poll, 1000);
      }
    }

    void poll();
    return () => {
      stopped = true;
      controller.abort();
      if (timer !== undefined) window.clearTimeout(timer);
    };
  }, [paused]);

  const visibleRows = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();
    return rows.filter((row) => {
      if (level === "ERROR") {
        if (row.level !== "ERROR" && row.level !== "FATAL") return false;
      } else if (level !== "ALL" && row.level !== level) {
        return false;
      }

      if (!normalizedQuery) return true;
      const haystack = `${row.time} ${row.level} ${row.logger} ${row.message}`.toLowerCase();
      return haystack.includes(normalizedQuery);
    });
  }, [level, query, rows]);

  const togglePause = useCallback(() => setPaused((current) => !current), []);

  return {
    rows,
    visibleRows,
    hiddenCount: rows.length - visibleRows.length,
    paused,
    level,
    query,
    connectionState,
    setLevel,
    setQuery,
    togglePause,
  };
}
