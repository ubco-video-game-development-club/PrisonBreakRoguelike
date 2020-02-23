using UnityEngine;

public class RestartButton : MonoBehaviour
{
    LevelController levelController;

    void Start()
    {
        levelController = LevelController.instance;
    }

    public void RestartGame()
    {
        levelController.LoadFirstLevel();
    }
}
