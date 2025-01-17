using ItemChanger;
using ItemChanger.Internal.Menu;
using LoreMaster.Enums;
using LoreMaster.ItemChangerData.Other;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.Greenpath;
using LoreMaster.LorePowers.HowlingCliffs;
using LoreMaster.LorePowers.RestingGrounds;
using LoreMaster.Randomizer;
using System.Collections.Generic;
using System.Linq;

namespace LoreMaster.Manager;

/// <summary>
/// Manager for handling the lore related logic.
/// </summary>
internal class LoreManager
{
    #region Constructors

    public LoreManager() => Instance = this;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets that powers should display their custom text (if available)
    /// </summary>
    public bool UseCustomText { get; set; } = true;

    /// <summary>
    /// Gets or sets if hints should be displayed instead of clear descriptions.
    /// </summary>
    public bool UseHints { get; set; } = true;

    /// <summary>
    /// Gets or sets the value, that indicates if the player can read lore tablets. (Rando only)
    /// </summary>
    public bool CanRead { get; set; } = true;

    /// <summary>
    /// Gets or sets the value, that indicates if the player can listen to npc.
    /// </summary>
    public bool CanListen { get; set; } = true;

    /// <summary>
    /// Gets or sets the amount of joker scrolls, that the player can use to obtain a power of their choice.
    /// </summary>
    public int JokerScrolls { get; set; } = 3;

    /// <summary>
    /// Gets or sets the amount of cleansing scrolls, that the player can use to undo a twisted obtain power of their choice.
    /// </summary>
    public int CleansingScrolls { get; set; } = 3;

    /// <summary>
    /// Gets or sets the current stages of the travelling npc.
    /// </summary>
    public Dictionary<Traveller, TravellerData> Traveller { get; set; } = new();

    public static LoreManager Instance { get; set; }

    #endregion

    #region Event handler

