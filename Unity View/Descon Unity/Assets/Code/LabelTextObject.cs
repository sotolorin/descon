using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// This script is attached to a gameObject that has a Text component. It also has a world transform to anchor itself to. It also have a Vector3 with which it points to.
/// </summary>
public class LabelTextObject : MonoBehaviour
{
    public string ShapeName
    {
        set
        {
            text.text = value;
        }

        get
        {
            return text.text;
        }
    }

    public Color TextColor
    {
        set
        {
            text.color = value;

            if(dragComponent != null)
            {
                if(dragComponent.additionalTextObj != null)
                {
                    dragComponent.additionalTextObj.GetComponent<Text>().color = value;
                }
            }
        }

        get
        {
            return text.color;
        }
    }

    public int TextSize
    {
        set
        {
            text.fontSize = value;

            if (dragComponent != null)
            {
                if (dragComponent.additionalTextObj != null)
                {
                    dragComponent.additionalTextObj.GetComponent<Text>().fontSize = value;
                }
            }
        }

        get
        {
            return text.fontSize;
        }
    }

    public FontStyle TextStyle
    {
        set
        {
            text.fontStyle = value;

            if (dragComponent != null)
            {
                if (dragComponent.additionalTextObj != null)
                {
                    dragComponent.additionalTextObj.GetComponent<Text>().fontStyle = value;
                }
            }
        }

        get
        {
            return text.fontStyle;
        }
    }

    public bool rightSide = false;
    public Draggable dragComponent;
    public Text text;

    private bool isVisible;
    public bool Visible
    {
        set
        {
            isVisible = value;

            if (dragComponent.arrowObject != null)
                dragComponent.arrowObject.SetActive(isVisible);

            if (dragComponent.additionalTextObj != null)
                dragComponent.additionalTextObj.SetActive(isVisible);

            text.enabled = isVisible;
        }

        get
        {
            return isVisible;
        }
    }

    void Awake()
    {
        text = GetComponent<Text>();
    }
}
