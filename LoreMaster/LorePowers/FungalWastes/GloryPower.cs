using LoreMaster.Enums;
using Modding;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class GloryPower : Power
{
    #region Members

    private Dictionary<string, int[]> _enemyGeoValues = new Dictionary<string, int[]>();

    #endregion
    
    #region Constructors

    public GloryPower() : base("Glory of the Wealth", Area.FungalWastes)
    {
        Hint = "Your enemies may \"share\" more of their wealth with you.";
        Description = "Enemies drop more geo. 60% for small Geo (0-20 pieces), 30% for medium Geo (2 - 10 pieces) or 10% for large Geo (2-10 pieces)";
    } 

    #endregion

    #region Protected Methods

    protected override void Enable()
    {
        LoreMaster.Instance.SceneActions.Add(PowerName, () =>
        {
            HealthManager[] enemies = GameObject.FindObjectsOfType<HealthManager>();

            foreach (HealthManager enemy in enemies)
            {
                // Get the enemy name. We need to use a regex to prevent flouding the dictionary with reduntant data. For example: If enemy is called Crawler 1, the entry for "Crawler" doesn't work.
                string enemyName = Regex.Match(enemy.name, @"^[^0-9]*").Value.Trim();

                // Check if we already have registered the enemy type. This action takes a lot of loading time, therefore we want to avoid it, as much as we can.
                if (!_enemyGeoValues.ContainsKey(enemyName))
                {
                    int[] geoValues = new int[3];

                    geoValues[0] = ReflectionHelper.GetField<HealthManager, int>(enemy, "smallGeoDrops");
                    geoValues[1] = ReflectionHelper.GetField<HealthManager, int>(enemy, "mediumGeoDrops");
                    geoValues[2] = ReflectionHelper.GetField<HealthManager, int>(enemy, "largeGeoDrops");

                    _enemyGeoValues.Add(enemyName, geoValues);
                }

                int[] geoDrops = _enemyGeoValues[enemyName];
                // We only increase one of the geo drops. But only if it would drop geo anyway
                if (geoDrops.Any(x => x != 0))
                {
                    // The dropped geo:
                    // Small Geo (60%) 0 to 20
                    // Medium Geo (30%) 10 to 50
                    // Large Geo (10%) 20 to 100
                    int rolledNumber = LoreMaster.Instance.Generator.Next(1, 101);

                    if (rolledNumber < 60)
                        enemy.SetGeoSmall(geoDrops[0] + LoreMaster.Instance.Generator.Next(0, 21));
                    else if (rolledNumber < 90)
                        enemy.SetGeoMedium(geoDrops[1] + LoreMaster.Instance.Generator.Next(2, 11));
                    else
                        enemy.SetGeoLarge(geoDrops[2] + LoreMaster.Instance.Generator.Next(2, 11));
                }
            }
        });
    }

    protected override void Disable() => LoreMaster.Instance.SceneActions.Remove(PowerName); 

    #endregion
}
