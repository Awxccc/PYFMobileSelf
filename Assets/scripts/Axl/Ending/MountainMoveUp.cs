using UnityEngine;
/** 
@brief Player sprite to move up the mountain in the ending scene
*/
public class MountainMoveUp : MonoBehaviour
{
    [SerializeField] private float targetY = 5f;
    [SerializeField] private float speed = 2f;

    private bool shouldMove = true;

    void Update()
    {
        if (!shouldMove) return;

        Vector3 position = transform.position;

        if (position.y < targetY)
        {
            position.y += speed * Time.deltaTime;
            transform.position = position;
        }
        else
        {
            transform.position = new Vector3(position.x, targetY, position.z);
            shouldMove = false;
        }
    }
}
