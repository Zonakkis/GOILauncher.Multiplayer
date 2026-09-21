import { useEffect, useRef, useState } from "react";
import { ArrowDown, MessageSquareText } from "lucide-react";
import { Button } from "@/components/ui/button";
import type { ChatMessageView } from "@/lib/dashboard";

interface ChatPanelProps {
  selectedRoomId: number | null;
  messages: ChatMessageView[];
  emptyText: string;
}

export function ChatPanel({ selectedRoomId, messages, emptyText }: ChatPanelProps) {
  const viewportRef = useRef<HTMLDivElement>(null);
  const [following, setFollowing] = useState(true);

  useEffect(() => {
    setFollowing(true);
  }, [selectedRoomId]);

  useEffect(() => {
    const viewport = viewportRef.current;
    if (viewport && following) viewport.scrollTop = viewport.scrollHeight;
  }, [following, messages.length]);

  const isNearBottom = () => {
    const viewport = viewportRef.current;
    if (!viewport) return true;
    return viewport.scrollHeight - viewport.scrollTop - viewport.clientHeight < 40;
  };

  return (
    <section className="chat-panel">
      <div className="panel-heading chat-heading">
        <span className="heading-with-icon"><MessageSquareText className="size-3.5" />聊天</span>
        <span>{messages.length} 条 · 仅内存保留 30 分钟</span>
      </div>
      <div
        ref={viewportRef}
        className="chat-scroll"
        onScroll={() => setFollowing(isNearBottom())}
      >
        {messages.length === 0 ? (
          <div className="empty-inline">{emptyText}</div>
        ) : (
          messages.map((message) => (
            <div className="chat-message" key={message.key}>
              <span className="chat-time">{message.time}</span>
              <span className="chat-name">{message.playerName}</span>
              <span className="chat-content">{message.content}</span>
            </div>
          ))
        )}
      </div>
      {!following ? (
        <Button
          type="button"
          size="sm"
          className="chat-jump"
          onClick={() => {
            const viewport = viewportRef.current;
            if (viewport) viewport.scrollTop = viewport.scrollHeight;
            setFollowing(true);
          }}
        >
          <ArrowDown /> 有新消息
        </Button>
      ) : null}
    </section>
  );
}
