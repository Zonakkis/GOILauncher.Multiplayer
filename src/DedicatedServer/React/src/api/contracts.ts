export const OBSERVATION_SCHEMA_VERSION = 1;

export interface ObservationSnapshotDto {
  schemaVersion: number;
  generatedAt: string;
  server: ServerStatusDto;
  rooms: RoomDto[];
  connections: ConnectionDto[];
  chat: Record<string, ChatMessageDto[]>;
}

export interface ServerStatusDto {
  isRunning: boolean;
  startedAt: string | null;
  uptimeSeconds: number | null;
  pollCount: number;
  maxPollGapMs: number;
  unattributedNetworkErrors: number;
  gamePort: number;
  utcOffsetMinutes: number;
}

export interface RoomDto {
  id: number;
  name: string | null;
  isLobby: boolean;
  hasPassword: boolean;
  playerCount: number;
  maxPlayers: number | null;
  ownerPlayerId: number | null;
  ownerName: string | null;
}

export interface ConnectionDto {
  playerId: number;
  name: string | null;
  platform: string | null;
  hasHandshaked: boolean;
  isInGame: boolean;
  roomId: number | null;
  endPoint: string | null;
  latencyMs: number | null;
  traffic: ConnectionTrafficDto | null;
  networkErrorCount: number;
}

export interface ConnectionTrafficDto {
  packetsSent: number;
  packetsReceived: number;
  bytesSent: number;
  bytesReceived: number;
  packetLoss: number;
  packetLossPercent: number;
}

export interface ChatMessageDto {
  timestamp: string;
  playerId: number;
  playerName: string | null;
  content: string | null;
}

export interface LogEntryDto {
  seq: number;
  timestamp: string;
  time: string;
  level: string;
  logger: string;
  message: string;
}

export interface LogBatchDto {
  rows: LogEntryDto[];
  nextSeq: number;
  reset: boolean;
}
