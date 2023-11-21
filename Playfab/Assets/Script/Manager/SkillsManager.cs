using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class SkillsManager : MonoBehaviour
{
    public List<Skill> skillList = new List<Skill>();
    public TMP_Text[] skillLevelDisplay;
    public TMP_Text SPDisplay;

    private void Start()
    {
        if (!DataCarrier.Instance.isGuest)
            LoadJSON();        
    }

    private void SendJSON()
    {
        string stringListAsJson = JsonUtility.ToJson(new JSListWrapper<Skill>(skillList));
        Debug.Log(stringListAsJson);
        var req = new UpdateUserDataRequest{
            Data = new Dictionary<string, string>
            {
                {"Skills", stringListAsJson}
            }
        };
        PlayFabClientAPI.UpdateUserData(req, 
        result=>{
            Debug.Log("Data sent success!");
            }, 
        e=>{
            Debug.Log("Error" + e.GenerateErrorReport());
        });
    }

    public void LoadJSON()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJSONDataReceived, 
        e=>{
            Debug.Log("Error" + e.GenerateErrorReport());
            Initialize();
            });
    }

    void OnJSONDataReceived(GetUserDataResult r)
    {
        Debug.Log("Received JSON data");
        if (r.Data != null && r.Data.ContainsKey("Skills"))
        {
            Debug.Log(r.Data["Skills"].Value);
            JSListWrapper<Skill> jlw = JsonUtility.FromJson<JSListWrapper<Skill>>(r.Data["Skills"].Value);
            skillList = jlw.list;
            DataCarrier.Instance.skills = skillList;
            for(int i = 0; i < skillList.Count; i++)
                UpdateDisplaySkillLevel(i, skillList[i].level);
        }
        else
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        skillList = new()
        {
            new Skill("Jump_Boost", 0, false),
            new Skill ("Double_Jump", 0, false),
            new Skill("Flight", 0, false)
        };
        DataCarrier.Instance.skills = skillList;
    }

    public void UpgradeSkill(string name)
    {
        for (int i = 0; i < skillList.Count; i++)
        {
            if (skillList[i].name == name)
            {
                LevelSystem.Instance.skillPoints--;

                Skill skillToUpdate = skillList[i];
                skillToUpdate.level++;
                skillList[i] = skillToUpdate;
                
                UpdateSPDisplay();
                UpdateDisplaySkillLevel(i, skillToUpdate.level);
                
                if (!DataCarrier.Instance.isGuest)
                {
                    SendJSON();
                    PFDataMgr.UpdateDataJSON(LevelSystem.Instance.level, (int)LevelSystem.Instance.maxLevel, LevelSystem.Instance.currentXp, LevelSystem.Instance.nextLevelXp, LevelSystem.Instance.skillPoints);
                }
                
                return;
            }
        }
    }

    public void UpdateSPDisplay()
    {
        SPDisplay.text = "Available Skill Points:" + LevelSystem.Instance.skillPoints;
    }

    public void UpdateDisplaySkillLevel(int skillIndex, int skillLevel)
    {
        skillLevelDisplay[skillIndex].text = "Level " + skillLevel;
    }

}
