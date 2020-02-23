using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    public void LoadMainMenu()
    {
        Destroy(LevelController.instance.gameObject);
        SceneManager.LoadSceneAsync("Title");
    }
}
