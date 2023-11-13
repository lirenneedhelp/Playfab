using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance = null;
    public int level;
    public float maxLevel;
    public float currentXp;
    public int nextLevelXp = 100;
    public int skillPoints;

    [Header("Multipliers")]
    [Range(1f, 300f)]
    public float additionMultiplier;
    [Range(2f, 4f)]
    public float powerMultiplier = 20f;
    [Range(7f, 14f)]
    public float divisionMultiplier = 7f;
    //public GameObject levelUpEffect;

    [Header("UI")]
    public Image frontXpBar;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;

    // Timers
    private float delayTimer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);

            PFDataMgr.LoadJSON();

            levelText.text = "Level " + level + ":";
            level = 1;
            xpText.text = Mathf.Round(currentXp) + "/" + Mathf.Round(nextLevelXp);
            frontXpBar.fillAmount = currentXp / nextLevelXp;
            nextLevelXp = CalculateNextLevelXp();
        }
        else
            Destroy(gameObject);
    }

    void Update()
    {
        UpdateXpUI();
        if (level != maxLevel)
        {
            if (currentXp >= nextLevelXp)
            {
                LevelUp();
            }        
        }
        else
        {
            currentXp = nextLevelXp;
            xpText.text = "MAX";
            frontXpBar.fillAmount = currentXp / nextLevelXp;
        }
    }

    private void UpdateXpUI() 
    {
        float xpFraction = currentXp / nextLevelXp;
        float fXP = frontXpBar.fillAmount;

        if (fXP < xpFraction)
        {
            delayTimer += Time.deltaTime;
            frontXpBar.fillAmount = Mathf.Lerp(fXP, xpFraction, delayTimer / 5f);
        }

        levelText.text = "Level " + level + ":";
        xpText.text = Mathf.Round(currentXp) + "/" + nextLevelXp;
    }

    public void GainExperienceFlatRate(float xpGained)
    {
        currentXp += xpGained;
        delayTimer = 0f;
    }

    public void GainExperienceScalable(float xpGained, int passedLevel)
    {
        if (passedLevel < level)
        {
            float multiplier = 1 + (level - passedLevel) * 0.1f;
            currentXp += Mathf.Round(xpGained * multiplier);
        }
        else
        {
            currentXp += xpGained;
        }

        delayTimer = 0f;
    }

    public void LevelUp() 
    {
        level += 1;
        frontXpBar.fillAmount = 0f;
        currentXp = Mathf.Round(currentXp - nextLevelXp);
        nextLevelXp = CalculateNextLevelXp();
        level = Mathf.Clamp(level, 0, 50);

        xpText.text = Mathf.Round(currentXp) + "/" + nextLevelXp;
        levelText.text = "Level " + level + ":";
        skillPoints++;
        //Instantiate(levelUpEffect, transform.position, Quaternion.identity);
    }

    private int CalculateNextLevelXp() 
    {
        int solveForRequiredXp = 4;
        for (int levelCycle = 1; levelCycle <= level; levelCycle++)
        {
            solveForRequiredXp += (int)Mathf.Floor(levelCycle + additionMultiplier * Mathf.Pow(powerMultiplier, levelCycle / divisionMultiplier));
        }
        return (solveForRequiredXp / 4) * level;
    }
}
