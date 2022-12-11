using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPointScript : MonoBehaviour
{

    //ゲーム終了
    public void Click()
    {
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
//#else
//    Application.Quit();//ゲームプレイ終了
//#endif
    }
    public void OnClickStartButton()
    {
        SceneManager.LoadScene("StartScene");
    }
}