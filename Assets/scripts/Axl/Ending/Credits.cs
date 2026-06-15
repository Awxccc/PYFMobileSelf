using UnityEngine;
/** 
@brief Simple script to make text move up in the credits, while the title will stop in the centre of the screen
*/
public class Credits : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 40f;
    private RectTransform rectTransform;
    private bool stopAtMiddle;

    private void Start()
    {
        rectTransform  = GetComponent<RectTransform>();
        stopAtMiddle = CompareTag("Logo");
    }

    private void Update()
    {
        if (stopAtMiddle && rectTransform.anchoredPosition.y >= 0f) return;

        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
    }
}
