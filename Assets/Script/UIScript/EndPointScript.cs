using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPointScript : MonoBehaviour
{

    //�Q�[���I��
    public void Click()
    {
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
//#else
//    Application.Quit();//�Q�[���v���C�I��
//#endif
    }
    public void OnClickStartButton()
    {
        SceneManager.LoadScene("StartScene");
    }
}