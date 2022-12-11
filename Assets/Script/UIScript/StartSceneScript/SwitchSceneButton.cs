using UnityEngine;
using TMPro;

public class SwitchSceneButton : MonoBehaviour
{
    [SerializeField] GameObject mainButtown;
    [SerializeField] GameObject setButtown;

    private void Awake()
    {
        setButtown.SetActive(false);
    }
    public void SettingClick()
    {
        setButtown.SetActive(true);
        mainButtown.SetActive(false);
    }
}