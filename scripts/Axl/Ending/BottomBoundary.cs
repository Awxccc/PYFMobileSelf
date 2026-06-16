using UnityEngine;
using System.Collections;
/** 
@brief Ensures the player never goes past the bottom of the screen in the ending scene
*/
public class BottomBoundary : MonoBehaviour
{
    [SerializeField] private float pushUpAmount = 0.3f;
    [SerializeField] private float pushDuration = 0.25f;

    private Coroutine pushRoutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (pushRoutine != null)
            {
                StopCoroutine(pushRoutine);
            }

            pushRoutine = StartCoroutine(PushUp(collision.transform));
        }
    }

    private IEnumerator PushUp(Transform player)
    {
        Vector3 startPos = player.position;
        Vector3 targetPos = startPos + Vector3.up * pushUpAmount;

        float elapsed = 0f;

        while (elapsed < pushDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pushDuration;

            t = t * t * (3f - 2f * t);

            player.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        player.position = targetPos;
    }
}
