using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/** @brief Script to trigger fade in, fade out, and handle async loading screens
*/
public class SceneTransitioner : MonoBehaviour
{
    public static SceneTransitioner Instance { get; private set; }

    private Animator animator;

    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private bool fadeInOnStart = true;

    [Header("Loading Screen UI")]
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

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
        if (Input.GetKeyDown(KeyCode.Alpha0)) StartCoroutine(FadeOut());
        if (Input.GetKeyDown(KeyCode.Alpha9)) StartCoroutine(FadeIn());
    }

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

    public IEnumerator LoadSceneAsyncWithLoadingScreen(string sceneName)
    {
        yield return FadeOut();

        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = (progress * 100f).ToString("F0") + "%";

            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);

                if (loadingScreenPanel != null)
                    loadingScreenPanel.SetActive(false);

                operation.allowSceneActivation = true;
            }

            yield return null;
        }
        yield return FadeIn();
    }
}