using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class InventoryManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Msg, coinsText;

    List<CatalogItem> items;

    void Start()
    {
        GetVirtualCurrencies();
        GetCatalog();
    }
    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
        Msg.text = msg + "\n";
    }

    void OnError(PlayFabError e)
    {
        UpdateMsg(e.GenerateErrorReport());
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void GetVirtualCurrencies()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        r=>{
            int coins = r.VirtualCurrency["CC"];
            coinsText.text = "Coins:" + coins;
            UpdateMsg("Coins:" + coins);
        },
        OnError);
    }
    public void GetCatalog()
    {
        var catreq = new GetCatalogItemsRequest(){
            CatalogVersion = "FoodCatalog"
        };
        PlayFabClientAPI.GetCatalogItems(catreq, 
        result =>{
            
            UpdateMsg("Catalog Items");
            items = result.Catalog;
            foreach(CatalogItem i in items)
            {
                UpdateMsg(i.DisplayName + "," + i.VirtualCurrencyPrices["CC"]);
            }
        }, OnError);
    }
    public void BuyItem(int index)
    {
        var buyreq = new PurchaseItemRequest(){
            CatalogVersion = items[index].CatalogVersion,
            ItemId = items[index].ItemId,
            VirtualCurrency = "CC",
            Price = (int)items[index].VirtualCurrencyPrices["CC"]
        };
        
        PlayFabClientAPI.PurchaseItem(buyreq,
        result => {
            UpdateMsg("Bought!");
        },OnError);
    }
    public void GetPlayerInventory()
    {
        var userInv = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(userInv, 
        result => {
            List<ItemInstance> ii = result.Inventory;
            UpdateMsg("Player Inventory");
            foreach(ItemInstance i in ii)
            {
                UpdateMsg(i.DisplayName + "," + i.ItemId + "," + i.ItemInstanceId);
            }
        }, OnError);
    }
}
