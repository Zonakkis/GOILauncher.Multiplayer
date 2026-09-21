import {
  OBSERVATION_SCHEMA_VERSION,
  type LogBatchDto,
  type ObservationSnapshotDto,
} from "@/api/contracts";

async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(path, {
    headers: { Accept: "application/json" },
    signal,
  });

  if (!response.ok) {
    throw new Error(`请求 ${path} 失败：HTTP ${response.status}`);
  }

  return response.json() as Promise<T>;
}

export async function fetchSnapshot(signal?: AbortSignal) {
  const snapshot = await getJson<ObservationSnapshotDto>("/api/v1/snapshot", signal);
  if (snapshot.schemaVersion !== OBSERVATION_SCHEMA_VERSION) {
    throw new Error(`不支持的观测接口版本：${snapshot.schemaVersion}`);
  }
  return snapshot;
}

export function fetchLogs(after: number, signal?: AbortSignal) {
  return getJson<LogBatchDto>(`/api/v1/logs?after=${after}&limit=600`, signal);
}
