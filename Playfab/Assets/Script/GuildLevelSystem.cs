using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildLevelSystem : MonoBehaviour
{
    public static GuildLevelSystem Instance = null;
    public float maxLevel;

    [Header("Multipliers")]
    [Range(1f, 300f)]
    public float additionMultiplier;
    [Range(2f, 4f)]
    public float powerMultiplier = 20f;
    [Range(7f, 14f)]
    public float divisionMultiplier = 7f;

    public void Start()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (DataCarrier.Instance.guildStats.currentLevel != maxLevel)
        {
            if (DataCarrier.Instance.guildStats.currentLevel != 0)
            {
                if (DataCarrier.Instance.guildStats.currentExp >= DataCarrier.Instance.guildStats.expToLevelUp)
                {
                    LevelUp();
                }
            }
        }
        else
        {
            DataCarrier.Instance.guildStats.currentExp = DataCarrier.Instance.guildStats.expToLevelUp;
        }
        
    }

    public void LevelUp()
    {
        Debug.Log("Levelled Up");
        DataCarrier.Instance.guildStats.currentLevel += 1;
        DataCarrier.Instance.guildStats.discountBonus += 1;
        CalculateNextLevelXp();
        Debug.Log(DataCarrier.Instance.guildStats.currentLevel);
       
    }

    public void CalculateNextLevelXp()
    {
        int solveForRequiredXp = 4;
        for (int levelCycle = 1; levelCycle <= DataCarrier.Instance.guildStats.currentLevel; levelCycle++)
        {
            solveForRequiredXp += (int)Mathf.Floor(levelCycle + additionMultiplier * Mathf.Pow(powerMultiplier, levelCycle / divisionMultiplier));
        }
        DataCarrier.Instance.guildStats.expToLevelUp = (solveForRequiredXp / 4) * DataCarrier.Instance.guildStats.currentLevel;
    }
    public static void GainExperienceFlatRate(float xpGained)
    {
        DataCarrier.Instance.guildStats.currentExp += xpGained;
    }
}
