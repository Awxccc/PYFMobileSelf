using UnityEngine;
/** 
@brief Make the hat feedback text persistent
*/
public class HatFeedbackScript : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
