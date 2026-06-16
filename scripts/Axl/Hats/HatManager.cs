using UnityEngine;
using System.Collections.Generic;
/** 
@brief In charge of the hat placement itself 
*/
public class HatManager : MonoBehaviour
{
    public static HatManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Hat Positioning")]
    [SerializeField] private float baseVerticalOffset = 1.07f; // Offset from player's head
    [SerializeField] private float verticalSpacing = 0.3f; // Spacing between stacked hats
    [SerializeField] private float hatRotationSpeed = 15f;

    [Header("Hat Animation")]
    [SerializeField] private float dropHeight = 2.0f; // Height from which hats drop
    [SerializeField] private float dropDuration = 0.5f; // Time for drop animation
    [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Store all placed hats
    private List<GameObject> placedHats = new List<GameObject>();
    private List<Coroutine> dropCoroutines = new List<Coroutine>();
    private GameObject playerHatAnchor; // Parent object for all hats
    [SerializeField] private Transform headTransform;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create hat anchor point
        if (playerHatAnchor == null)
        {
            playerHatAnchor = new GameObject("HatAnchor");
            DontDestroyOnLoad(playerHatAnchor);
        }
    }

    void Start()
    {
        FindPlayer();
        FindHead();
        SetupHatAnchor();
        LoadHats();
    }


    void OnDestroy()
    {
        // Clean up coroutines
        foreach (var coroutine in dropCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }
    /** 
    @brief Locate player to place hats on
    */
    private void FindPlayer()
    {
        if (playerTransform != null) return;

        GameObject playerMain = GameObject.FindGameObjectWithTag("Player");
        if (playerMain != null)
        {
            // Find child
            Transform childPlayer = playerMain.transform.Find("Player");

            if (childPlayer != null)
            {
                playerTransform = childPlayer;
                return;
            }

            Debug.LogWarning("HatManager: Player child not found under Player_main.");
        }

        if (Player_data.instance != null)
        {
            Transform childPlayer = Player_data.instance.transform.Find("Player");
            if (childPlayer != null)
            {
                playerTransform = childPlayer;
            }
        }
    }
    /** 
    @brief Setup a hat anchor that will be used to parent the hats
    */
    private void SetupHatAnchor()
    {
        if (headTransform == null) return;

        if (playerHatAnchor == null)
        {
            playerHatAnchor = new GameObject("HatAnchor");
            DontDestroyOnLoad(playerHatAnchor);
        }

        playerHatAnchor.transform.SetParent(headTransform, false);
        playerHatAnchor.transform.localPosition = Vector3.zero;
        playerHatAnchor.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
    }

    /** 
    @brief Hat placement physics
    */
    public void PlaceHat(hat.hat_data hatToPlace)
    {
        if (hatToPlace == null || hatToPlace.hat_model == null) return;
        if (playerHatAnchor == null) return;

        // Calculate final position
        float finalHeight = baseVerticalOffset + (placedHats.Count * verticalSpacing);

        // Create new hat at drop position
        float topHatHeight = GetCurrentTopHeight();

        float dropStartHeight = Mathf.Max(
            finalHeight + dropHeight,
            topHatHeight + verticalSpacing + 0.2f
        );

        Vector3 dropStartPosition = new Vector3(0, dropStartHeight, 0);
        GameObject newHat = Instantiate(hatToPlace.hat_model);

        newHat.transform.SetParent(playerHatAnchor.transform, false);
        newHat.transform.localPosition = dropStartPosition;

        // Parent to hat anchor
        newHat.transform.parent = playerHatAnchor.transform;

        // Add to list
        placedHats.Add(newHat);

        // Animate the drop
        StartCoroutine(AnimateHatDrop(newHat, dropStartPosition, finalHeight));

        // Save hats
        SaveHats();

        Debug.Log($"Hat placed at height {finalHeight}! Total hats: {placedHats.Count}");
    }
    /** 
    @brief Coroutine to make the hat drop smoother
    */
    private System.Collections.IEnumerator AnimateHatDrop(GameObject hat, Vector3 startLocalPos, float finalHeight)
    {
        float elapsedTime = 0f;
        Vector3 startPos = hat.transform.localPosition;
        Vector3 endPos = new Vector3(0, finalHeight, 0);

        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dropDuration;
            float curveValue = dropCurve.Evaluate(t);

            hat.transform.localPosition = Vector3.Lerp(startPos, endPos, curveValue);

            if (t > 0.8f)
            {
                float bounce = Mathf.Sin((t - 0.8f) * Mathf.PI * 5) * 0.1f * (1 - (t - 0.8f) / 0.2f);
                hat.transform.localPosition += Vector3.up * bounce;
            }

            yield return null;
        }

        hat.transform.localPosition = endPos;
    }

    public void ClearAllHats()
    {
        foreach (GameObject hat in placedHats)
        {
            if (hat != null) Destroy(hat);
        }
        placedHats.Clear();

        foreach (var coroutine in dropCoroutines)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        dropCoroutines.Clear();

        ClearSavedHats();
    }

    public int GetHatCount()
    {
        return placedHats.Count;
    }

    private void SaveHats()
    {
        PlayerPrefs.SetInt("HatCount", placedHats.Count);
        PlayerPrefs.Save();
    }

    private void LoadHats()
    {
        int savedCount = PlayerPrefs.GetInt("HatCount", 0);
        Debug.Log($"Loaded {savedCount} hats from save");
    }

    private void ClearSavedHats()
    {
        PlayerPrefs.SetInt("HatCount", 0);
        PlayerPrefs.Save();
    }

    // Called when changing scenes to ensure hats persist
    public void OnSceneChanged(Transform newPlayerTransform)
    {
        playerTransform = newPlayerTransform;

        if (playerHatAnchor == null)
        {
            playerHatAnchor = new GameObject("HatAnchor");
            DontDestroyOnLoad(playerHatAnchor);
        }

        if (playerHatAnchor != null && playerTransform != null)
        {
            playerHatAnchor.transform.parent = playerTransform;
            playerHatAnchor.transform.localPosition = Vector3.zero;
            playerHatAnchor.transform.localRotation = Quaternion.identity;

            for (int i = 0; i < placedHats.Count; i++)
            {
                if (placedHats[i] != null)
                {
                    float height = baseVerticalOffset + (i * verticalSpacing);
                    placedHats[i].transform.localPosition = new Vector3(0, height, 0);
                }
            }
        }
    }

    private float GetCurrentTopHeight()
    {
        if (placedHats.Count == 0)
            return baseVerticalOffset;

        return baseVerticalOffset + ((placedHats.Count - 1) * verticalSpacing);
    }
    /** 
    @brief Find the exact game object that the hats should be parented to
    */
    private void FindHead()
    {
        if (playerTransform == null) return;

        headTransform = playerTransform.Find(
            "MC_combat_idle/Hip_Bone_Offset/Hip/Waist/Clavical/Head"
        );

        if (headTransform == null)
        {
            Debug.LogError("HatManager: Head bone not found!");
        }
    }
}
