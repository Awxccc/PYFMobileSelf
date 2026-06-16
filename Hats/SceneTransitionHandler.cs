using UnityEngine;
using UnityEngine.SceneManagement;
/** 
@brief Handles loading of saved hats when scene changes
*/
public class SceneTransitionHandler : MonoBehaviour
{
    public static SceneTransitionHandler Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            if (Player_data.instance != null)
            {
                player = Player_data.instance.gameObject;
            }
        }

        if (player != null && HatManager.Instance != null)
        {
            Debug.Log($"Found player, updating hat manager");

            HatManager.Instance.OnSceneChanged(player.transform);

            if (Player_data.instance != null)
            {
                HatManager.Instance.ClearAllHats();
                Player_data.instance.LoadOwnedHats();
            }
        }
        else
        {
            Debug.LogWarning("Could not find player or HatManager instance");
        }
    }
}
