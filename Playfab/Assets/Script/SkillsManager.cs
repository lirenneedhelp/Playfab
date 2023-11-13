using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class SkillsManager : MonoBehaviour
{
    public List<Skill> skillList = new List<Skill>();

    private void Start()
    {
        LoadJSON();
    }

    public void SendJSON()
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
            for (int i = 0; i < skillList.Count; i++)
                skillList[i] = jlw.list[i];
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
            new Skill("Jump", 0, false),
            new Skill ("Fly", 0, false),
            new Skill("Dash", 0, false)
        };
    }

}
