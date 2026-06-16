using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
public class onclick_btn : MonoBehaviour,IPointerClickHandler
{//this is mostly just a wrapper for unityevents to make buttons easier to use
    public UnityEvent onClick;


    public virtual void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}
    