using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScene : MonoBehaviour
{
    [SerializeField] GameObject mainButtown;
    [SerializeField] GameObject setButtown;

    private void Awake()
    {
    }
    public void SettingClick()
    {
        setButtown.SetActive(false);
        mainButtown.SetActive(true);
    }
}
