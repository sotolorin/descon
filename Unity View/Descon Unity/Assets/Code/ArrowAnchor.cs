using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrowAnchor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Draggable draggable;
    private bool isDragging = false;

    void Update()
    {
        if (draggable != null)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
                draggable.SetDraggingOffset();
        }

        if(isDragging)
        {
            //Move the anchor point
            if (draggable != null)
                draggable.SetOffsetPos(Input.mousePosition, Input.GetKey(KeyCode.LeftControl));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isDragging = true;

            if (Input.GetKey(KeyCode.LeftControl))
                draggable.SetDraggingOffset();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            isDragging = false;
    }
}
