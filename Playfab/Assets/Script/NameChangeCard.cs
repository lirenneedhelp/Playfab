using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class NameChangeCard : MonoBehaviour
{
    [SerializeField] GameObject nameChangePanel;
    [SerializeField] TMP_InputField nameChangeInput;
    [SerializeField] TMP_Text displayNameText;

    int new_index = 0;
     
    public void OpenNameChange(int index)
    {
        nameChangePanel.SetActive(true);
        new_index = index;
    }
    
    public void CloseNameChange()
    {
        nameChangePanel.SetActive(false);
    }

    public void UpdateDisplayName()
    {
        string name = nameChangeInput.text;

        var req = new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = name
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(req, 
            r=> {
                displayNameText.text = name;
                Inventory.Instance.UseInventoryItem(new_index);
                CloseNameChange();
                }, 
            e=>{
                e.GenerateErrorReport();
                });

    }
}
