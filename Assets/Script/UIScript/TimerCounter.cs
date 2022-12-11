using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerCounter : MonoBehaviour
{
    //    private TextMeshProUGUI timerText;
    public static float second = 0;
    public static int minute = 0;
    public static bool timerStop = true;
    // Start is called before the first frame update
    void Start()
    {
        //        timerText = GetComponent<TextMeshProUGUI>();
        second = 0;
        minute = 0;
        timerStop = true;
    }

    // Update is called once per frame
    void Update()
    {
        //second += Time.deltaTime;

        //if (second > 60f)
        //{
        //    minute += 1;
        //    second = 0;
        //}
        //Text Timer = GetComponent<Text>();
        //Timer.text = minute.ToString("00") + ":" + second.ToString("f2");

        if (timerStop == true)
        {
            second += Time.deltaTime;

            if (second > 60f)
            {
                minute += 1;
                second = 0;
            }
            Text Timer = GetComponent<Text>();
            Timer.text = minute.ToString("00") + ":" + second.ToString("f2");
        }
    }
}
