using UnityEngine;
using System.Collections;

public class PanelResizer : MonoBehaviour
{
    public RectTransform rectTransform;
    public Camera targetCamera;

    void Update()
    {
        UpdateBounds();
    }

    public void UpdateBounds()
    {
        if (rectTransform != null && targetCamera != null)
        {
            var rect = targetCamera.pixelRect;
            rectTransform.anchoredPosition = new Vector3(rect.x, rect.y);
            rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
        }
    }
}
