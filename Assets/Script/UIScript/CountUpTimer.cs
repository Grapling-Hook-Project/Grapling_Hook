using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountUpTimer : MonoBehaviour
{
    private TextMeshProUGUI timerText;
    //RankingScriptで使用 
    public static float second = 0;
    public static int minute = 0;
    //GoalScriptで使用
    public static bool timerStop;

    // Start is called before the first frame update
    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        timerStop = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerStop == true)
        {
            second += Time.deltaTime;

            if (second > 60f)
            {
                minute += 1;
                second = 0;
            }
            timerText.text = minute.ToString("00") + ":" + second.ToString("f2");
        }
    }
}