using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance = null;

    public Image[] images;

    public TMP_Text[] textArray;

    public Button[] buttons;

    public Sprite[] itemArray;

    public List<ItemInstance> itemsList;

    public int usage = 0;

    public int totemUsage = 0;
    //public bool 
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GetPlayerInventory();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GetPlayerInventory()
    {
        var userInv = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(userInv,
        result => {
           
            //Debug.Log(ii.Count);
            itemsList = result.Inventory;
            int index = 0;

            // Clear existing event listeners before updating the UI
            foreach (var button in buttons)
            {
                button.onClick.RemoveAllListeners();
            }

            foreach (ItemInstance i in itemsList)
            {
                if (i.RemainingUses > 0)
                {
                    if (i.DisplayName == "Totem Of Undying")
                    {
                        int itemIndex = index;
                        images[index].sprite = itemArray[0];
                        buttons[index].onClick.AddListener(
                            ()=> {
                                UseInventoryItem(itemIndex);
                                totemUsage++;
                                //Debug.Log(totemUsage);
                            });
                    }
                    else if (i.DisplayName == "Score Multiplier")
                    {
                        int itemIndex = index;
                        images[index].sprite = itemArray[1];
                        buttons[index].onClick.AddListener(
                            () => {
                                UseInventoryItem(itemIndex);
                                usage++;
                            });
                    }
                    else if (i.DisplayName == "Name Change Card")
                    {
                        int itemIndex = index;
                        images[index].sprite = itemArray[2];
                        buttons[index].onClick.AddListener(
                            () => {
                                UseInventoryItem(itemIndex);
                            });
                    }
                    textArray[index].text = i.RemainingUses.ToString();
                }
                index++;
                //Debug.Log(i.DisplayName + "," + i.ItemId + "," + i.ItemInstanceId);
            }
        }, e=>
        {
            Debug.Log("Error retrieving inventory");
        });
    }

    public void UseInventoryItem(int index)
    {
        var userInvReq = new ConsumeItemRequest()
        {
            ConsumeCount = 1,
            ItemInstanceId = itemsList[index].ItemInstanceId
           // CharacterId = DataCarrier.Instance.playfabID
        };

        PlayFabClientAPI.ConsumeItem(userInvReq, r =>
        {
            Debug.Log("Used Item");
            //Debug.Log(itemsList[index].RemainingUses);
            if (itemsList[index].RemainingUses - 1 == 0)
            {
                images[index].sprite = null;
                textArray[index].text = "";
            }

            GetPlayerInventory();
        },
        e =>
        {
            Debug.Log(e.GenerateErrorReport());
        });
    }
}
