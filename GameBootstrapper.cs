using UnityEngine;
using System.Collections;

public class GameBootstrapper : MonoBehaviour
{
    [Tooltip("The exact name of your Main Menu or starting game scene")]
    [SerializeField] private string firstSceneToLoad = "start";

    private void Start()
    {
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        yield return null;

        if (SceneTransitioner.Instance != null)
        {
            StartCoroutine(SceneTransitioner.Instance.LoadSceneAsyncWithLoadingScreen(firstSceneToLoad));
        }
        else
        {
            Debug.LogError("SceneTransitioner is missing from the Boot Scene!");
        }
    }
}