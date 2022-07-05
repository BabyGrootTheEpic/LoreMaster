using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.KingdomsEdge;

internal class WisdomOfTheSagePower : Power
{
    #region Members

    private int _soulBonus;

    #endregion

    #region Constructors

    public WisdomOfTheSagePower() : base("", Area.KingdomsEdge)
    {
        // If the player is using spell twister (enable or disable) it triggers this fsm AFTER the charm update hook, which would negate the effect.
        // Therefore we have to add a clauses here too.
        PlayMakerFSM fsm = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Set Spell Cost");
        fsm.GetState("Idle").AddFirstAction(new Lambda(() =>
        {
            if (IsCurrentlyActive())
                UpdateSpellCost();

        }));
    }

    #endregion

    #region Event handler

    private void ModHooks_CharmUpdateHook(PlayerData data, HeroController controller) => UpdateSpellCost();

    #endregion

    #region Public Methods

    public override void Enable() 
    {
        ModHooks.CharmUpdateHook += ModHooks_CharmUpdateHook;
        _soulBonus = PlayerData.instance.mrMushroomState;
        GameObject.Find("Knight").LocateMyFSM("Spell Control").FsmVariables.FindFsmInt("MP Cost").Value -= _soulBonus;
    }

    public override void Disable() 
    { 
        ModHooks.CharmUpdateHook -= ModHooks_CharmUpdateHook;
        if (_soulBonus != 0)
            GameObject.Find("Knight").LocateMyFSM("Spell Control").FsmVariables.FindFsmInt("MP Cost").Value += _soulBonus;
        _soulBonus = 0;
    }

    #endregion

    #region Private Methods

    private void UpdateSpellCost()
    {
        GameObject.Find("Knight").LocateMyFSM("Spell Control").FsmVariables.FindFsmInt("MP Cost").Value += _soulBonus;
        _soulBonus = PlayerData.instance.mrMushroomState;
        GameObject.Find("Knight").LocateMyFSM("Spell Control").FsmVariables.FindFsmInt("MP Cost").Value -= _soulBonus;
    }

    #endregion
}
