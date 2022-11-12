using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitelScript : MonoBehaviour
{
    private TextMeshProUGUI text;
    private float flashTiem;
    private float flashSpeed = 1.0f;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    
    //Color WaitColor(Color color)
    //{
    //    flashTiem += Time.deltaTime * 5.0f * flashSpeed;
    //    color.a = Mathf.Sin(flashTiem);
    //    return color;
    //}
    Color GetAlphaColor(Color color)
    {
        flashTiem += Time.deltaTime * 5.0f * flashSpeed;
        color.a = Mathf.Sin(flashTiem) * 0.5f + 0.5f;

        return color;
    }
}
