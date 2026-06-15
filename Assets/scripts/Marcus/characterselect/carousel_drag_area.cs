using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class carousel_drag_area : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public character_select carousel_script;
    public void OnDrag(PointerEventData eventData)
    {
        float deltaX = -eventData.delta.x;
        carousel_script.target_angle = carousel_script.target_angle + deltaX * carousel_script.dragspeed;
        
        carousel_script.light_transform.gameObject.SetActive(false);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        carousel_script.dragging = true;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        carousel_script.enddrag();
    }
}
