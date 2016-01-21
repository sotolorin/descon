using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Draw a dimension callout that fixes the overlap issue
public class DimensionTest : MonoBehaviour
{
    public Vector3 worldPointA;
    public Vector3 worldPointB;
    public GameObject anchor;
    public Camera viewCamera;
    public float zOffset = 0.1f;
    public float dimensionLength = 10.0f;
    public RectTransform textTransform;
    private GameObject lineObject;
    public Text text;
    public Descon.Data.EOffsetType offsetBehaviour;

    private Vector3 dimensionDirection;
    private Vector3 dimensionDirectionCross;

    public float lineSize = 0.00157f;
    public bool isVerticalDimension = false;

    void Start()
    {
        dimensionDirection = Vector3.Cross(worldPointA - worldPointB, viewCamera.transform.forward).normalized;
        dimensionDirectionCross = (worldPointB - worldPointA).normalized;

        lineObject = MeshCreator.CreateLineObject("Dimension Test Lines");
    }

    void Update()
    {
        //Draw the lines
        MeshCreator.Reset();

        var textLength = (isVerticalDimension ? textTransform.rect.height : textTransform.rect.width);
        var otherTextLength = (!isVerticalDimension ? textTransform.rect.height : textTransform.rect.width);

        //Move the anchor
        anchor.transform.position = (worldPointA + worldPointB) / 2 + dimensionDirection * dimensionLength;

        var pointB = worldPointA + dimensionDirection * dimensionLength;
        var pointC = worldPointB + dimensionDirection * dimensionLength;

        var guiDirection = (viewCamera.WorldToScreenPoint(pointB) - viewCamera.WorldToScreenPoint(pointC)).normalized;

        //Move the text
        var pos = RectTransformUtility.WorldToScreenPoint(viewCamera, anchor.transform.position) - new Vector2(viewCamera.pixelRect.x, viewCamera.pixelRect.y);

        //Calculate the distance between the two points, if this distance is too small, move the text outward
        var offset = Vector3.zero;

        var dist = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(viewCamera, worldPointA), RectTransformUtility.WorldToScreenPoint(viewCamera, worldPointB));
        if (dist < textLength + 20)
        {
            switch(offsetBehaviour)
            {
                case Descon.Data.EOffsetType.Top:
                    offset = dimensionDirection * otherTextLength * 0.5f + dimensionDirection * 5;
                    break;

                case Descon.Data.EOffsetType.Right:
                    offset = dimensionDirectionCross * textLength / 2 + dimensionDirectionCross * dist / 2 + dimensionDirectionCross * 5;
                    break;

                case Descon.Data.EOffsetType.Bottom:
                    offset = -(dimensionDirection * otherTextLength * 0.5f + dimensionDirection * 5);
                    break;

                case Descon.Data.EOffsetType.Left:
                    offset = -(dimensionDirectionCross * textLength / 2 + dimensionDirectionCross * dist / 2 + dimensionDirectionCross * 5);
                    break;
            }
            
            //Distance is too small, draw arrows on outside
            MeshCreator.DrawArrow(viewCamera, viewCamera.WorldToScreenPoint(pointB), viewCamera.WorldToScreenPoint(pointB) - guiDirection * 15, lineSize);
            MeshCreator.DrawArrow(viewCamera, viewCamera.WorldToScreenPoint(pointC), viewCamera.WorldToScreenPoint(pointC) + guiDirection * 15, lineSize);
        }
        else
        {
            var arrowLength = (dist - textLength - 20) / 2;

            //Draw the arrows towards the center
            MeshCreator.DrawArrow(viewCamera, viewCamera.WorldToScreenPoint(pointB), viewCamera.WorldToScreenPoint(pointB) - guiDirection * arrowLength, lineSize);
            MeshCreator.DrawArrow(viewCamera, viewCamera.WorldToScreenPoint(pointC), viewCamera.WorldToScreenPoint(pointC) + guiDirection * arrowLength, lineSize);
        }

        //Move the text over based on the offset behaviour
        textTransform.anchoredPosition = new Vector2(Mathf.Round(pos.x + offset.x), Mathf.Round(pos.y + offset.y));

        //Draw the lines that extend up towards the text
        MeshCreator.DrawEdgeWorldSpace(viewCamera, worldPointA, pointB, lineSize, zOffset);
        MeshCreator.DrawEdgeWorldSpace(viewCamera, worldPointB, pointC, lineSize, zOffset);

        MeshCreator.Create(lineObject.GetComponent<MeshFilter>().sharedMesh);
    }
}
