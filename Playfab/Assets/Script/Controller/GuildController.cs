using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildController : MonoBehaviour
{
    public GameObject panel;
    [SerializeField] GuildTestController guildTestController;

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
        guildTestController.ListGroups();
        panel.SetActive(true);
        playerCallback = callBack;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        playerCallback();
    }
}
