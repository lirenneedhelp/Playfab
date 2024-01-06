using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TradeSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            draggableItem.parentAfterDrag = transform;

            byte[] spriteBytes = ConvertSpriteToBytes(draggableItem.GetComponent<Image>().sprite);
            transform.root.GetComponent<PhotonView>().RPC("UpdateTrade", transform.root.GetComponent<Player>().photonPlayer, spriteBytes);
        }

    }
    byte[] ConvertSpriteToBytes(Sprite sprite)
    {
        Texture2D texture = sprite.texture;
        byte[] bytes = ImageConversion.EncodeToPNG(texture); // You can also use EncodeToJPG
        return bytes;
    }


}
