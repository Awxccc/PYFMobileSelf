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

        
    }

    private void Start()
    {

    }


    public IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator LoadSceneAsyncWithLoadingScreen(string sceneName)
    {

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
                /*
                if (loadingScreenPanel != null)
                    loadingScreenPanel.SetActive(false);*/

                operation.allowSceneActivation = true;
            }
            loadingScreenPanel.SetActive(false);
            yield return null;
        }
        
        yield return new WaitForSeconds(0f);
        
    }
}