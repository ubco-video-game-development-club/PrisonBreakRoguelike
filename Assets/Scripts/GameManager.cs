using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  

public class GameManager : MonoBehaviour // Used to encapsulate SceneManager calls
{
    public float pauseWidth, pauseHeight;
    
    public float pauseMargin;
    public float buttonWidth, buttonHeight;
    private bool isPaused;
    void Update() 
    {
        TogglePause();

        if(Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name == "Level") //Game will only pause during gameplay
        {
            isPaused = !isPaused;
        }

    }


    public void GenLevel()
    {
        SceneManager.LoadSceneAsync("Level");
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
        float pauseX = Screen.width / 2 - pauseWidth / 2;
        float pauseY = Screen.height / 2 - pauseHeight / 2;
        
        if(isPaused) //Subject to change in accordance to design agreed on by dev. and art teams
        {
            //Pause menu box 
            GUI.Box(new Rect(pauseX, pauseY, pauseWidth, pauseHeight), "Paused");
            
            //Button to unpause
            if (GUI.Button(new Rect(pauseX + pauseMargin , pauseHeight/2 + buttonHeight, buttonWidth, buttonHeight),"Continue"))
            {
                isPaused = !isPaused;
                TogglePause();
            }

            //Button to quit to main menu
            if (GUI.Button(new Rect(pauseX + pauseMargin , pauseHeight/2 + buttonHeight * 2, buttonWidth, buttonHeight),"Main Menu"))
            {
                SceneManager.LoadSceneAsync("Title");
            }    

        }
    }
        

}
