using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCarrier : MonoBehaviour
{
    public static DataCarrier Instance = null;
    public string playfabID;
    public string sessionTicket;

    public string entityID;
    public string entityType;
    public string displayName;
    public bool isGuest;
    public List<Skill> skills;
    
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
