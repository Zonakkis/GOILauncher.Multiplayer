using GOILauncher.Multiplayer.UI.Pages;
using UniverseLib.UI.Models;

namespace GOILauncher.Multiplayer.UI
{
    public partial class MultiplayerUI
    {
        private sealed class PageEntry
        {
            public PageEntry(MultiplayerPage id, string buttonText, IPage page)
            {
                Id = id;
                ButtonText = buttonText;
                Page = page;
            }

            public MultiplayerPage Id { get; }

            public string ButtonText { get; }

            public IPage Page { get; }

            public ButtonRef Button { get; set; }
        }
    }
}
