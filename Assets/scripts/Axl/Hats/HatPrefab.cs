using UnityEngine;
/** 
@brief Code to handle each hat prefab
*/
public class HatPrefab : MonoBehaviour
{
    private Rigidbody rb;
    private bool frozen = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    /** 
    @brief Find either player or another hat and freeze upon impact and stay at that position
    */
    private void OnCollisionEnter(Collision collision)
    {
        if (frozen) return;

        PlayerMarker player = collision.gameObject.GetComponentInParent<PlayerMarker>();

        if (player != null)
        {
            FreezeHat();
            AttachToPlayer(player.transform);
        }
        else if (collision.gameObject.CompareTag("Hat"))
        {
            FreezeHat();
            transform.SetParent(collision.transform);
        }
    }

    /** 
    @brief Function to stop the movement of the hat and make it stay in place
    */
    private void FreezeHat()
    {
        frozen = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void AttachToPlayer(Transform player)
    {
        transform.SetParent(player, true);
    }
}
