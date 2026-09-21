import { LockKeyhole } from "lucide-react";
import { ScrollArea } from "@/components/ui/scroll-area";
import type { RoomItemView } from "@/lib/dashboard";
import { cn } from "@/lib/utils";

interface RoomRailProps {
  rooms: RoomItemView[];
  onSelect: (id: number) => void;
}

export function RoomRail({ rooms, onSelect }: RoomRailProps) {
  return (
    <aside className="room-rail">
      <div className="panel-heading room-rail-heading">
        <span>房间</span>
        <span className="count-badge">{rooms.length}</span>
      </div>

      <ScrollArea className="min-h-0 flex-1">
        {rooms.length === 0 ? (
          <div className="rail-empty">没有房间。服务器启动后这里会出现大厅。</div>
        ) : (
          <nav aria-label="房间列表" className="room-list">
            {rooms.map((room) => (
              <button
                key={room.id}
                type="button"
                className={cn("room-item", room.isSelected && "room-item-active", room.isEmpty && "room-item-empty", room.isUnassigned && "room-item-unassigned")}
                aria-current={room.isSelected ? "page" : undefined}
                onClick={() => onSelect(room.id)}
                title={`切换到 ${room.name}`}
              >
                <span className="room-item-main">
                  <span className="room-name">{room.name}</span>
                  {room.hasPassword ? <LockKeyhole className="room-lock" aria-label="需要密码" /> : null}
                  <span className="room-capacity">{room.capacityText}</span>
                </span>
                <span className="room-subtitle">
                  {room.isUnassigned
                    ? "尚未入房 / 握手中"
                    : room.isLobby
                      ? "公共大厅"
                      : room.ownerName
                        ? `房主 ${room.ownerName}`
                        : "无房主"}
                </span>
              </button>
            ))}
          </nav>
        )}
      </ScrollArea>
    </aside>
  );
}
