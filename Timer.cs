using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    float time, starTime;
    public Text timerTxt;
    // Start is called before the first frame update
    void Start()
    {
        starTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        time = Time.time - starTime;
        int sec = (int)time % 60;
        int minutes = (int)time / 60;
        string strTime = string.Format("{0:00}:{1:00}", minutes, sec);
        timerTxt.text = strTime;
    }
}