    /// <summary>
    /// This is the main control, which determines which power is on.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="sheetTitle"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    internal string GetText(string key, string sheetTitle, string text)
    {
        if (key.Equals("LoreMaster"))
            return "Lore Powers";
        else if (key.Equals("TreasureCharts"))
            return "Treasure Charts";
        // Shows the location of the key for the more doors door (removes the preview part and the _1 at the and.
        else if (key.StartsWith("Master_Key_Preview-"))
            return "The Master Key whispers " + (key.Substring(19, key.Length - 21));
        key = ModifyKey(key);
        if (key.Equals("INV_NAME_SUPERDASH"))
        {
            bool hasDiamondDash = PowerManager.HasObtainedPower("MYLA");
            bool hasDiamondCore = PowerManager.HasObtainedPower("QUIRREL");

            if (hasDiamondDash && !hasDiamondCore)
                text = Properties.AdditionalText.CORELESS_DIAMOND_HEART_NAME;
            else if (!hasDiamondDash && hasDiamondCore)
                text = Properties.AdditionalText.SHELLLESS_DIAMOND_HEART_NAME;
            else if (hasDiamondCore && hasDiamondDash)
                text = Properties.AdditionalText.FULL_DIAMOND_HEART_NAME;
        }
        else if (key.Equals("INV_DESC_SUPERDASH"))
        {
            bool hasDiamondDash = PowerManager.HasObtainedPower("MYLA");
            bool hasDiamondCore = PowerManager.HasObtainedPower("QUIRREL");

            if (hasDiamondDash && !hasDiamondCore)
                text += Properties.AdditionalText.CORELESS_DIAMOND_HEART_DESCRIPTION;
            else if (!hasDiamondDash && hasDiamondCore)
                text += Properties.AdditionalText.SHELLLESS_DIAMOND_HEART_DESCRIPTION;
            else if (hasDiamondCore && hasDiamondDash)
                text += Properties.AdditionalText.FULL_DIAMOND_HEART_DESCRIPTION;
        }
        else if (key.Equals("FOUNTAIN_PLAQUE_DESC"))
        {
            PowerManager.GetPowerByKey(key, out Power fountain, false);
            if (fountain.StayTwisted)
                text += " [Cursed: " + fountain.PowerName + "] " + (UseHints ? fountain.TwistedHint : fountain.TwistedDescription);
            else
                text += " [" + fountain.PowerName + "] " + (UseHints ? fountain.Hint : fountain.Description);
        }
        else if (key.Contains("DREAMERS_INSPECT_RG"))
        {
            PowerManager.GetPowerByKey("DREAMERS_INSPECT_RG5", out Power dreamer, false);
            text += ((DreamBlessingPower)dreamer).GetExtraText(key);
        }
        else if (key.Equals("TISO_TOWN_GREET"))
            text += "<page>Hm... maybe I could teach you something. But not here... if we meet again.";
        else if (key.Equals("TISO_TOWN_REPEAT"))
            text = "I said not here. If you are not as weak as you look, we'll meet again.";
        else if (key.Contains("Menderbug_Warning"))
            text = Properties.AdditionalText.ResourceManager.GetString(key.Substring(0, key.Length - 2));
        else if (string.Equals(key, "Menderbug_Journal_1"))
            text = "Acquired Menderbug Journal entry.";
        else if (string.Equals(key, "Stag_Egg_Convo"))
            text = StagAdoptionPower.Instance.CanSpawnStag ? "Stag Egg" : "Broken Stag Egg";
        else if (string.Equals(key, "Stag_Egg_Desc_Convo"))
            text = StagAdoptionPower.Instance.CanSpawnStag ? "You can feel something moving inside, maybe knocking let it hatch?" : "The remains of an egg shell.";
        else if (key.Contains("Remove_Power_"))
            text = $"You lost {key.Substring(13, key.Length - 15)}";
        else if (string.Equals(key, "Coward_1"))
            text = "Only cowards retreat from battle...";
        else if (string.Equals(key, "Temple_Door"))
            text = "Want to enter the temple?";
        else if (string.Equals(key, "Elderbug_Preview") && RandomizerManager.PlayingRandomizer)
            text = GenerateElderbugPreview();
        else if (key.StartsWith("Elderbug_"))
        {
            if (key.EndsWith("Casual"))
                text = Properties.ElderbugDialog.ResourceManager.GetString(RandomizerManager.PlayingRandomizer ? "Elderbug_Casual_Randomizer" : "Elderbug_Casual_Normal");
            else if (key == "Elderbug_Reward_2")
                text = Properties.ElderbugDialog.ResourceManager.GetString("Elderbug_Gain_Listening");
            else if (key == "Elderbug_Met")
            {
                PlayerData.instance.SetBool(nameof(PlayerData.instance.metElderbug), true);
                text = Properties.ElderbugDialog.ResourceManager.GetString(key);
                if (RandomizerManager.PlayingRandomizer)
                    text += "<page>" + Properties.ElderbugDialog.ResourceManager.GetString("Elderbug_Randomizer");
                if (SettingManager.Instance.GameMode != GameMode.Normal || (RandomizerManager.PlayingRandomizer && (RandomizerManager.Settings.RandomizeElderbugRewards || RandomizerManager.Settings.DefineRefs)))
                {
                    SettingManager.Instance.ElderbugState++;
                    text += "<page>" + Properties.ElderbugDialog.ResourceManager.GetString("Elderbug_Extra_Intro") + "<page>" + Properties.ElderbugDialog.ResourceManager.GetString("Elderbug_Task_1");
                }
                else if (RandomizerManager.PlayingRandomizer)
                {
                    PowerManager.ControlState = PowerControlState.ToggleAccess;
                    text += "<page>" + Properties.ElderbugDialog.ResourceManager.GetString("Elderbug_Randomizer_Not_Randomized");
                    SettingManager.Instance.ElderbugState--;
                }
                else
                {
                    PowerManager.ControlState = PowerControlState.ToggleAccess;
                    text += "<page>" + Properties.ElderbugDialog.ResourceManager.GetString("Elderbug_Normal");
                    SettingManager.Instance.ElderbugState--;
                }
            }
            else if (key.EndsWith("Hint"))
                text = RollElderbugHint();
            else
                text = Properties.ElderbugDialog.ResourceManager.GetString(key);
            if (text == null)
                text = $"Hm? (Couldn't resolve: {key} (Report this to the mod developer.))";
        }
        else if (key.StartsWith("Treasure-") && RandomizerRequestModifier.TreasureLocation.Contains(key.Substring(0, key.Length - 2)))
        {
            key = key.Substring(0, key.Length - 2);
            try
            {
                if (ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(key) && ItemChanger.Internal.Ref.Settings.Placements[key].Items.Any())
                    text = "The compass whispers: " + ItemChanger.Internal.Ref.Settings.Placements[key].Items[0]?.name.Replace("_", " ").Replace("-", " ") + " is buried.";
            }
            catch
            {
                text = "The compass couldn't determine the buried treasure";
            }
        }
        else if (PowerManager.HasObtainedPower("QUEEN", true))
        {
            if (key.Equals("CHARM_NAME_12"))
                return "Queen's Thorns";
            else if (key.Equals("CHARM_DESC_12"))
                return text + "<br>Blessed by the white lady, which causes them to drain soul and sometimes energy from their victims. Leash out more agile.";
        }
        return text;
    }

