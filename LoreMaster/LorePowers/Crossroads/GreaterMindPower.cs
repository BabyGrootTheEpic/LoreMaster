using HutongGames.PlayMaker;
using LoreMaster.Enums;
using LoreMaster.Helper;
using LoreMaster.Manager;
using LoreMaster.Randomizer;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LoreMaster.LorePowers.Crossroads;

public class GreaterMindPower : Power
{
    #region Members

    private Sprite _loreSprite;

    private GameObject _loreTracker;

    private Dictionary<Area, string> _areas = new()
    {
        {Area.None, "Menu" },
        {Area.AncientBasin, "Ancient Basin" },
        {Area.CityOfTears, "City of Tears" },
        {Area.Dirtmouth, "Dirtmouth"},
        {Area.Crossroads, "Crossroads"},
        {Area.Greenpath, "Greenpath"},
        {Area.Deepnest, "Deepnest" },
        {Area.FungalWastes, "Fungal Wastes"},
        {Area.QueensGarden, "Queen's Gardens"},
        {Area.Peaks, "Crystal Peaks"},
        {Area.RestingGrounds, "Resting Grounds"},
        {Area.WaterWays, "Waterways"},
        {Area.KingdomsEdge, "Kingdom's Edge"},
        {Area.FogCanyon, "Fog Canyon"},
        {Area.Cliffs, "Howling Cliffs"},
        {Area.WhitePalace, "White Palace"},
    };

    #endregion

    #region Constructors

    public GreaterMindPower() : base("Greater Mind", Area.Crossroads)
    {
        _loreSprite = SpriteHelper.CreateSprite("Lore");
    }

    #endregion

    #region Properties

    /// <summary>
    /// Desperate attempt to make the tracker work, if it doesn't work before...
    /// </summary>
    public override Action SceneAction => () =>
    {
        try
        {
            if (_loreTracker == null)
                Initialize();
        }
        catch (Exception)
        { }
    };

    /// <summary>
    /// Gets the cost of the glory effect.
    /// </summary>
    public static bool PermanentTracker { get; set; }

    public static bool NormalTracker { get; set; }

    #endregion

    #region Event handler

    private void PlayerDataBoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        orig(self);
        if (self.IsCorrectContext("Hero Death Anim", "Hero Death", "Break Glass HP") && PowerManager.ObtainedPowers.Count > 0)
        {
            Power power = PowerManager.ObtainedPowers.Last();
            power.DisablePower(false);
            PowerManager.ObtainedPowers.Remove(power);
            PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value, "Display");
            playMakerFSM.FsmVariables.GetFsmInt("Convo Amount").Value = 1;
            playMakerFSM.FsmVariables.GetFsmString("Convo Title").Value = $"Remove_Power_{power.PowerName}";
            playMakerFSM.SendEvent("DISPLAY ENEMY DREAM");
        }
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        GameObject prefab = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Inv_Items/Geo").gameObject;
        GameObject hudCanvas = GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas").gameObject;
        _loreTracker = GameObject.Instantiate(prefab, hudCanvas.transform, true);
        _loreTracker.name = "Lore Tracker";
        PositionHudElement(_loreTracker, 3);
        _loreTracker.SetActive(false);
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        if (_loreTracker != null)
        {
            _loreTracker.SetActive(true);
            UpdateLoreCounter(PowerManager.ObtainedPowers, PowerManager.GetAllPowers(), SettingManager.Instance.CurrentArea, PowerManager.IsAreaGlobal(SettingManager.Instance.CurrentArea));
        }
    }

    /// <inheritdoc/>
    protected override void Disable() => _loreTracker?.SetActive(false);

    /// <inheritdoc/>
    protected override void TwistEnable() => On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;
    
    /// <inheritdoc/>
    protected override void TwistDisable() => On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= PlayerDataBoolTest_OnEnter;
    
    #endregion

    #region Private Methods

    private void PositionHudElement(GameObject go, int fontSize)
    {
        try
        {
            go.transform.localPosition = new(-3.66f, -4.32f, 0f);
            go.transform.localScale = new(1.3824f, 1.3824f, 1.3824f);
            go.GetComponent<DisplayItemAmount>().playerDataInt = go.name;
            go.GetComponent<DisplayItemAmount>().textObject.text = "";
            go.GetComponent<DisplayItemAmount>().textObject.fontSize = fontSize;
            go.GetComponent<DisplayItemAmount>().textObject.gameObject.name = "Counter";
            go.GetComponent<SpriteRenderer>().sprite = _loreSprite;
            go.GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 1f);
            go.GetComponent<BoxCollider2D>().offset = new Vector2(0.5f, 0f);
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error in Position Hud: " + exception.Message);
        }
    }

    public void UpdateLoreCounter(IEnumerable<Power> activePowers, IEnumerable<Power> allPowers, Area currentArea, bool globalActive)
    {
        if (_loreTracker == null)
        {
            LoreMaster.Instance.LogError("The lore tracker doesn't exist");
            return;
        }
        try
        {
            _loreTracker.SetActive(true);
            TextMeshPro currentCounter = _loreTracker.GetComponent<DisplayItemAmount>().textObject;
            if (ModHooks.GetMod("Randomizer 4") is Mod && RandomizerManager.RandoTracker(currentArea, out string areaLore))
            {
                currentCounter.text = $"{_areas[currentArea]}: {areaLore}";
                if (globalActive)
                    currentCounter.text = $"<color=#7FFF7B>{currentCounter.text}</color>";
            }
            else
            {
                currentCounter.text = _areas[currentArea] + ": " + activePowers.Count(x => x.Location == currentArea);
                currentCounter.text += "/" + allPowers.Count(x => x.Location == currentArea);
                if (globalActive)
                    currentCounter.text = "<color=#7FFF7B>" + currentCounter.text + "</color>";
            }
            if (!PermanentTracker)
                LoreMaster.Instance.Handler.StartCoroutine(HideTracker());
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error while loading counter: " + exception.Message);
            LoreMaster.Instance.LogError("Error while loading counter: " + exception.StackTrace);
        }
    }

    private IEnumerator HideTracker()
    {
        yield return new WaitForSeconds(5f);
        if (_loreTracker.activeSelf)
            _loreTracker.SetActive(false);
    }

    #endregion
}
