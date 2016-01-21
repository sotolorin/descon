using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DimensionDraggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{  
    public bool isDragging = false;
    private GameObject anchor;

    void Awake()
    {
        anchor = new GameObject("Anchor");
        //anchor.hideFlags = HideFlags.HideAndDontSave;
    }

    public GameObject Anchor
    {
        get
        {
            if(anchor == null)
            {
                anchor = new GameObject("Anchor");
            }

            return anchor;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            //Mouse up
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
    }

    public void Clean()
    {
        if (anchor != null)
        {
            Destroy(anchor);
        }
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
            isDragging = true;
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
            isDragging = false;
    }
}
