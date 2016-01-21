using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConnectionControl : MonoBehaviour
{
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.white;
    public bool isSelected = false;
    public float highlightStrength = 0.1f;
    public Descon.Data.EMemberType parentMemberType;
    public Descon.Data.EMemberSubType subMemberType;
    public ViewMask viewMask;
    public Dictionary<ViewMask, Drawable> drawables;

    public Dictionary<ViewMask, List<CustomLine>> customLines;
    public bool useCustomLines = false;
    public float customLineOffset = 0.001f;

    private RaycastHit hit;

    void Awake()
    {
        customLines = new Dictionary<ViewMask, List<CustomLine>>();

        customLines.Add(ViewMask.D3, new List<CustomLine>());
        customLines.Add(ViewMask.FRONT, new List<CustomLine>());
        customLines.Add(ViewMask.LEFT, new List<CustomLine>());
        customLines.Add(ViewMask.RIGHT, new List<CustomLine>());
        customLines.Add(ViewMask.TOP, new List<CustomLine>());

        renderer.material.color = defaultColor;

        drawables = new Dictionary<ViewMask, Drawable>();
        drawables.Add(ViewMask.D3, new Drawable());
        drawables.Add(ViewMask.FRONT, new Drawable());
        drawables.Add(ViewMask.LEFT, new Drawable());
        drawables.Add(ViewMask.RIGHT, new Drawable());
        drawables.Add(ViewMask.TOP, new Drawable());
    }

    public void DestroyDrawables()
    {
        foreach(var item in drawables)
        {
            item.Value.Destroy();
        }
    }

    void OnDestroy()
    {
        DestroyDrawables();
    }

    public void SetSelected(bool selected, bool sendMessage = false, int mouseButton = 0, bool doubleClicked = false)
    {
        isSelected = selected;

        if (isSelected)
        {
            renderer.material.color = selectedColor;

            if (sendMessage)
            {
                var click = Descon.Data.EClickType.Single;

                if (doubleClicked)
                    click = Descon.Data.EClickType.Double;
                else if (mouseButton == 1)
                {
                    click = Descon.Data.EClickType.Right;
                }

                MessageQueueTest.instance.SendUnityData(MessageQueueTest.GetClickString(parentMemberType, subMemberType, click));
            }
        }
        else
        {
            renderer.material.color = defaultColor;
        }
    }

    public void SetHover(GameObject obj)
    {
        if (obj == gameObject)
        {
            if (isSelected)
            {
                renderer.material.color = selectedColor;
            }
            else
                renderer.material.color = defaultColor + new Color(highlightStrength, highlightStrength, highlightStrength, 1);
        }
        else
        {
            if (isSelected)
            {
                renderer.material.color = selectedColor;
            }
            else
                renderer.material.color = defaultColor;
        }
    }

    public void SetSelected(GameObject obj, bool sendMessage = false, int mouseButton = 0, bool doubleClicked = false)
    {
        SetSelected(obj == gameObject, sendMessage, mouseButton, doubleClicked);
    }

    public void CheckSelected(Camera cam, bool doubleClicked = false)
    {
        //Mouse over
        if (Physics.Raycast(cam.ViewportPointToRay(cam.ScreenToViewportPoint(Input.mousePosition)), out hit, 1000.0f))
        {
            if (hit.collider == this.collider)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    isSelected = true;

                    MessageQueueTest.instance.SendUnityData(new Descon.Data.CommonLists().CompleteMemberList[parentMemberType]);
                }

                if (isSelected)
                {
                    renderer.material.color = selectedColor;
                }
                else
                    renderer.material.color = defaultColor + new Color(highlightStrength, highlightStrength, highlightStrength, 1);
            }
            else
            {
                //Hit something else
                if (Input.GetMouseButtonUp(0))
                {
                    isSelected = false;
                }

                if (isSelected)
                {
                    renderer.material.color = selectedColor;
                }
                else
                    renderer.material.color = defaultColor;
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                isSelected = false;
            }

            if (isSelected)
            {
                renderer.material.color = selectedColor;
            }
            else
                renderer.material.color = defaultColor;
        }
    }
}