import type {
  ChatMessageDto,
  ConnectionDto,
  ObservationSnapshotDto,
  RoomDto,
  TrafficDto,
} from "@/api/contracts";

export const LOBBY_ID = 0;
export const MIN_LOSS_SAMPLE = 100;
export const LATENCY_WARN_MS = 150;
export const LATENCY_BAD_MS = 300;
export const STALLED_THRESHOLD_MS = 1000;

export type LatencyLevel = "unknown" | "ok" | "warn" | "bad";

export interface StatusView {
  isRunning: boolean;
  isStalled: boolean;
  port: number;
  uptime: string;
  startedAt: string;
  serverNow: string;
  timeZoneLabel: string;
  pollGapMs: number;
  onlineCount: number;
  roomCount: number;
  totalErrors: number;
  traffic: TrafficView;
}

export interface TrafficView {
  bytesSent: string;
  bytesReceived: string;
  packetsSent: string;
  packetsReceived: string;
  totalPackets: string;
  packetLoss: string;
  packetLossPercent: string;
}

export interface RoomItemView {
  id: number;
  name: string;
  isLobby: boolean;
  isUnassigned: boolean;
  isSelected: boolean;
  hasPassword: boolean;
  ownerName: string | null;
  isEmpty: boolean;
  playerCount: number;
  capacityText: string;
}

export interface ConnectionRowView {
  playerId: number;
  displayName: string;
  hasHandshaked: boolean;
  isInGame: boolean;
  platformLabel: string;
  endPoint: string;
  latencyMs: number | null;
  latency: LatencyLevel;
  latencyText: string;
  lossPercent: number | null;
  lossText: string;
  lossWarn: boolean;
  lossBarPercent: number;
  hasStats: boolean;
  networkErrorCount: number;
  packetsSent: string;
  packetsReceived: string;
  bytesSent: string;
  bytesReceived: string;
  packetLoss: string;
  packetLossPercent: string;
}

export interface ChatMessageView {
  key: string;
  time: string;
  playerName: string;
  content: string;
}

export interface DashboardView {
  status: StatusView;
  rail: RoomItemView[];
  selected: RoomItemView | null;
  connections: ConnectionRowView[];
  chat: ChatMessageView[];
  chatEmptyText: string;
}

export function isStalled(maxPollGapMs: number) {
  return maxPollGapMs > STALLED_THRESHOLD_MS;
}

export function classifyLatency(milliseconds: number | null): LatencyLevel {
  if (milliseconds === null) return "unknown";
  if (milliseconds >= LATENCY_BAD_MS) return "bad";
  if (milliseconds > LATENCY_WARN_MS) return "warn";
  return "ok";
}

export function lossPercent(traffic: TrafficDto | null) {
  if (!traffic || traffic.packetsSent < MIN_LOSS_SAMPLE) return null;
  return ((traffic.packetsSent - traffic.packetsReceived) / traffic.packetsSent) * 100;
}

export function isLossWarn(percent: number | null) {
  return percent !== null && percent > 1;
}

export function platformLabel(platform: string | null) {
  switch (platform) {
    case "PC":
      return "PC";
    case "iOS":
      return "iOS";
    case "Android":
      return "安卓";
    default:
      return "未知";
  }
}

export function formatUptime(seconds: number | null) {
  if (seconds === null) return "-";
  const total = Math.max(0, Math.floor(seconds));
  const hours = Math.floor(total / 3600);
  const minutes = Math.floor((total % 3600) / 60);
  const remainingSeconds = total % 60;
  return `${hours}:${pad(minutes)}:${pad(remainingSeconds)}`;
}

export function formatCount(value: number) {
  if (value < 1000) return String(value);
  if (value < 1_000_000) return `${(value / 1000).toFixed(1)}k`;
  return `${(value / 1_000_000).toFixed(1)}M`;
}

export function formatBytes(value: number) {
  if (value < 1024) return `${value} B`;
  if (value < 1024 * 1024) return `${(value / 1024).toFixed(1)} KiB`;
  if (value < 1024 * 1024 * 1024) return `${(value / (1024 * 1024)).toFixed(1)} MiB`;
  return `${(value / (1024 * 1024 * 1024)).toFixed(1)} GiB`;
}

export function formatTimeZoneOffset(offsetMinutes: number) {
  const sign = offsetMinutes < 0 ? "-" : "+";
  const absolute = Math.abs(offsetMinutes);
  return `UTC${sign}${pad(Math.floor(absolute / 60))}:${pad(absolute % 60)}`;
}

export function formatClockAtOffset(value: string | null, offsetMinutes: number) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  const shifted = new Date(date.getTime() + offsetMinutes * 60_000);
  return `${pad(shifted.getUTCHours())}:${pad(shifted.getUTCMinutes())}:${pad(shifted.getUTCSeconds())}`;
}

export function formatChatTime(value: string, offsetMinutes: number) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  const shifted = new Date(date.getTime() + offsetMinutes * 60_000);
  return `${pad(shifted.getUTCHours())}:${pad(shifted.getUTCMinutes())}:${pad(shifted.getUTCSeconds())}`;
}

export function capacityText(playerCount: number, maxPlayers: number | null) {
  return `${playerCount}/${maxPlayers && maxPlayers > 0 ? maxPlayers : "∞"}`;
}

