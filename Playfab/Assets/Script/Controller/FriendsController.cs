using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendsController : MonoBehaviour
{
    public GameObject panel;
    [SerializeField] FriendsManagement friendManager;
    System.Action playerCallback;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OpenPanel(System.Action callBack = null)
    {
        friendManager.DisplayFL();
        panel.SetActive(true);
        playerCallback = callBack;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        playerCallback();
    }
}
