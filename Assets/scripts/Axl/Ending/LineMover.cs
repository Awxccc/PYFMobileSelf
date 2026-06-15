using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
/** 
@brief Handles the line renderer used in the ending scene, ensuring a completely unique and random path for each and every different player
*/
public class LineMover : MonoBehaviour
{
    [Header("Waypoints and Randomness")]
    [SerializeField] private Transform waypointParent;
    private int numberOfWaypointsToUse = 5;
    [SerializeField] private Transform endWaypoint;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rowHeightTolerance = 0.5f;
    private bool isReversing = false;
    private int rewindTargetIndex = -1;
    private int resumeForwardIndex = -1;
    private Transform[] waypoints;
    private GameObject[] wpTick;
    private GameObject[] wpCross;
    private int currentWaypoint = 0;
    private bool reachedEnd = false;
    private bool waitingAfterReverse = false;

    [Header("Line Visualiser")]
    private LineRenderer line;
    private int pointIndex = 0;
    private float distanceThreshold = 0.1f;

    [Header("Jitter and Random Movement")]
    [SerializeField] private float jitterStrength = 0.15f;
    [SerializeField] private float jitterFrequency = 2f;
    private float noiseTime;

    [Header("Player Feedback")]
    private bool isWaiting = false;
    private SpriteRenderer playerSprite;
    [SerializeField] private TextMeshProUGUI eventText;
    private Coroutine waitCoroutine;

    [Header("Scene Transitioner")]
    [SerializeField] private SceneTransitioner sceneTransitioner;

    void Start()
    {
        numberOfWaypointsToUse = Player_data.instance.progress_log.Count;

        line.positionCount = 1;
        line.SetPosition(0, transform.position);

        StartCoroutine(InitWaypoints());
    }

    void Awake()
    {
        sceneTransitioner ??= FindAnyObjectByType<SceneTransitioner>();

        //eventText = FindFirstObjectByType<TextMeshProUGUI>(FindObjectsInactive.Include);

        playerSprite = GetComponent<SpriteRenderer>();
        line = GetComponent<LineRenderer>();

        // Line setup
        line.widthMultiplier = 0.8f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.useWorldSpace = true;
        line.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SkipWaiting();
        }

        if (isWaiting || reachedEnd)
        {
            return;
        }

        if (waypoints == null || currentWaypoint >= waypoints.Length)
        {
            return;
        }

        Transform target;

        if (isReversing)
        {
            target = waypoints[rewindTargetIndex];
        }
        else
        {
            target = waypoints[currentWaypoint];
        }

        Vector3 direction = (target.position - transform.position).normalized;

        // Jitter and random movement
        // Perpendicular direction
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

        // Animate noise over time
        noiseTime += Time.deltaTime * jitterFrequency;
        float noise = Mathf.PerlinNoise(noiseTime, 0f) - 0.5f;

        // Apply jitter sideways
        Vector3 jitterOffset = perpendicular * noise * jitterStrength;

