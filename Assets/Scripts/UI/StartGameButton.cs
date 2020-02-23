using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    LevelController levelController;

    void Start()
    {
        levelController = LevelController.instance;
    }

    public void StartGame()
    {
        levelController.LoadFirstLevel();
    }
}
