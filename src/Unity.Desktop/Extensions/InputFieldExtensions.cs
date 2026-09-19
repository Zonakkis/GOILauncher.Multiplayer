using System.Globalization;
using GOILauncher.Multiplayer.Unity.Config;
using UniverseLib.UI.Models;

namespace GOILauncher.Multiplayer.Extensions
{
    /// <summary>
    /// Input field helpers shared by the pages. Reading a port lives here instead of once per page:
    /// the client page, the server page and the settings page all parse the same thing, so they must
    /// also agree on what counts as valid and on what the player is told when it is not.
    /// </summary>
    public static class InputFieldExtensions
    {
        public const string InvalidPortMessage = "\u7aef\u53e3\u5fc5\u987b\u662f 1-65535 \u4e4b\u95f4\u7684\u6570\u5b57";

        /// <summary>
        /// Permanently hides the placeholder child of a field that is allowed to stay blank. A blank
        /// name is the "not chosen yet" state, and an in-box hint makes an empty field look prefilled.
        /// </summary>
        public static void HidePlaceholder(this InputFieldRef input)
        {
            if (input == null)
                return;

            var placeholder = input.PlaceholderText;
            if (placeholder != null)
                placeholder.gameObject.SetActive(false);
        }

        /// <summary>
        /// Makes a field read-only without making it look broken: interactable keeps the text
        /// selectable and the background unchanged, while readOnly blocks every edit path
        /// (typing, paste, backspace) at the InputField level.
        /// </summary>
        public static void SetReadOnly(this InputFieldRef input, bool readOnly)
        {
            if (input == null)
                return;

            input.Component.readOnly = readOnly;
            input.Component.interactable = true;
        }

        /// <summary>
        /// Parses the field as a port, using <see cref="MultiplayerSettings.IsValidPort"/> as the range
        /// check so the UI never writes a value the settings would reject on the next load. On success
        /// the field is rewritten with the parsed value, so what the player sees is what was read.
        /// Reporting a failure is left to the caller, which owns the toast.
        /// </summary>
        public static bool TryReadPort(this InputFieldRef input, out int port)
        {
            port = 0;

            string text = input == null ? null : input.Text;
            if (text == null)
                return false;

            if (!int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out port))
                return false;

            if (!MultiplayerSettings.IsValidPort(port))
                return false;

            input.Text = FormatPort(port);
            return true;
        }

        /// <summary>
        /// The text form of a port, paired with <see cref="TryReadPort"/>: a value written into a field
        /// reads back unchanged whatever the machine's regional settings are.
        /// </summary>
        public static string FormatPort(int port)
        {
            return port.ToString(CultureInfo.InvariantCulture);
        }
    }
}
