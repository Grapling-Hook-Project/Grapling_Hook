using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GradeAnnounceScript : MonoBehaviour
{
    public static int firstRankMinute = 59;
    public static float firstRankSecond = 59;
    public static int secondRankMinute = 59;
    public static float secondRankSecond = 59;
    public static int thirdRankMinute = 59;
    public static float thirdRankSecond = 59;
    public static float NewTimer = 0;

    private float firstRankTimer = 10000;
    private float secondRankTimer = 10000;
    private float thirdRankTimer = 10000;

    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI timeResultText;
    public TextMeshProUGUI onlyRankText;
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        NewTimer = TimerCounter.minute * 60 + TimerCounter.second;
        if (NewTimer <= firstRankTimer)
        {
            thirdRankMinute = secondRankMinute;
            thirdRankSecond = secondRankSecond;
            thirdRankTimer = secondRankTimer;
            secondRankMinute = firstRankMinute;
            secondRankSecond = firstRankSecond;
            secondRankTimer = firstRankTimer;
            firstRankMinute = TimerCounter.minute;
            firstRankSecond = TimerCounter.second;
            firstRankTimer = NewTimer;
            Debug.Log(firstRankTimer);
        }
        else if (NewTimer > firstRankTimer && NewTimer <= secondRankTimer)
        {

            thirdRankMinute = secondRankMinute;
            thirdRankSecond = secondRankSecond;
            thirdRankTimer = secondRankTimer;
            secondRankMinute = TimerCounter.minute;
            secondRankSecond = TimerCounter.second;
            secondRankTimer = NewTimer;
        }
        else if (NewTimer > secondRankTimer && NewTimer <= thirdRankTimer)
        {
            thirdRankMinute = TimerCounter.minute;
            thirdRankSecond = TimerCounter.second;
            thirdRankTimer = NewTimer;
        }

        Invoke("Grade", 1);
    }
    void Grade()
    {
        //NewTimer = 70;
        if (NewTimer <= 60)
        {
            gradeText.text = "S";
            gradeText.color = new Color(1.0f, 0.83f, 0.106f, 1.0f);
        }
        else if (NewTimer > 60 && NewTimer <= 80)
        {
            gradeText.text = "A";
            gradeText.color = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        }
        else if (NewTimer > 80 && NewTimer <= 100)
        {
            gradeText.text = "B";
            gradeText.color = new Color(0.9f, 0.63f, 0.0f, 1.0f);
        }
        else
        {
            gradeText.text = "C";
            gradeText.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        }
        timeResultText.text = TimerCounter.minute.ToString("00") + ":" + TimerCounter.second.ToString("00");
        onlyRankText.text = "Clear Time:";
    }

    // Update is called once per frame

}