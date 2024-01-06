using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine.EventSystems;
using System.Linq;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance = null;

    public Image[] images;

    public TMP_Text[] textArray;

    public Sprite[] itemArray;

    public List<ItemInstance> itemsList;

    public int usage = 0;

    public int totemUsage = 0;

    public string[] instanceIdArray;

    public NameChangeCard nameChangeObject;
    //public bool 
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            instanceIdArray = new string[10];
            instanceIdArray = Enumerable.Repeat("", 10).ToArray();
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

             //Clear existing event listeners before updating the UI
            foreach (var button in images)
            {
                button.GetComponent<EventTrigger>().triggers.Clear();
            }

            foreach (ItemInstance i in itemsList)
            {
                if (i.RemainingUses > 0 && !instanceIdArray.Contains(i.ItemInstanceId))
                {
                    int itemIndex = 0;
                    instanceIdArray[index] = i.ItemInstanceId;

                    if (i.DisplayName == "Totem Of Undying")
                    {
                        itemIndex = 0;
                        images[index].sprite = itemArray[0];
                    }
                    else if (i.DisplayName == "Score Multiplier")
                    {
                        itemIndex = 1;
                        images[index].sprite = itemArray[1];
                    }
                    else if (i.DisplayName == "Name Change Card")
                    {
                        itemIndex = 2;
                        images[index].sprite = itemArray[2];                          
                    }

                    int temp = index;
                    EventTrigger trigger = images[index].gameObject.GetComponent<EventTrigger>();
                    EventTrigger.Entry entry = new();
                    entry.eventID = EventTriggerType.PointerClick;
                    entry.callback.AddListener((data) => OnRightClick((PointerEventData)data, itemIndex, temp));
                    trigger.triggers.Add(entry);
                    //textArray[index].text = i.RemainingUses.ToString();
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
        Debug.Log(index);
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
                instanceIdArray[index] = "";
                //textArray[index].text = "";
            }

            GetPlayerInventory();
        },
        e =>
        {
            Debug.Log(e.GenerateErrorReport());
        });
    }

    private void OnRightClick(PointerEventData eventData, int itemIndex, int slotNumber)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            switch (itemIndex)
            {
                case 0:
                    UseInventoryItem(slotNumber);
                    totemUsage++;
                    Debug.Log("USING TOTEM");
                    break;
                case 1:
                    UseInventoryItem(slotNumber);
                    usage++;
                    Debug.Log("USING SCORE MULTIPLIER");
                    break;
                case 2:
                    nameChangeObject.OpenNameChange(slotNumber);
                    break;
            }
        }
    }
}
