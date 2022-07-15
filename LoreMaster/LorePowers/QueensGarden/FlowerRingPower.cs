using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;

namespace LoreMaster.LorePowers.QueensGarden;

public class FlowerRingPower : Power
{
    #region Constructors

    public FlowerRingPower() : base("Ring of Flowers", Area.QueensGarden)
    {
        Hint = "The power of all existing flower are gathered if you channel powerful strikes.";
        Description = "Increase the damage of your nail arts by 10% for each recipient for the flower.";
    }

    #endregion

    #region Event Handler

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.FsmName.Equals("nailart_damage"))
        {
            if (self.GetState("Set").Actions[0] is not Lambda)
                self.GetState("Set").AddFirstAction(new Lambda(() =>
                {
                    if (Active)
                    {
                        float damageMultiplier = 1f;

                        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.elderbugGaveFlower)))
                            damageMultiplier += .1f;
                        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.givenEmilitiaFlower)))
                            damageMultiplier += .1f;
                        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.givenGodseekerFlower)))
                            damageMultiplier += .1f;
                        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.givenOroFlower)))
                            damageMultiplier += .1f;
                        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.givenWhiteLadyFlower)))
                            damageMultiplier += .1f;

                        // This applies onto the already modified damage value. This means that if the multiplier is 1.5f, the end damage is 3.75 times the nail damage.
                        // With fury about 7 times (if I'm not that bad in math).
                        self.FsmVariables.FindFsmFloat("Damage Float").Value *= damageMultiplier;
                    }
                }));
        }
        orig(self);
    }

    #endregion

    #region Protected Methods

    protected override void Enable() => On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    
    protected override void Disable() => On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
    
    #endregion
}
