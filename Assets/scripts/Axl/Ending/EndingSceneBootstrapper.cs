using UnityEngine;
/** 
@brief Add line renderer and move to player in the ending scene
*/
public class EndingSceneBootstrapper : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (!player) return;

        if (!player.TryGetComponent(out LineRenderer _))
            player.AddComponent<LineRenderer>();

        if (!player.TryGetComponent(out LineMover _))
            player.AddComponent<LineMover>();
    }
}
