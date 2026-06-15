using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
/** 
@brief Script to trigger fade in and fade out transition for whatever scene you want
*/
public class SceneTransitioner : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeInDuration = 1f;

    [SerializeField] private bool fadeInOnStart = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (fadeInOnStart)
        {
            StartCoroutine(FadeIn());
        }
    }

    private void Update()
    {
        /** 
        @brief Cheats to test the transition
        */
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            StartCoroutine(FadeOut());
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            StartCoroutine(FadeIn());
        }
    }

    /** 
    @brief Coroutines to transition scenes with fade in or out
    */
    public IEnumerator FadeOut()
    {
        animator.SetTrigger("SceneFade");
        yield return new WaitForSeconds(fadeOutDuration + 0.5f);
    }

    public IEnumerator FadeIn()
    {
        animator.SetTrigger("SceneReveal");
        yield return new WaitForSeconds(fadeInDuration);
    }

    public IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        yield return FadeOut();
        SceneManager.LoadScene(sceneName);
    }
}