    #endregion

    #region Methods

    private string RollElderbugHint()
    {
        List<string> viablePowerNames = PowerManager.GetAllPowers()
            .Except(PowerManager.ObtainedPowers)
            .Select(x => x.GetType().Name.Substring(0, x.GetType().Name.Length - 5))
            .ToList();
        if (!viablePowerNames.Any())
            return Properties.ElderbugDialog.Elderbug_Tip_WellFocused;
        return Properties.ElderbugDialog.ResourceManager.GetString("Elderbug_Tip_" + viablePowerNames[LoreMaster.Instance.Generator.Next(0, viablePowerNames.Count)]);
    }

    /// <summary>
    /// Modifies the language key, to keep consistancy between the lore keys (mostly for NPC).
    /// </summary>
    private string ModifyKey(string key)
    {
        if (key.Contains("BRETTA_DIARY"))
            key = "BRETTA";
        else if (key.Contains("MENDER_DIARY"))
            key = "MENDERBUG";
        else if (string.Equals(key, "HIVEQUEEN_TALK") || string.Equals(key, "HIVEQUEEN_REPEAT"))
            key = "HIVEQUEEN";
        else if (string.Equals(key, "JONI_TALK") || string.Equals(key, "JONI_REPEAT"))
            key = "JONI";
        else if (string.Equals(key, "POGGY_TALK") || string.Equals(key, "POGGY_REPEAT"))
            key = "POGGY";
        else if (string.Equals(key, "GRAVEDIGGER_TALK") || string.Equals(key, "GRAVEDIGGER_REPEAT"))
            key = "GRAVEDIGGER";
        else if (string.Equals(key, "GRASSHOPPER_TALK") || string.Equals(key, "GRASSHOPPER_REPEAT"))
            key = "GRASSHOPPER";
        else if (string.Equals(key, "MARISSA_TALK") || string.Equals(key, "MARISSA_REPEAT"))
            key = "MARISSA";
        else if (IsMidwife(key))
            key = "MIDWIFE";
        else if (IsBardoon(key))
            key = "BARDOON";
        else if (IsFlukeHermit(key))
            key = "FLUKE_HERMIT";
        else if (IsQueen(key))
            key = "QUEEN";
        else if (IsMaskMaker(key))
            key = "MASKMAKER";
        else if (IsWilloh(key))
            key = "WILLOH";
        else if (IsMyla(key))
            key = "MYLA";
        else if (IsQuirrel(key))
            key = "QUIRREL";
        else if (IsEmilitia(key))
            key = "EMILITIA";
        else if (IsMossProphet(key))
            key = "MOSSPROPHET";
        else if (string.Equals(key, "TISO_BENCH_GREET") || string.Equals(key, "TISO_BENCH_REPEAT"))
            key = "TISO";
        else if (string.Equals(key, "ZOTE_DEEPNEST_1") || string.Equals(key, "ZOTE_DEEPNEST_2"))
            key = "ZOTE";
        else if (string.Equals(key, "XUN_GRAVE_INSPECT2"))
            key = "XUN_GRAVE_INSPECT";
        return key;
    }