        Vector3 desiredPosition = transform.position + direction * speed * Time.deltaTime + jitterOffset;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, line.GetPosition(pointIndex)) > distanceThreshold)
        {
            pointIndex++;
            line.positionCount = pointIndex + 1;
            noiseTime += Time.deltaTime * jitterFrequency;

            line.SetPosition(pointIndex, transform.position);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            if (isReversing)
            {
                isReversing = false;
                StartCoroutine(PauseAtReversedWaypoint(1f));
                return;
            }

            if (currentWaypoint == waypoints.Length - 1)
            {
                reachedEnd = true;
                StartCoroutine(FadeDisableAndLoad());
                return;
            }

            int pathTaken = Player_data.instance.progress_log[currentWaypoint].path_taken;

            if (pathTaken == 3)
            {
                wpCross[currentWaypoint].SetActive(true);
                waitCoroutine = StartCoroutine(WaitThenReverse(3f));
                return;
            }
            else
            {
                if (pathTaken != 4)
                    wpTick[currentWaypoint].SetActive(true);
                else
                    wpCross[currentWaypoint].SetActive(true);

                waitCoroutine = StartCoroutine(WaitThenAdvance(3f));
            }
        }
    }

    private IEnumerator InitWaypoints()
    {
        yield return null;

        var allWaypoints = waypointParent.GetComponentsInChildren<Transform>().Where(t => t != waypointParent && t != endWaypoint).ToList();

        allWaypoints.Sort((a, b) => a.position.y.CompareTo(b.position.y));

        var rows = new List<List<Transform>>();

        foreach (var wp in allWaypoints)
        {
            bool added = false;

            foreach (var row in rows)
            {
                if (Mathf.Abs(row[0].position.y - wp.position.y) <= rowHeightTolerance)
                {
                    row.Add(wp);
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                rows.Add(new List<Transform> { wp });
            }
        }

        int rowsToUse = Mathf.Min(numberOfWaypointsToUse, rows.Count);

        waypoints = new Transform[rowsToUse + 1];

        // Waypoints are determined here
        for (int i = 0; i < rowsToUse; i++)
        {
            var row = rows[i];

            // Sort row left to right
            row.Sort((a, b) => a.position.x.CompareTo(b.position.x));

            int pathTaken = Player_data.instance.progress_log[i].path_taken;

            // Clamp according to path_taken
            pathTaken = Mathf.Clamp(pathTaken, 0, row.Count - 1);

            waypoints[i] = row[pathTaken];
        }

        waypoints[rowsToUse] = endWaypoint;

        wpTick = new GameObject[waypoints.Length];
        wpCross = new GameObject[waypoints.Length];

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Transform tick = waypoints[i].Find("Tick");
            Transform cross = waypoints[i].Find("Cross");

            wpTick[i] = tick.gameObject;
            wpTick[i].SetActive(false);

            wpCross[i] = cross.gameObject;
            wpCross[i].SetActive(false);
        }

        currentWaypoint = 0;
    }

    private IEnumerator WaitThenAdvance(float seconds)
    {
        eventText.text = Player_data.instance.progress_log[currentWaypoint].scene_description;
        eventText.gameObject.SetActive(true);
        isWaiting = true;
        yield return new WaitForSeconds(seconds);
        currentWaypoint++;
        isWaiting = false;
        eventText.gameObject.SetActive(false);
        waitCoroutine = null;
    }

    private IEnumerator WaitThenReverse(float seconds)
    {
        eventText.text = Player_data.instance.progress_log[currentWaypoint].scene_description;
        eventText.gameObject.SetActive(true);
        isWaiting = true;

        yield return new WaitForSeconds(seconds);

        eventText.gameObject.SetActive(false);
        isWaiting = false;

        rewindTargetIndex = Mathf.Max(currentWaypoint - 1, 0);
        resumeForwardIndex = currentWaypoint + 1;

        isReversing = true;

        waitCoroutine = null;
    }

    private IEnumerator PauseAtReversedWaypoint(float seconds)
    {
        isWaiting = true;
        waitingAfterReverse = true;

        yield return new WaitForSeconds(seconds);

        isWaiting = false;
        waitingAfterReverse = false;

        currentWaypoint = resumeForwardIndex;
    }

    private IEnumerator FadeDisableAndLoad()
    {
        yield return sceneTransitioner.FadeOut();

        playerSprite.enabled = false;
        line.enabled = false;

        yield return null;

        SceneManager.LoadScene("PlayerTypeScene");
    }

    private void SkipWaiting()
    {
        if (!isWaiting)
            return;

        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        eventText.gameObject.SetActive(false);
        isWaiting = false;

        if (isReversing)
        {
            isReversing = false;
            currentWaypoint = resumeForwardIndex;
        }
        else
        {
            currentWaypoint++;
        }
    }
}
