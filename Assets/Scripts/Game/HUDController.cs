using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    public static HUDController instance = null;

    public GameObject deathScreen;

    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;

        deathScreen.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        deathScreen.SetActive(true);
    }

    // called from death screen button
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
