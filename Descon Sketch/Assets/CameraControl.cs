using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform anchor;
    public Vector3 rotationSensitivity;
    public Vector3 translationSensitivity;
    public Vector3 zoomSensitivity;
    private Vector2 cameraRotation;
    private Vector2 startRotation = Vector2.zero;
    private Vector3 startPosition;
    private bool isVirtualMachine;
    public bool disableRotation;
    public bool transformLocal = false;
    public float minZoom = 5.0f;
    public float maxZoom = 100.0f;
    public float zoomFitZoom = 25.0f;
    public bool changed = false;

    public float nextOrthoValue = 0;

	// Use this for initialization
	void Start ()
    {
        //Save the starting position and rotation
        startPosition = anchor.transform.position;

        startRotation.x = anchor.transform.rotation.eulerAngles.x;
        startRotation.y = anchor.transform.rotation.eulerAngles.y;

        cameraRotation = startRotation;

        nextOrthoValue = camera.orthographicSize;

	    isVirtualMachine = CheckForVm();

        if (Application.isEditor)
            isVirtualMachine = false;

        SetMouseSensitivity();
    }

    void SetMouseSensitivity()
    {
        var translationspd = 1.0f;
        var rotationspd = 1.0f;
        var zoomspd = 1.0f;
        rotationSensitivity = new Vector3(rotationspd, rotationspd, rotationspd);
        translationSensitivity = new Vector3(translationspd, translationspd, translationspd);
        zoomSensitivity = new Vector3(zoomspd, zoomspd, zoomspd);
    }

    public void Reset()
    {
        if(camera.isOrthoGraphic)
            nextOrthoValue = zoomFitZoom;

        if (!transformLocal)
            anchor.transform.position = startPosition;
        else
            transform.localPosition = new Vector3(0, 0, -100);
    }

    public void ApplyZoom()
    {
        camera.orthographicSize = nextOrthoValue;
    }
	
	public void ControlUpdate()
	{
	    if (!KeyInputs())
	    {
            if (!disableRotation)
            {
                //Note: Mouse movement is frame-independent, thus you do not have to multiply by Time.deltaime.
                //Rotate camera key press
                if ((!isVirtualMachine && Input.GetMouseButton(0)) || (isVirtualMachine && Input.GetMouseButton(1)))
                {
                    //Written to enable rotations on VMs (right get mouse input to rotate)
                    if (isVirtualMachine)
                    {
                        cameraRotation.x -= MouseManager.GetMouseAxisY() * rotationSensitivity.x;
                        cameraRotation.y += MouseManager.GetMouseAxisX() * rotationSensitivity.y;
                    }
                    else
                    {
                        cameraRotation.x -= MouseManager.GetMouseAxisY() * rotationSensitivity.x;
                        cameraRotation.y += MouseManager.GetMouseAxisX() * rotationSensitivity.y;
                    }
                }
                //Pan camera key press
                else if ((!isVirtualMachine && Input.GetMouseButton(1)) || (isVirtualMachine && Input.GetMouseButton(0)))
                {
                    //Written to enable rotations on VMs (left get mouse input to pan)
                    Vector3 translation;
                    if (isVirtualMachine)
                    {
                        translation = new Vector3(MouseManager.GetMouseAxisX() * translationSensitivity.x, MouseManager.GetMouseAxisY() * translationSensitivity.y, 0);
                    }
                    else
                        translation = new Vector3(MouseManager.GetMouseAxisX() * translationSensitivity.x, MouseManager.GetMouseAxisY() * translationSensitivity.y, 0);

                    if (!transformLocal)
                        anchor.Translate(-translation, Space.Self);
                    else
                        transform.Translate(-translation, Space.Self);
                }

                //Camera zoom key press
                if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
                {
                    if (!transformLocal)
                        anchor.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity.z, Space.Self);
                    else
                        transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity.z, Space.Self);

                    changed = true;
                }
            }
            else
            {
                //Translation
                if ((!isVirtualMachine && Input.GetMouseButton(1)) || (isVirtualMachine && Input.GetMouseButton(0)))
                {
                    Vector3 translation;
                    if (!isVirtualMachine)
                        translation = new Vector3(MouseManager.GetMouseAxisX() * translationSensitivity.x,
                            MouseManager.GetMouseAxisY() * translationSensitivity.y, 0);
                    else
                    {
                        translation = new Vector3(MouseManager.GetMouseAxisX() * translationSensitivity.x,
                            MouseManager.GetMouseAxisY() * translationSensitivity.y, 0);
                    }
                    anchor.Translate(-translation, Space.Self);
                }

                //Camera zoom key press
                if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
                {
                    nextOrthoValue -= Input.GetAxis("Mouse ScrollWheel") * translationSensitivity.z;

                    nextOrthoValue = Mathf.Clamp(nextOrthoValue, minZoom, maxZoom);

                    changed = true;
                }
            }
	    }
        
	    //Reset view
        if (Input.GetKeyDown(KeyCode.F))
        {
            cameraRotation = startRotation;

            if(!transformLocal)
                anchor.transform.position = startPosition;
            else
                transform.localPosition = new Vector3(0, 0, -100);
        }

        if(transformLocal)
        {
            if(transform.localPosition.z > -10.0f)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -10.0f);
            }
        }


        cameraRotation.x = Mathf.Clamp(cameraRotation.x, -90, 90);

        //Set the camera to the rotations
        anchor.transform.rotation = Quaternion.Euler(cameraRotation);
	}

    bool CheckForVm()
    {
        return (Input.GetAxis("Mouse Y").Equals(0) && !Input.mousePosition.y.Equals(0) && Input.GetAxis("Mouse X").Equals(0) && !Input.mousePosition.x.Equals(0));
    }

    bool KeyInputs()
    {
        return
            KeyIsUp()
            || KeyIsDown()
            || KeyIsLeft()
            || KeyIsRight()
            || KeyIsPageUp()
            || KeyIsPageDown();
    }

    bool KeyIsUp()
    {
        if (!Input.GetKeyDown(KeyCode.UpArrow)) return false;
        if (!disableRotation)
        {
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)))
            {
                KeyRotation(Vector3.up);
                return true;
            }
        }
        var translation = new Vector3(0, 1 * translationSensitivity.y, 0);
        KeyTranslation(translation);
        return true;
    }

    bool KeyIsDown()
    {
        if (!Input.GetKeyDown(KeyCode.DownArrow)) return false;
        if (!disableRotation)
        {
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)))
            {
                KeyRotation(-Vector3.up);
                return true;
            }
        }
        var translation = new Vector3(0, -1 * translationSensitivity.y, 0);
        KeyTranslation(translation);
        return true;
    }
    bool KeyIsLeft()
    {
        if (!Input.GetKeyDown(KeyCode.LeftArrow)) return false;
        if (!disableRotation)
        {
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)))
            {
                KeyRotation(-Vector3.right);
                return true;
            }
        }
        var translation = new Vector3(-1 * translationSensitivity.x, 0, 0);
        KeyTranslation(translation);
        return true;
    }
    bool KeyIsRight()
    {
        if (!Input.GetKeyDown(KeyCode.RightArrow)) return false;
        if (!disableRotation)
        {
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)))
            {
                KeyRotation(Vector3.right);
                return true;
            }
        }
        var translation = new Vector3(1 * translationSensitivity.x, 0, 0);
        KeyTranslation(translation);
        return true;
    }


    bool KeyIsPageUp()
    {
        if (!Input.GetKeyDown(KeyCode.PageUp)) return false;
        KeyZoom(0.5f);
        return true;
    }
    bool KeyIsPageDown()
    {
        if (!Input.GetKeyDown(KeyCode.PageDown)) return false;
        KeyZoom(-0.5f);
        return true;
    }

    void KeyTranslation(Vector3 translation)
    {
        if (!transformLocal) anchor.Translate(-translation, Space.Self);
        else transform.Translate(-translation, Space.Self);
    }

    void KeyRotation(Vector3 direction)
    {
        cameraRotation.x -= direction.y * rotationSensitivity.x;
        cameraRotation.y += direction.x * rotationSensitivity.y;
    }

    void KeyZoom(float direction)
    {
        if (disableRotation)
        {
            nextOrthoValue -= direction*translationSensitivity.z;
            nextOrthoValue = Mathf.Clamp(nextOrthoValue, minZoom, maxZoom);
            changed = true;
        }
        else
        {
            if (!transformLocal) anchor.Translate(Vector3.forward * direction * zoomSensitivity.z, Space.Self);
            else transform.Translate(Vector3.forward * direction * zoomSensitivity.z, Space.Self);
        }
    }

}
