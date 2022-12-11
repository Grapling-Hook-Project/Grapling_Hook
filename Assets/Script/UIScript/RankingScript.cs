using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class RankingScript : MonoBehaviour
{
    public TextMeshProUGUI firstText;
    public TextMeshProUGUI secondText;
    public TextMeshProUGUI thirdText;

    // Start is called before the first frame update
    void Start()
    {
        if (GradeAnnounceScript.firstRankMinute == 59 && GradeAnnounceScript.firstRankSecond == 59)
        {
            firstText.text = "1st:    " + GradeAnnounceScript.firstRankMinute.ToString("--") + ":" + GradeAnnounceScript.firstRankSecond.ToString("--");
        }
        else
        {
            firstText.text = "1st:    " + GradeAnnounceScript.firstRankMinute.ToString("00") + ":" + GradeAnnounceScript.firstRankSecond.ToString("00");
        }

        if (GradeAnnounceScript.secondRankMinute == 59 && GradeAnnounceScript.secondRankSecond == 59)
        {
            secondText.text = "2nd:    " + GradeAnnounceScript.secondRankMinute.ToString("--") + ":" + GradeAnnounceScript.secondRankSecond.ToString("--");
        }
        else
        {
            secondText.text = "2nd:    " + GradeAnnounceScript.secondRankMinute.ToString("00") + ":" + GradeAnnounceScript.secondRankSecond.ToString("00");
        }

        if (GradeAnnounceScript.thirdRankMinute == 59 && GradeAnnounceScript.thirdRankSecond == 59)
        {
            thirdText.text = "3rd:    " + GradeAnnounceScript.thirdRankMinute.ToString("--") + ":" + GradeAnnounceScript.thirdRankSecond.ToString("--");
        }
        else
        {
            thirdText.text = "3rd:    " + GradeAnnounceScript.thirdRankMinute.ToString("00") + ":" + GradeAnnounceScript.thirdRankSecond.ToString("00");
        }

    }
}
