using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Screen.SetResolution(1280, 720, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartGameScene()
    {
        SceneManager.LoadScene("Game");
    }

    public void ExitGameScene()
    {
        Application.Quit();
    }
}
