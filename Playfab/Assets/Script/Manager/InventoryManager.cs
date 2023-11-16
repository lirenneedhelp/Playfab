using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class InventoryManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Msg, coinsText;

    List<CatalogItem> items;

    [SerializeField] private RectTransform _content;

    public GameObject itemShopPrefab;

    public Sprite[] itemArray;


    struct ItemData
    {
        public string itemName;
        public string itemID;
        public string virtualCurrency;
        public int price;

        public ItemData(string name, string itemId, string currency, int itemPrice)
        {
            itemName = name;
            itemID = itemId;
            virtualCurrency = currency;
            price = itemPrice;
        }

    }

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
            CatalogVersion = "ShopCatalog"
        };
        PlayFabClientAPI.GetCatalogItems(catreq, 
        result =>{
            
            UpdateMsg("Catalog Items");
            items = result.Catalog;
            List<ItemData> itemList = new();

            foreach(CatalogItem i in items)
            {
                ItemData iData = new ItemData(i.DisplayName, i.ItemId, "CC", (int)i.VirtualCurrencyPrices["CC"]);
                itemList.Add(iData);
                //UpdateMsg(i.DisplayName + "," + i.VirtualCurrencyPrices["CC"]);
            }

            PopulateShop(itemList);

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
    void PopulateShop(List<ItemData> Items)
    {
        // Clear existing leaderboard items
        foreach (Transform child in _content.transform)
        {
            Destroy(child.gameObject);
        }

        // Create leaderboard items based on the fetched scores
        for (int i = 0; i < Items.Count; i++)
        {
            GameObject shopItem = Instantiate(itemShopPrefab, _content);
            TMP_Text itemText = shopItem.transform.Find("ItemName").GetComponent<TMP_Text>();
            TMP_Text costText = shopItem.transform.Find("CostText").GetComponent<TMP_Text>();
            Image itemImage = shopItem.transform.Find("ItemImage").GetComponent<Image>();

            itemText.text = Items[i].itemName;
            costText.text = Items[i].price.ToString();
            itemImage.sprite = itemArray[i];
            

        }
    }
}
