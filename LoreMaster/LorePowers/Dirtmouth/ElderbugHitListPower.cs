using LoreMaster.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace LoreMaster.LorePowers.Dirtmouth;

/// <summary>
/// Unused currently.
/// </summary>
public class ElderbugHitListPower : Power
{
    #region Members

    private List<HealthManager> _enemies = new();
    private string _currentScene;

    #endregion

    #region Constructors

    public ElderbugHitListPower() : base("Elderbug Hit List", Area.Dirtmouth)
    {
        Hint = "When entering a new room with enemies the list marks enemies. If you succeed the \"talk\" with them, you are granted a reward. Be careful, elderbug wants a clean execution.";
        Description = "When entering a new room marks a random enemy with a red outline. If you kill that enemy FIRST another one gets marked. If all enemies are killed in the right order you receive a" +
            " soul vessel fragment, mask shard, charm notch, 200 geo, or a small damage buff.";
    }

    #endregion

    #region Properties

    public Dictionary<string, ElderbugReward> ElderbugRewards { get; set; } = new();

    public bool CanAchieveGeo => ElderbugRewards.Count(x => x.Value == ElderbugReward.Geo) < 15;

    public bool CanAchieveMask => ElderbugRewards.Count(x => x.Value == ElderbugReward.MaskShard) < 12;

    public bool CanAchieveVessel => ElderbugRewards.Count(x => x.Value == ElderbugReward.SoulVessel) < 9;

    public bool CanAchieveNail => ElderbugRewards.Count(x => x.Value == ElderbugReward.Nail) < 5;

    public bool CanAchieveNotch => ElderbugRewards.Count(x => x.Value == ElderbugReward.Notch) < 5;

    public bool CanAchieveReward => CanAchieveGeo || CanAchieveMask || CanAchieveNail || CanAchieveReward || CanAchieveVessel;

    #endregion

    #region Protected Methods

    protected override void Enable()
    {
        LoreMaster.Instance.SceneActions.Add(PowerName, () =>
        {
            _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            _enemies.Clear();
            if (!_currentScene.Contains("GG") && !ElderbugRewards.ContainsKey(_currentScene) && CanAchieveReward)
            {
                _enemies = GameObject.FindObjectsOfType<HealthManager>().ToList();
                if (_enemies.Count > 0)
                    MarkEnemy();
            }
        });

        On.HealthManager.Die += HealthManager_Die;
    }

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (self.GetComponents<Outline>() != null)
        {
            _enemies.Remove(self);
            GameObject.Destroy(self.GetComponent<Outline>());
            if (_enemies.Count > 0)
                MarkEnemy();
            else
                GiveReward();
        }
        else
        {
            HealthManager markedEnemy = _enemies.FirstOrDefault(x => x.GetComponent<Outline>() != null);
            if (markedEnemy != null)
                GameObject.Destroy(markedEnemy.GetComponent<Outline>());
        }
    }

    #endregion

    #region Private Methods

    private void MarkEnemy()
    {
        HealthManager selectedEnemy = _enemies[LoreMaster.Instance.Generator.Next(0, _enemies.Count)];
        Outline outline = selectedEnemy.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.red;
    }

    private void GiveReward()
    {
        List<ElderbugReward> receivableRewards = new();

        if (CanAchieveGeo)
            receivableRewards.Add(ElderbugReward.Geo);
        if (CanAchieveMask)
            receivableRewards.Add(ElderbugReward.MaskShard);
        if (CanAchieveVessel)
            receivableRewards.Add(ElderbugReward.SoulVessel);
        if (CanAchieveNail)
            receivableRewards.Add(ElderbugReward.Nail);
        if (CanAchieveNotch)
            receivableRewards.Add(ElderbugReward.Notch);

        ElderbugReward selectedReward = receivableRewards[LoreMaster.Instance.Generator.Next(0, receivableRewards.Count)];

        switch (selectedReward)
        {
            case ElderbugReward.Geo:
                HeroController.instance.AddGeo(200);
                break;
            case ElderbugReward.MaskShard:
                
                break;
            case ElderbugReward.SoulVessel:
                break;
            case ElderbugReward.Notch:
                
                break;
            case ElderbugReward.Nail:
                break;
            default:
                break;
        }

        if (!CanAchieveReward)
            On.HealthManager.Die -= HealthManager_Die;
    }

    #endregion
}
