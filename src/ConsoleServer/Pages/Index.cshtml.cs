using ConsoleServer.Web;
using GOILauncher.Multiplayer.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConsoleServer.Web.Pages
{
    /// <summary>
    /// 面板只有一个页面。选中哪个房间是唯一的界面状态，放在查询串里而不是 JS 变量里——
    /// 这样刷新、书签、多开标签页都一致，服务端也不用猜客户端在想什么。
    /// </summary>
    public sealed class IndexModel : PageModel
    {
        private readonly IObservationService _observation;
        private readonly int _gamePort;

        public DashboardView Dashboard { get; private set; }

        public IndexModel(IObservationService observation, DashboardOptions options)
        {
            _observation = observation;
            _gamePort = options.GamePort;
        }

        public void OnGet(int? room) => Load(room);

        /// <summary>
        /// 1 秒一次的局部刷新，只吐 #dashboard 那一块。整块换掉、由 idiomorph 就地在原节点上
        /// 合并，所以不会丢选中、不会跳滚动位置、也不会闪。
        /// </summary>
        public IActionResult OnGetDashboard(int? room)
        {
            Load(room);
            return Partial("_Dashboard", Dashboard);
        }

        private void Load(int? room) =>
            Dashboard = DashboardProjection.Build(_observation.Snapshot, _gamePort, room);
    }
}
