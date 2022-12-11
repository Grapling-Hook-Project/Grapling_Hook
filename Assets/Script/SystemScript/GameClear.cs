using UnityEngine;
using UnityEngine.SceneManagement;

public class GameClear : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            TimerCounter.timerStop = false;
                LoadEndingScene();
        }
    }
    void LoadEndingScene()
    {
        SceneManager.LoadScene("ClearScene");
    }
}
