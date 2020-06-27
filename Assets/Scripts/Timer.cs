using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    float time, starTime;
    public static string timerTxt;

    [SerializeField] Text curTimer;// 유니티에서 사용하는 변수


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
        curTimer.text = strTime;
        //timerTxt = strTime;
    }
}