    internal string AddPowerData(Power power, string displayText, bool IsWarning = false)
    {
        if (power.Tag != PowerTag.Remove)
        {
            if (UseCustomText && !string.IsNullOrEmpty(power.CustomText))
                displayText = power.CustomText;
            if (power.StayTwisted)
            {
                displayText += "<br><color=#c034eb>[Cursed " + power.PowerName + "]</color>";
                displayText += "<br>" + (UseHints ? power.TwistedHint : power.TwistedDescription);
            }
            else
            {
                displayText += "<br>[" + power.PowerName + "]";
                displayText += "<br>" + (UseHints ? power.Hint : power.Description);
            }
        }
        if (IsWarning)
        {
            PowerManager.GetPowerByKey("POP", out Power popPower, false);
            if (popPower.Tag != PowerTag.Remove)
            {
                displayText += "<page>For those, that reveals the secret, awaits the power:";
                displayText += "<br>[" + popPower.PowerName + "] ";
                displayText += "<br>" + (UseHints ? popPower.Hint : popPower.Description);
            }
        }
        return displayText;
    }

    private string GenerateElderbugPreview()
    {
        if (!ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(LocationList.Elderbug_Reward_Prefix + "1"))
            return "You shouldn't be able to see this.";
        string tabletText = "Curious, what I have in store for you?<br>";

        if (ItemChanger.Internal.Ref.Settings.Placements[LocationList.Elderbug_Reward_Prefix + "1"].Items.All(x => x.IsObtained()))
            tabletText += "Any Spell - Obtained<br>";
        else
            tabletText += $"Any Spell - {ItemChanger.Internal.Ref.Settings.Placements[LocationList.Elderbug_Reward_Prefix + "1"].Items[0].UIDef.GetPreviewName()}<br>";

        int[] neededLore = new int[] { 5, 10, 15, 20, 30, 40, 50, 55 };
        for (int i = 2; i < 10; i++)
        {
            if (ItemChanger.Internal.Ref.Settings.Placements[LocationList.Elderbug_Reward_Prefix + "" + i].Items.All(x => x.IsObtained()))
                tabletText += $"{neededLore[i - 2]} Lore - Obtained";
            else
                tabletText += $"{neededLore[i - 2]} Lore - {ItemChanger.Internal.Ref.Settings.Placements[LocationList.Elderbug_Reward_Prefix + "" + i].Items[0].UIDef.GetPreviewName()}";

            if (i == 2 || i == 5 || i == 8)
                tabletText += "<page>";
            else
                tabletText += "<br>";
        }
        LoreMaster.Instance.Log("Tablet text is: "+tabletText);
        return tabletText;
    }

    #region NPC Dialogues

    private bool IsBardoon(string key)
    {
        return string.Equals(key, "BIGCAT_INTRO") || string.Equals(key, "BIGCAT_TALK_01")
            || string.Equals(key, "BIGCAT_TALK_02") || string.Equals(key, "BIGCAT_TALK_03")
            || string.Equals(key, "BIGCAT_TAIL_HIT") || string.Equals(key, "BIGCAT_KING_BRAND")
            || string.Equals(key, "BIGCAT_SHADECHARM") || string.Equals(key, "BIGCAT_REPEAT");
    }

    private bool IsMidwife(string key)
    {
        return string.Equals(key, "SPIDER_MEET") || string.Equals(key, "SPIDER_GREET")
            || string.Equals(key, "SPIDER_GREET2") || string.Equals(key, "SPIDER_REPEAT") || string.Equals(key, "MIDWIFE_WEAVERSONG");
    }

