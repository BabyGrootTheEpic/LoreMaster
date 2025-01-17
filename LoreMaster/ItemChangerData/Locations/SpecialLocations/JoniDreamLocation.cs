﻿using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Extensions;
using LoreMaster.Randomizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

internal class JoniDreamLocation : GhostDialogueLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        try
        {
            if (self.gameObject.name == "Ghost Activator" && self.transform.childCount > 0 && string.Equals("Ghost NPC Joni", self.transform.GetChild(0)?.name)
                && RandomizerManager.PlayingRandomizer)
                self.GetState("Idle").ReplaceAction(new Lambda(() =>
                {
                    if (PlayerData.instance.GetBool(nameof(PlayerData.instance.hasDreamNail)))
                        self.SendEvent("SHINY PICKED UP");
                }), 0);
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error while modifying joni spawn: "+exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
        orig(self);
    }
}
