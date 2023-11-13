using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    public GameObject panel;
    public GameObject confirmationPanel;
    public GameObject SkillPanel;

    public SkillsManager sM;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenPanel()
    {
        panel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        Time.timeScale = 1;
    }
    public void OpenConfirmationPanel()
    {
        panel.SetActive(false);
        confirmationPanel.SetActive(true);
    }
    public void CloseConfirmationPanel()
    {
        panel.SetActive(true);
        confirmationPanel.SetActive(false);   
    }
    public void OpenSkillsPanel()
    {
        SkillPanel.SetActive(true);
        panel.SetActive(false);
        sM.UpdateSPDisplay();
    }
    public void CloseSkillsPanel()
    {
        SkillPanel.SetActive(false);
        panel.SetActive(false);
        Time.timeScale = 1;
    }
}