export function buildDashboard(snapshot: ObservationSnapshotDto, requestedRoom: number): DashboardView {
  const offset = snapshot.server.utcOffsetMinutes;
  const byRoom = groupConnections(snapshot.connections);
  const lobby = snapshot.rooms.find((room) => room.isLobby) ?? null;
  const customRooms = snapshot.rooms
    .filter((room) => !room.isLobby)
    .sort((left, right) => left.id - right.id);

  const rail: RoomItemView[] = [];
  if (lobby) rail.push(roomItem(lobby, byRoom));
  for (const room of customRooms) rail.push(roomItem(room, byRoom));

  const unassigned = byRoom.get(-1) ?? [];
  if (unassigned.length > 0) {
    rail.push({
      id: -1,
      name: "其他连接",
      isLobby: false,
      isUnassigned: true,
      isSelected: false,
      hasPassword: false,
      ownerName: null,
      isEmpty: false,
      playerCount: unassigned.length,
      capacityText: `${unassigned.length} 人`,
    });
  }

  const selected = rail.find((item) => item.id === requestedRoom)
    ?? rail.find((item) => item.isLobby)
    ?? null;
  if (selected) selected.isSelected = true;
  const selectedConnections = selected ? (byRoom.get(selected.id) ?? []) : [];
  const connections = selectedConnections
    .slice()
    .sort((left, right) => left.playerId - right.playerId)
    .map(connectionRow);

  const totalErrors = snapshot.connections.reduce(
    (total, connection) => total + connection.networkErrorCount,
    snapshot.server.unattributedNetworkErrors,
  );

  return {
    status: {
      isRunning: snapshot.server.isRunning,
      isStalled: isStalled(snapshot.server.maxPollGapMs),
      port: snapshot.server.gamePort,
      uptime: formatUptime(snapshot.server.uptimeSeconds),
      startedAt: formatClockAtOffset(snapshot.server.startedAt, offset),
      serverNow: formatClockAtOffset(snapshot.generatedAt, offset),
      timeZoneLabel: formatTimeZoneOffset(offset),
      pollGapMs: Math.round(snapshot.server.maxPollGapMs),
      onlineCount: snapshot.connections.length,
      roomCount: snapshot.rooms.length,
      totalErrors,
      traffic: {
        bytesSent: formatBytes(snapshot.traffic.bytesSent),
        bytesReceived: formatBytes(snapshot.traffic.bytesReceived),
        packetsSent: formatCount(snapshot.traffic.packetsSent),
        packetsReceived: formatCount(snapshot.traffic.packetsReceived),
        totalPackets: formatCount(snapshot.traffic.packetsSent + snapshot.traffic.packetsReceived),
        packetLoss: formatCount(snapshot.traffic.packetLoss),
        packetLossPercent: `${snapshot.traffic.packetLossPercent}%`,
      },
    },
    rail,
    selected,
    connections,
    chat: buildChat(snapshot, selected, offset),
    chatEmptyText: selected === null
      ? "服务器未运行。"
      : selected.isUnassigned
        ? "未入房的连接没有聊天频道。"
        : "（本房间暂无聊天）",
  };
}

function groupConnections(connections: ConnectionDto[]) {
  const grouped = new Map<number, ConnectionDto[]>();
  for (const connection of connections) {
    const roomId = connection.roomId ?? -1;
    const rows = grouped.get(roomId) ?? [];
    rows.push(connection);
    grouped.set(roomId, rows);
  }
  return grouped;
}

function roomItem(room: RoomDto, byRoom: Map<number, ConnectionDto[]>): RoomItemView {
  const actual = room.playerCount;
  return {
    id: room.id,
    name: room.name || "（无名房间）",
    isLobby: room.isLobby,
    isUnassigned: false,
    isSelected: false,
    hasPassword: room.hasPassword,
    ownerName: room.ownerName,
    isEmpty: actual === 0,
    playerCount: actual,
    capacityText: capacityText(actual, room.maxPlayers),
  };
}

function connectionRow(connection: ConnectionDto): ConnectionRowView {
  const traffic = connection.traffic;
  const loss = lossPercent(traffic);
  return {
    playerId: connection.playerId,
    displayName: connection.hasHandshaked
      ? (connection.name || "（未命名）")
      : "（握手中）",
    hasHandshaked: connection.hasHandshaked,
    isInGame: connection.isInGame,
    platformLabel: connection.hasHandshaked ? platformLabel(connection.platform) : "",
    endPoint: connection.endPoint || "-",
    latencyMs: connection.latencyMs,
    latency: classifyLatency(connection.latencyMs),
    latencyText: connection.latencyMs === null ? "-" : `${connection.latencyMs} ms`,
    lossPercent: loss,
    lossText: loss === null ? "-" : `${loss.toFixed(1)}%`,
    lossWarn: isLossWarn(loss),
    lossBarPercent: Math.max(0, Math.min(100, Math.round(loss ?? 0))),
    hasStats: traffic !== null,
    networkErrorCount: connection.networkErrorCount,
    packetsSent: traffic ? formatCount(traffic.packetsSent) : "-",
    packetsReceived: traffic ? formatCount(traffic.packetsReceived) : "-",
    bytesSent: traffic ? formatCount(traffic.bytesSent) : "-",
    bytesReceived: traffic ? formatCount(traffic.bytesReceived) : "-",
    packetLoss: traffic ? String(traffic.packetLoss) : "-",
    packetLossPercent: traffic ? `${traffic.packetLossPercent}%` : "-",
  };
}

function buildChat(
  snapshot: ObservationSnapshotDto,
  selected: RoomItemView | null,
  offsetMinutes: number,
): ChatMessageView[] {
  if (!selected || selected.isUnassigned) return [];
  const messages: ChatMessageDto[] = snapshot.chat[String(selected.id)] ?? [];
  return messages.slice(-200).map((message, index) => ({
    key: `${message.timestamp}-${message.playerId}-${index}`,
    time: formatChatTime(message.timestamp, offsetMinutes),
    playerName: message.playerName || "？",
    content: message.content || "",
  }));
}

function pad(value: number) {
  return String(value).padStart(2, "0");
}
