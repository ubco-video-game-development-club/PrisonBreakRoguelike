using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  

public class GameManager : MonoBehaviour // Used to encapsulate SceneManager calls
{
    private bool isPaused;

    void Update()
    {
        TogglePause();

        if(Input.GetKeyDown(KeyCode.Escape)) 
        {
            isPaused = !isPaused;
        }

    }


    public void GenLevel()
    {
        SceneManager.LoadSceneAsync("level");
        isPaused = false;
    }
    
    public void TogglePause()
    {
        if(isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void OnGUI() //Draw pause buttons
    {
        if(isPaused)
        {
            if (GUI.Button(new Rect(10, 10, 150, 100), "Continue"))
            {
                isPaused = !isPaused;
                TogglePause();
            }
        }
    }
        

}
