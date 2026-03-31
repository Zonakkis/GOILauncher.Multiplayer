import { useState } from 'react';
import { Play, Settings } from 'lucide-react';

export default function MainMenu() {
  const [activeTab, setActiveTab] = useState<'connect' | 'settings'>('connect');
  const [playerName, setPlayerName] = useState('Player');
  const [serverIp, setServerIp] = useState('127.0.0.1:7777');

  const handleConnect = () => {
    // 当点击连接时，调用挂载在window上的Unity方法
    if (window.unityConnect) {
      window.unityConnect(serverIp, playerName);
    } else {
      console.log('Connecting to', serverIp, 'as', playerName);
    }
  };

  return (
    <div className="goi-card">
      <div className="goi-header">
        <h2 className="goi-title">GOI Multiplayer</h2>
        <p className="goi-desc">Welcome to the Multiplayer Mod</p>
      </div>

      <div className="goi-tabs">
        <button 
          className={`goi-tab-btn ${activeTab === 'connect' ? 'active' : ''}`}
          onClick={() => setActiveTab('connect')}
        >
          <Play />
          连接
        </button>
        <button 
          className={`goi-tab-btn ${activeTab === 'settings' ? 'active' : ''}`}
          onClick={() => setActiveTab('settings')}
        >
          <Settings />
          设置
        </button>
      </div>

      <div className="goi-content">
        {activeTab === 'connect' && (
          <div>
            <div className="goi-form-group">
              <label htmlFor="player-name" className="goi-label">玩家名称 (Player Name)</label>
              <input 
                id="player-name" 
                type="text"
                className="goi-input"
                value={playerName}
                onChange={(e) => setPlayerName(e.target.value)}
              />
            </div>
            
            <div className="goi-form-group">
              <label htmlFor="server-ip" className="goi-label">服务器 IP (IP:Port)</label>
              <input 
                id="server-ip"
                type="text"
                className="goi-input"
                value={serverIp}
                onChange={(e) => setServerIp(e.target.value)}
                placeholder="127.0.0.1:7777"
              />
            </div>

            <button className="goi-button-primary" onClick={handleConnect}>
              连接服务器
            </button>
          </div>
        )}

        {activeTab === 'settings' && (
          <div className="text-center text-muted">
            联机设置选项预留位...
            <br/><br/>
            (如：物理碰撞开关、幽灵模式等)
          </div>
        )}
      </div>
    </div>
  );
}
