using UnityEngine;
using System.Collections;

public class MouseManager : MonoBehaviour
{
    public static MouseManager instance;
    private Vector3 lastMousePosition;
    private bool isVirtual;

    void Awake()
    {
        instance = this;
        isVirtual = true;
    }

    void LateUpdate()
    {
        //Update mouse position
        lastMousePosition = Input.mousePosition;

        //Test whether on the VM or not
        //If GetAxis returns a value, that means the machine is not on the VM
        if (isVirtual)
        {
            if (Input.GetAxis("Mouse X") != 0)
            {
                isVirtual = false;
            }
        }
    }

    public static float GetMouseAxisX()
    {
        if (instance != null)
        {
            if (instance.isVirtual)
            {
                var dist = (Input.mousePosition.x - instance.lastMousePosition.x) * 0.1f;

                if (Mathf.Abs(dist) > 800)
                {
                    return 0.0f;
                }
                else
                    return dist;
            }
            else
            {
                return Input.GetAxis("Mouse X");
            }
        }
        else
            return 0.0f;
    }

    public static float GetMouseAxisY()
    {
        if (instance != null)
        {
            if (instance.isVirtual)
            {
                var dist = (Input.mousePosition.y - instance.lastMousePosition.y) * 0.1f;

                if (Mathf.Abs(dist) > 800)
                {
                    return 0.0f;
                }
                else
                    return dist;
            }
            else
            {
                return Input.GetAxis("Mouse Y");
            }
        }
        else
            return 0.0f;
    }
}
