using GOILauncher.Multiplayer.Server.Services;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity
{
    public class UnityServer : MonoBehaviour, IUnityServer
    {
        public IServerService ServerService { get; set; }
        public IPlayerService PlayerService { get; set; }
        public ChatService ChatService { get; set; }

        /// <summary>
        /// 读透服务端在不在跑。门面没有"关着但还在"这种状态：整张对象图随联机一起销毁，
        /// 所以这里不需要再 AND 一次开关。
        /// </summary>
        public bool IsRunning => ServerService.IsRunning;

        public void Start()
        {

        }

        public void Start(int port)
        {
            ServerService.Start(port);
        }

        public void Stop()
        {
            ServerService.Stop();
        }

        public void Update()
        {
            ServerService?.Poll();
        }
    }
}
