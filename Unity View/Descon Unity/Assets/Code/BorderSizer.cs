using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BorderSizer : MonoBehaviour
{
    public Image left;
    public Image right;
    public Image top;
    public Image bottom;
    public Text text;

    [SerializeField]
    private float borderWidth = 1.0f;
    [SerializeField]
    private Color borderColor = Color.white;

    void Start()
    {
        UpateBorders();
        UpdateColors();
    }

    public float BorderWidth
    {
        get
        {
            return borderWidth;
        }

        set
        {
            borderWidth = value;
            UpateBorders();
        }
    }

    public Color BorderColor
    {

        get
        {
            return borderColor;
        }

        set
        {
            borderColor = value;
            UpdateColors();
        }
    }

    private void UpateBorders()
    {
        left.rectTransform.sizeDelta = new Vector2(borderWidth, 0);
        right.rectTransform.sizeDelta = new Vector2(borderWidth, 0);
        top.rectTransform.sizeDelta = new Vector2(0, borderWidth);
        bottom.rectTransform.sizeDelta = new Vector2(0, borderWidth);
    }

    private void UpdateColors()
    {
        left.color = borderColor;
        right.color = borderColor;
        top.color = borderColor;
        bottom.color = borderColor;
        text.color = borderColor;
    }
}
