import { useCallback, useEffect, useState } from "react";

function readRoomId() {
  const raw = new URLSearchParams(window.location.search).get("room");
  if (raw === null || raw.trim() === "") return 0;
  const parsed = Number(raw);
  return Number.isInteger(parsed) ? parsed : 0;
}

export function useRoomSelection() {
  const [roomId, setRoomId] = useState(readRoomId);

  useEffect(() => {
    const onPopState = () => setRoomId(readRoomId());
    window.addEventListener("popstate", onPopState);
    return () => window.removeEventListener("popstate", onPopState);
  }, []);

  const selectRoom = useCallback((id: number) => {
    const url = new URL(window.location.href);
    url.searchParams.set("room", String(id));
    window.history.replaceState(null, "", url);
    setRoomId(id);
  }, []);

  return { roomId, selectRoom };
}
