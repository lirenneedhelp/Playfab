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

    int coins;


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
        //Inventory.Instance.GetPlayerInventory();
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
            coins = r.VirtualCurrency["CC"];
            coinsText.text = coins.ToString();
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
            
            //UpdateMsg("Catalog Items");
            items = result.Catalog;
            //Inventory.Instance.itemsList = items;
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
            GetVirtualCurrencies();
            Inventory.Instance.GetPlayerInventory();
        },
        error=>
        {
            UpdateMsg("Not Enough Coins");
        });
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
            int itemIndex = i;
            GameObject shopItem = Instantiate(itemShopPrefab, _content);
            TMP_Text itemText = shopItem.transform.Find("ItemName").GetComponent<TMP_Text>();
            TMP_Text costText = shopItem.transform.Find("CostText").GetComponent<TMP_Text>();
            Image itemImage = shopItem.transform.Find("ItemImage").GetComponent<Image>();

            itemText.text = Items[i].itemName;
            costText.text = (Items[i].price - Items[i].price * DataCarrier.Instance.guildStats.discountBonus / 100.0f).ToString();
            itemImage.sprite = itemArray[i];

            shopItem.GetComponent<Button>().onClick.AddListener(() => {
                BuyItem(itemIndex);
            });


        }
    }

}