    private bool IsMaskMaker(string key)
    {
        return string.Equals(key, "MASK_MAKER_GREET") || string.Equals(key, "MASK_MAKER_REPEAT")
            || string.Equals(key, "MASK_MAKER_REPEAT2") || string.Equals(key, "MASK_MAKER_REPEAT3")
            || string.Equals(key, "MASK_MAKER_UNMASK") || string.Equals(key, "MASK_MAKER_UNMASK3")
            || string.Equals(key, "MASK_MAKER_UNMASK4") || string.Equals(key, "MASK_MAKER_UNMASK2") || string.Equals(key, "MASK_MAKER_UNMASK_REPEAT")
            || string.Equals(key, "MASKMAKER_GREET") || string.Equals(key, "MASKMAKER_REPEAT")
            || string.Equals(key, "MASKMAKER_REPEAT2") || string.Equals(key, "MASKMAKER_REPEAT3")
            || string.Equals(key, "MASKMAKER_UNMASK") || string.Equals(key, "MASKMAKER_UNMASK3")
            || string.Equals(key, "MASKMAKER_UNMASK4") || string.Equals(key, "MASKMAKER_UNMASK2") || string.Equals(key, "MASKMAKER_UNMASK_REPEAT");
    }

    private bool IsFlukeHermit(string key)
    {
        return string.Equals(key, "FLUKE_HERMIT_PRAY") || string.Equals(key, "FLUKE_HERMIT_PRAY_REPEAT")
            || string.Equals(key, "FLUKE_HERMIT_IDLE_1") || string.Equals(key, "FLUKE_HERMIT_IDLE_2")
            || string.Equals(key, "FLUKE_HERMIT_IDLE_3") || string.Equals(key, "FLUKE_HERMIT_IDLE_4")
            || string.Equals(key, "FLUKE_HERMIT_IDLE_5") || string.Equals(key, "MASK_MAKER_UNMASK2") || string.Equals(key, "MASK_MAKER_UNMASK_REPEAT");
    }

    private bool IsQueen(string key)
    {
        return string.Equals(key, "QUEEN_MEET") || string.Equals(key, "QUEEN_MEET_REPEAT")
            || string.Equals(key, "QUEEN_TALK_01") || string.Equals(key, "QUEEN_TALK_02")
            || string.Equals(key, "QUEEN_HORNET") || string.Equals(key, "QUEEN_DUNG")
            || string.Equals(key, "QUEEN_DUNG_02") || string.Equals(key, "QUEEN_REPEAT_KINGSOUL")
            || string.Equals(key, "QUEEN_TALK_EXTRA") || string.Equals(key, "QUEEN_REPEAT_SHADECHARM")
            || string.Equals(key, "QUEEN_GRIMMCHILD") || string.Equals(key, " QUEEN_GRIMMCHILD_FULL");
    }

    private bool IsWilloh(string key)
    {
        return string.Equals(key, "GIRAFFE_MEET") || string.Equals(key, "GIRAFFE_LOWER") || string.Equals(key, "GIRAFFE_LOWER_REPEAT");
    }

    private bool IsMyla(string key)
    {
        return string.Equals(key, "MINER_MEET_1_B") || string.Equals(key, "MINER_MEET_REPEAT") || string.Equals(key, "MINER_EARLY_1_B") || string.Equals(key, "MINER_EARLY_2_B") || string.Equals(key, "MINER_EARLY_3");
    }

    private bool IsQuirrel(string key)
    {
        return string.Equals(key, "QUIRREL_MINES_1") || string.Equals(key, "QUIRREL_MINES_2") || string.Equals(key, "QUIRREL_MINES_3") || string.Equals(key, "QUIRREL_MINES_4");
    }

    private bool IsEmilitia(string key)
    {
        return string.Equals(key, "EMILITIA_MEET") || string.Equals(key, "EMILITIA_KING_BRAND") || string.Equals(key, "EMILITIA_GREET") || string.Equals(key, "EMILITIA_REPEAT");
    }

    private bool IsMossProphet(string key)
    {
        return string.Equals(key, "MOSS_CULTIST_01") || string.Equals(key, "MOSS_CULTIST_02") || string.Equals(key, "MOSS_CULTIST_03");
    }

    #endregion

    #endregion
}
