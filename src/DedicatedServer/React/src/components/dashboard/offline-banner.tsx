import { WifiOff } from "lucide-react";

export function OfflineBanner() {
  return (
    <div className="offline-banner" role="status" aria-live="polite">
      <WifiOff className="size-3.5" />
      面板与服务器失去联系，正在重试…
    </div>
  );
}
