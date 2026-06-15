using UnityEngine;
/** 
@brief Makes BGM persistent and play throughout the game
*/
public class BGM : MonoBehaviour
{
    public static BGM instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
