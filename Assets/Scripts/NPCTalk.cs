using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class NPCTalk : MonoBehaviour
{
    //public string CharacterId;
    //[SerializeField] private AudioSource audioSource;
    //[SerializeField] private AudioClip sfxClip;

    [YarnCommand("switchScene")]
    public static void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /*[YarnCommand("playSFX")]
    public void PlaySFX(string clipName)
    {
        if (audioSource == null || sfxClip == null)
        {
            Debug.LogWarning($"{nameof(NPCTalk)} on {gameObject.name} is missing an AudioSource or AudioClip.");
            return;
        }

        audioSource.PlayOneShot(sfxClip);
    }*/
}
