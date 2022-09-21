using LoreMaster.Enums;
using System.Collections.Generic;

namespace LoreMaster.SaveManagement;

/// <summary>
/// Class for local save data
/// </summary>
public class LoreMasterLocalSaveData
{
    /// <summary>
    /// Gets or sets the tags of the powers.
    /// </summary>
    public Dictionary<string, PowerTag> Tags { get; set; } = new();

    /// <summary>
    /// Get or set the acquired powers key.
    /// </summary>
    public List<string> AcquiredPowersKeys { get; set; } = new();

    /// <summary>
    /// Gets or sets power specific data.
    /// </summary>
    public LocalPowerSaveData PowerData { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the player can read lore tablets. (Rando only)
    /// </summary>
    public bool HasReadAbility { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates, if the player can read lore tablets. (Rando only)
    /// </summary>
    public bool HasListenAbility { get; set; }
}
