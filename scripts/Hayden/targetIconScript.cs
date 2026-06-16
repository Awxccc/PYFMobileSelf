using UnityEngine;

public class targetIconScript : MonoBehaviour
{
    private Vector3 smallestSize = new Vector3(1, .6f, .6f);
    private Vector3 biggestSize = new Vector3(2, 1.3f, 1.3f);

    private float elapsedTime = 0;
    private float duration = 2;
    private bool bounced = false;

    float percent = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (elapsedTime < duration && !bounced)
        {
            elapsedTime += Time.deltaTime;
            percent = elapsedTime / duration;
        }
        else
        {
            bounced = true;
        }

        if (elapsedTime > 0 && bounced)
        {
            elapsedTime -= Time.deltaTime;
            percent = elapsedTime / duration;
        }
        else
        {
            bounced = false;
        }
        gameObject.transform.localScale = Vector3.Lerp(smallestSize, biggestSize, percent);
    }
}
