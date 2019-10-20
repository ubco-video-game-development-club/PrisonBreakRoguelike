using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  

public class GameManager : MonoBehaviour // Used to encapsulate SceneManager calls
{
 
    public void GenLevel()
    {
        SceneManager.LoadSceneAsync("level");
    }
}
