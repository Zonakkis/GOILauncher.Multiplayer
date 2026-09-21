import { describe, expect, it } from "vitest";
import type { ObservationSnapshotDto } from "@/api/contracts";
import {
  buildDashboard,
  capacityText,
  classifyLatency,
  formatClockAtOffset,
  formatBytes,
  formatCount,
  formatTimeZoneOffset,
  formatUptime,
  isLossWarn,
  lossPercent,
} from "@/lib/dashboard";

function snapshot(): ObservationSnapshotDto {
  return {
    schemaVersion: 1,
    generatedAt: "2026-09-21T05:00:00Z",
    server: {
      isRunning: true,
      startedAt: "2026-09-21T04:00:00Z",
      uptimeSeconds: 3600,
      pollCount: 10,
      maxPollGapMs: 17,
      unattributedNetworkErrors: 1,
      gamePort: 9027,
      utcOffsetMinutes: 480,
    },
    traffic: {
      packetsSent: 400,
      packetsReceived: 300,
      bytesSent: 16_000,
      bytesReceived: 12_000,
      packetLoss: 4,
      packetLossPercent: 1,
    },
    rooms: [
      { id: 3, name: "room-b", isLobby: false, hasPassword: false, playerCount: 1, maxPlayers: 8, ownerPlayerId: 2, ownerName: "b" },
      { id: 0, name: "大厅", isLobby: true, hasPassword: false, playerCount: 1, maxPlayers: 0, ownerPlayerId: null, ownerName: null },
      { id: 2, name: "room-a", isLobby: false, hasPassword: true, playerCount: 0, maxPlayers: 0, ownerPlayerId: null, ownerName: null },
    ],
    connections: [
      { playerId: 2, name: "b", platform: "PC", hasHandshaked: true, isInGame: true, roomId: 3, endPoint: "127.0.0.1:2", latencyMs: 301, traffic: { packetsSent: 100, packetsReceived: 98, bytesSent: 1, bytesReceived: 1, packetLoss: 2, packetLossPercent: 2 }, networkErrorCount: 1 },
      { playerId: 1, name: "a", platform: "Android", hasHandshaked: true, isInGame: false, roomId: 0, endPoint: "127.0.0.1:1", latencyMs: 20, traffic: null, networkErrorCount: 0 },
      { playerId: 3, name: null, platform: null, hasHandshaked: false, isInGame: false, roomId: null, endPoint: null, latencyMs: null, traffic: null, networkErrorCount: 0 },
    ],
    chat: {
      "0": [{ timestamp: "2026-09-21T04:59:00Z", playerId: 1, playerName: "a", content: "hello" }],
      "3": [],
    },
  };
}

describe("dashboard rules", () => {
  it("classifies latency and loss", () => {
    expect(classifyLatency(null)).toBe("unknown");
    expect(classifyLatency(150)).toBe("ok");
    expect(classifyLatency(151)).toBe("warn");
    expect(classifyLatency(300)).toBe("bad");
    expect(lossPercent({ packetsSent: 99, packetsReceived: 1, bytesSent: 0, bytesReceived: 0, packetLoss: 0, packetLossPercent: 0 })).toBeNull();
    expect(lossPercent({ packetsSent: 100, packetsReceived: 99, bytesSent: 0, bytesReceived: 0, packetLoss: 1, packetLossPercent: 1 })).toBeCloseTo(1);
    expect(isLossWarn(1)).toBe(false);
    expect(isLossWarn(1.01)).toBe(true);
  });

  it("formats server-local values", () => {
    expect(formatUptime(93_784)).toBe("26:03:04");
    expect(formatCount(1_234_567)).toBe("1.2M");
    expect(formatBytes(999)).toBe("999 B");
    expect(formatBytes(1536)).toBe("1.5 KiB");
    expect(formatBytes(2 * 1024 * 1024)).toBe("2.0 MiB");
    expect(capacityText(2, null)).toBe("2/∞");
    expect(formatTimeZoneOffset(480)).toBe("UTC+08:00");
    expect(formatClockAtOffset("2026-09-21T05:00:00Z", 480)).toBe("13:00:00");
  });

  it("orders rooms and falls back to lobby", () => {
    const view = buildDashboard(snapshot(), 999);
    expect(view.rail.map((room) => room.id)).toEqual([0, 2, 3, -1]);
    expect(view.selected?.id).toBe(0);
    expect(view.chat[0]?.time).toBe("12:59:00");
    expect(view.status.totalErrors).toBe(2);
    expect(view.status.serverNow).toBe("13:00:00");
    expect(view.status.traffic.bytesSent).toBe("15.6 KiB");
    expect(view.status.traffic.totalPackets).toBe("700");
  });

  it("keeps unassigned connections in their pseudo room", () => {
    const view = buildDashboard(snapshot(), -1);
    expect(view.selected?.isUnassigned).toBe(true);
    expect(view.connections.map((connection) => connection.displayName)).toEqual(["（握手中）"]);
  });
});
