using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountUpTimer : MonoBehaviour
{
    private TextMeshProUGUI timerText;
    private float second;
    private int minute;

    // Start is called before the first frame update
    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
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