using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using Modding;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers.Ancient_Basin;

public class WeDontTalkAboutShadePower : Power
{
    #region Constructors

    public WeDontTalkAboutShadePower() : base("We don't talk about the Shade", Area.AncientBasin) { }

    #endregion

    #region Properties

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override Action SceneAction => () =>
    {
        // The game uses the soulLimited value to determine, if the shade is active, because we negate that, we manually need to spawn the shade.
        if (GameObject.Find("Hollow Shade(Clone)") == null && string.Equals(PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)), UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
            GameObject.Instantiate(GameManager.instance.sm.hollowShadeObject, new Vector3(PlayerData.instance.GetFloat(nameof(PlayerData.instance.shadePositionX)), PlayerData.instance.GetFloat(nameof(PlayerData.instance.shadePositionY))), Quaternion.identity);
    };

    #endregion

    #region Event Handler

    /// <summary>
    /// Removes the soul limited punishment from the player.
    /// </summary>
    private void AfterPlayerDied() => PlayerData.instance.SetBool("soulLimited", false);

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        HeroController.instance.transform.Find("Hero Death").gameObject.LocateMyFSM("Hero Death Anim").GetState("Remove Geo").ReplaceAction(new Lambda(() =>
        {
            if (Active)
            {
                int shadeGeo = PlayerData.instance.GetInt(nameof(PlayerData.instance.geoPool));
                PlayerData.instance.SetInt(nameof(PlayerData.instance.geoPool), PlayerData.instance.GetInt(nameof(PlayerData.instance.geo)) + (shadeGeo > 1
                    ? shadeGeo / 2
                    : 0));
            }
            else
                PlayerData.instance.SetInt(nameof(PlayerData.instance.geoPool), PlayerData.instance.GetInt(nameof(PlayerData.instance.geo)));
        })
        {
            Name = "Diminish geo punishment"
        }, 1);
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        ModHooks.AfterPlayerDeadHook += AfterPlayerDied;
        PlayerData.instance.SetBool("soulLimited", false);
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (string.Equals(self.FsmName, "Deactivate if !SoulLimited"))
            self.GetState("Check").ReplaceAction(new Lambda(() =>
            {
                if (Active)
                {
                    if (string.Equals(PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)), "None"))
                        self.SendEvent("DEACTIVATE");
                }
                else if (!PlayerData.instance.GetBool(nameof(PlayerData.instance.soulLimited)))
                    self.SendEvent("DEACTIVATE");
            })
            { Name = "Check for Shade"}, 0);
        orig(self);
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        ModHooks.AfterPlayerDeadHook -= AfterPlayerDied;
        if (!PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)).Equals("None"))
            PlayerData.instance.StartSoulLimiter();
    }

    #endregion
}
