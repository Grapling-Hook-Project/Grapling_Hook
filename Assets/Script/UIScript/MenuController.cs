using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject MainMenuPannel;
    public GameObject SettingPannel;
    public GameObject ManualPannel;

    public void CloseGame()
    {
        UnityEngine.Application.Quit();
    }

    public void ToSetting()
    {
        MainMenuPannel.SetActive(false);
        SettingPannel.SetActive(true);
    }

    public void ToManual()
    {
        MainMenuPannel.SetActive(false);
        ManualPannel.SetActive(true);
    }

    public void ToMain()
    {
        SettingPannel.SetActive(false);
        ManualPannel.SetActive(false);
        MainMenuPannel.SetActive(true);
    }
}
