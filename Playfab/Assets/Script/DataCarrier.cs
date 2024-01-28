using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCarrier : MonoBehaviour
{
    [System.Serializable]
    public class GuildStats
    {
        public int currentLevel = 0;
        public int discountBonus = 0;
        public float currentExp = 0;
        public int expToLevelUp = 0;
    }

    public static DataCarrier Instance = null;
    public string playfabID;
    public string sessionTicket;

    public string entityID;
    public string entityType;
    public string displayName;
    public bool isGuest;
    public bool inGuild;
    public List<Skill> skills;
    public GuildStats guildStats = new();
    public PlayFab.GroupsModels.EntityKey group;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
