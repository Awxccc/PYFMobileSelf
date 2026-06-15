using UnityEngine;
/** 
@brief Handles the animation of the other characters in the ending scene moving up
*/
public class EndingAnim : MonoBehaviour
{
    [SerializeField] private Animator sdmHeadAnimator;
    [SerializeField] private Animator sitHeadAnimator;
    [SerializeField] private Animator sbmHeadAnimator;

    private void Start()
    {
        sdmHeadAnimator = sdmHeadAnimator.GetComponent<Animator>();
        sdmHeadAnimator.SetTrigger("MoveUpMountainSDM");

        sitHeadAnimator = sitHeadAnimator.GetComponent<Animator>();
        sitHeadAnimator.SetTrigger("MoveUpMountainSIT");

        sbmHeadAnimator = sbmHeadAnimator.GetComponent<Animator>();
        sbmHeadAnimator.SetTrigger("MoveUpMountainSBM");
    }
}
