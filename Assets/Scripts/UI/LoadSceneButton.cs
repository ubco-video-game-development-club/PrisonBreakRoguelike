using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : MonoBehaviour
{
    /// <summary> The name of the scene this button should load. </summary>
    [Tooltip("The name of the scene this button should load.")]
    public string targetSceneName;

    public void LoadTargetScene()
    {
        SceneManager.LoadSceneAsync(targetSceneName);
    }
}
