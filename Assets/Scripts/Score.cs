using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    int score;
    public static int currentScore = 0;

    [SerializeField] Text curScore;// 유니티에서 사용하는 변수

    // Update is called once per frame
    void Update()
    {
        score = currentScore;
        curScore.text = score.ToString();
    }
}
