using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices; // Required for DllImport
using System.Windows.Forms; // Required for Keys enum
using ExileCore.Shared.Helpers; // For Delay helper
using ExileCore.PoEMemory.Elements; // For Element class which represents UI elements.
using ExileCore.Core; // For Core.TheCore and Core.CurrentGame.

// Ensure your PartyManagerSettings is in the same namespace or accessible via a using directive.
using YourPluginNamespace; // Replace with your actual plugin's namespace

public class PartyManager
{
    // Reference to the plugin's settings, allowing access to configured values.
    private readonly PartyManagerSettings _settings;

    /// <summary>
    /// Initializes a new instance of the PartyManager class with provided settings.
    /// </summary>
    /// <param name="settings">The PartyManagerSettings object containing configuration for auto-invites.</param>
    public PartyManager(PartyManagerSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Retrieves a list of player names currently in the party by traversing the UI elements
    /// using ExileCore's <see cref="Element"/> structure.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of strings, where each string is a player's name in the current party.
    /// Returns an empty list if no players are found or if there's an error during UI traversal.</returns>
    private List<string> GetCurrentPartyList()
    {
        var partyMembers = new List<string>();
        try
        {
            // Access the main party UI element through ExileCore's IngameUi.
            // This path (Core.CurrentGame.IngameUi.PartyElement) refers to the UI element
            // that is the parent of all party member entries.
            // You might need to adjust this path depending on the exact UI structure in PoE.
            var partyRootElement = Core.CurrentGame.IngameUi.PartyElement;

            // The original structure implies specific nested children.
            // This translates to navigating through multiple levels of children.
            // Let's assume this path correctly leads to a list of individual player elements.
            var partyElementListContainer = partyRootElement?.GetChildAtIndex(0)?.GetChildAtIndex(0);
            var partyElementList = partyElementListContainer?.Children;


            // If the UI path is invalid or no elements are found, log a warning using ExileCore's logging.
            if (partyElementList == null || !partyElementList.Any())
            {
                LogWarning("Party UI elements not found or structure unexpected. Cannot retrieve current party list. Verify your ExileCore UI element paths for the party panel.");
                return partyMembers;
            }

            // Iterate through each detected party element (each representing a party member).
            foreach (var partyElement in partyElementList)
            {
                // Assuming the player's name is the text of the first child element
                // of each party member's UI entry.
                var playerNameElement = partyElement?.GetChildAtIndex(0); // This is likely the element holding the name
                var playerName = playerNameElement?.Text;

                // Add the player's name to the list if it's not null or empty.
                if (!string.IsNullOrEmpty(playerName))
                {
                    partyMembers.Add(playerName);
                }
            }
        }
        catch (Exception ex)
        {
            // Catch any exceptions during UI traversal and log the error using ExileCore's logging.
            LogError($"GetCurrentPartyList Error: {ex.Message}");
        }
        return partyMembers;
    }

    /// <summary>
    /// Compares the configured player name with the current party members and sends an invite
    /// if the player is not found in the party.
    /// </summary>
    public void CheckAndInvitePlayerIfNeeded()
    {
        // Get the player name from the settings.
        string playerToAutoInvite = _settings.PlayerToAutoInviteName.Value;

        // Ensure a player name has been configured for auto-inviting.
        if (string.IsNullOrEmpty(playerToAutoInvite))
        {
            LogWarning("No player name configured for auto-invite. Skipping invitation check.");
            return;
        }

        // Get the list of players currently in the party.
        List<string> currentPartyMembers = GetCurrentPartyList();

        bool foundInParty = false;
        // Iterate through the current party members to see if the auto-invite player is present.
        foreach (string memberName in currentPartyMembers)
        {
            // Perform a case-insensitive comparison to ensure flexibility with player names.
            if (string.Equals(playerToAutoInvite, memberName, StringComparison.OrdinalIgnoreCase))
            {
                foundInParty = true; // Player found in party.
                break; // No need to continue checking, player is already there.
            }
        }

        // Based on whether the player was found, either log that they are present
        // or attempt to send an invite.
        if (foundInParty)
        {
            LogInfo($"'{playerToAutoInvite}' is already in the party. No invite sent.");
        }
        else
        {
            LogInfo($"'{playerToAutoInvite}' not found in the party. Attempting to send an invite...");
            SendPartyInvite(playerToAutoInvite);
        }
    }

    /// <summary>
    /// Sends a party invite to a specified player by simulating keyboard input to type the command.
    /// </summary>
    /// <param name="playerName">The name of the player to invite.</param>
    private void SendPartyInvite(string playerName)
    {
        // Open chat window (usually 'Enter' key)
        Keyboard.KeyPress(Keys.Enter);
        Delay.Frame(); // Wait a frame for the chat to open

        string command = $"/invite {playerName}";

        // Type each character of the command
        foreach (char c in command)
        {
            // Convert char to Keys enum. This might need careful handling for special characters.
            // For simple alphanumeric, it often works by casting.
            // For more robust solutions, consider a mapping if complex characters are expected.
            // For common letters/numbers, direct cast often works.
            Keys key;
            if (Enum.TryParse(c.ToString(), true, out key)) // Attempt to parse character directly
            {
                Keyboard.KeyPress(key);
            }
            else if (char.IsDigit(c)) // Handle digits specifically if direct parse fails
            {
                key = (Keys)Enum.Parse(typeof(Keys), "_" + c.ToString(), true); // e.g., D1 for '1'
                Keyboard.KeyPress(key);
            }
            else if (c == '/') // Handle '/' specifically for commands
            {
                Keyboard.KeyPress(Keys.Oem2); // Keys.Oem2 is typically '/'
            }
            // Add more specific mappings here if needed for other symbols or international characters.
            else
            {
                LogError($"Failed to map character '{c}' to a keyboard key for typing.");
            }

            Delay.Frame(); // Small delay between characters for reliability
        }

        // Send the command by pressing 'Enter'
        Keyboard.KeyPress(Keys.Enter);
        Delay.Frames(2); // Wait a couple of frames for the command to process

        LogSuccess($"ExileCore: Simulated keyboard input to send party invite for '{playerName}'.");
    }

    // --- Logging Helper Methods using ExileCore.Logger ---
    // These methods now directly call ExileCore's logging system.
    private void LogError(string message)
    {
        ExileCore.Logger.LogError($"[PartyManager] {message}");
    }

    private void LogWarning(string message)
    {
        ExileCore.Logger.LogWarning($"[PartyManager] {message}");
    }

    private void LogInfo(string message)
    {
        ExileCore.Logger.LogInfo($"[PartyManager] {message}");
    }

    private void LogSuccess(string message)
    {
        // ExileCore.Logger.LogDebug is often used for success messages or detailed info.
        ExileCore.Logger.LogDebug($"[PartyManager] {message}");
    }
    // --- End Logging Helper Methods ---
}

/// <summary>
/// A static class to simulate keyboard input using Windows API calls.
/// </summary>
public static class Keyboard
{
    private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
    private const int KEYEVENTF_KEYUP = 0x0002;

    [DllImport("user32.dll")]
    private static extern uint keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    /// <summary>
    /// Simulates pressing down a key.
    /// </summary>
    /// <param name="key">The key to press down.</param>
    public static void KeyDown(Keys key)
    {
        keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
    }

    /// <summary>
    /// Simulates releasing a key.
    /// </summary>
    /// <param name="key">The key to release.</param>
    public static void KeyUp(Keys key)
    {
        keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
    }

    /// <summary>
    /// Simulates a full key press (down and up).
    /// </summary>
    /// <param name="key">The key to press.</param>
    public static void KeyPress(Keys key)
    {
        KeyDown(key);
        KeyUp(key);
    }
}
