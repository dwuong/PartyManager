using ExileCore.Shared.Interfaces; // For ISettings interface
using ExileCore.Shared.Nodes;    // For ToggleNode, TextNode, RangeNode, etc.

namespace PartyManager 
{
    /// <summary>
    /// Settings for the PartyManager plugin.
    /// This class defines configurable options that will appear in the ExileCore UI.
    /// </summary>
    public class PartyManagerSettings : ISettings
    {
        // Property required by ISettings interface. Controls the main enable/disable for the plugin.
        public ToggleNode Enable { get; set; } = new ToggleNode(false); // Default to disabled

        /// <summary>
        /// Gets or sets a value indicating whether the auto-invite feature is enabled.
        /// This will show as a checkbox in the ExileCore settings UI.
        /// </summary>
        public ToggleNode EnableAutoInvite { get; set; } = new ToggleNode(false); // Default to disabled

        /// <summary>
        /// Gets or sets the name of the player to automatically invite.
        /// This will show as a text input field in the ExileCore settings UI.
        /// </summary>
        public TextNode PlayerToAutoInviteName { get; set; } = new TextNode(""); // Default to empty string

        // You can add more settings here, for example:
        // public RangeNode<int> CheckIntervalMs { get; set; } = new RangeNode<int>(5000, 1000, 30000);
        // public HotkeyNode OpenChatHotkey { get; set; } = new HotkeyNode(System.Windows.Forms.Keys.Enter);
    }
}
