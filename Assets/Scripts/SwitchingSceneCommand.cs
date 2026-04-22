using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class SwitchingSceneCommand : MonoBehaviour
{
    [YarnCommand("switchScene")]
    public static void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
