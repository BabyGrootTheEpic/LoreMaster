using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using Modding;

namespace LoreMaster.LorePowers.CityOfTears;

public class SoulExtractEfficiencyPower : Power
{
    #region Constructors

    public SoulExtractEfficiencyPower() : base("Soul Extract Efficiency", Area.CityOfTears)
    {
        Hint = "Allows you to drain soul more efficient from your foes and draw you reserve more quickly.";
        Description = "You gain 5 more soul per hit on enemies and your extra soul vessel soul will be added instantly to be used for spells after casting spell. This does NOT affect focus, unless you do the wind up again.";
    }

    #endregion

    #region Event handler

    /// <summary>
    /// Handles the soul gain on hit for enemies.
    /// </summary>
    private int OnSoulGain(int soulToGain) => soulToGain + 5;

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        HeroController.instance.spellControl.GetState("Inactive").ReplaceAction(new Lambda(() =>
        {
            if (Active)
            {
                int currentMP = PlayerData.instance.GetInt(nameof(PlayerData.instance.MPCharge));
                int maxMP = PlayerData.instance.GetInt(nameof(PlayerData.instance.maxMP));
                int reserveMP = PlayerData.instance.GetInt(nameof(PlayerData.instance.MPReserve));

                if (currentMP < maxMP && reserveMP > 0)
                {
                    int neededMP = maxMP - currentMP;
                    if (reserveMP - neededMP >= 0)
                    {
                        reserveMP -= neededMP;
                    }
                    HeroController.instance.TakeReserveMP(reserveMP);
                    HeroController.instance.AddMPCharge(reserveMP);
                }
            }
            HeroController.instance.spellControl.FsmVariables.FindFsmBool("Double").Value = false;
        })
        { Name = "Quick Refreshing MP"}, 0);
    }

    /// <inheritdoc/>
    protected override void Enable() => ModHooks.SoulGainHook += OnSoulGain;
    
    /// <inheritdoc/>
    protected override void Disable() => ModHooks.SoulGainHook -= OnSoulGain;

    #endregion
}

