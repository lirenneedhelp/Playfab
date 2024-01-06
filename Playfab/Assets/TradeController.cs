using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeController : MonoBehaviour
{
    public GameObject localPlayerItem;
    public GameObject otherPlayerItem;

    public void UpdateSprite(Sprite sprite)
    {
        otherPlayerItem.GetComponent<Image>().sprite = sprite;
    }
    public GameObject ReturnLocalPlayerItem()
    {
        return localPlayerItem.transform.Find("Item(Clone)").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
