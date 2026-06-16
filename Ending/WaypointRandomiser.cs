using UnityEngine;
/** 
@brief Randomises waypoints in the ending scene to give every path a unique look
*/
public class WaypointRandomiser : MonoBehaviour
{
    /** 
    @brief Determine how many rows and columns to randomise. Rows will be determined by your gameplay, columns will stay at 3
    */
    [Header("Waypoint Settings")]
    [SerializeField] private GameObject wpPrefab;
    private int rows = 5;
    [SerializeField] private int columns = 3;
    [SerializeField] private float maxJitter = 0.3f;

    [Header("Spawn Area")]
    [SerializeField] private Transform areaObject;

    private void Start()
    {
        rows = Player_data.instance.progress_log.Count;

        GenerateWaypoints();
    }
    /** 
    @brief Generate waypoints within the space of the sprite used to mark out the mountain
    */
    private void GenerateWaypoints()
    {
        Bounds areaBounds;

        if (areaObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
        {
            areaBounds = sr.bounds;
        }
        else
        {
            areaBounds = new Bounds(
                areaObject.position,
                new Vector3(areaObject.localScale.x, areaObject.localScale.y, 0f)
            );
        }

        float rowSpacing = areaBounds.size.y / rows;
        float colSpacing = (columns > 1) ? areaBounds.size.x / (columns) : 0f;

        for (int r = 0; r < rows; r++)
        {
            float rowY = areaBounds.max.y - (r + 0.5f) * rowSpacing;

            for (int c = 0; c < columns; c++)
            {
                float colX = areaBounds.min.x + (c + 0.5f) * colSpacing;

                float jitterX = Random.Range(-maxJitter, maxJitter);
                float jitterY = Random.Range(-maxJitter, maxJitter);

                Vector3 pos = new Vector3(
                Mathf.Clamp(colX + jitterX, areaBounds.min.x, areaBounds.max.x), Mathf.Clamp(rowY + jitterY, areaBounds.min.y, areaBounds.max.y), transform.position.z);

                Instantiate(wpPrefab, pos, Quaternion.identity, transform);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (areaObject == null) return;

        if (areaObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(sr.bounds.center, sr.bounds.size);
        }
    }
}
