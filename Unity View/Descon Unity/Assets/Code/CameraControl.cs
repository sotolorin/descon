using Descon.Data;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform anchor;
    public Vector3 rotationSensitivity;
    public Vector3 translationSensitivity;
    public float zoomSpeed = 20;
    private Vector2 cameraRotation;
    private Vector2 startRotation = Vector2.zero;
    public Vector3 startPosition;
    private bool isVirtualMachine;
    public bool disableRotation;
    public bool transformLocal = false;
    public float minZoom = 5.0f;
    public float maxZoom = 100.0f;
    public float zoomFitZoom = 25.0f;
    public bool zoomChanged = false;
    public bool hasMoved = false;   //True when the zoom or pan changes

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
        var translationspd = 5;// (float)CommonDataStatic.Preferences.ViewSettings.MouseTranslationSpeed;
        var rotationspd = 10;// (float)CommonDataStatic.Preferences.ViewSettings.MouseRotationSpeed;
        var zoomspd = 20;// (float)CommonDataStatic.Preferences.ViewSettings.MouseZoomSpeed;
        rotationSensitivity = new Vector3(rotationspd, rotationspd, rotationspd);
        translationSensitivity = new Vector3(translationspd, translationspd, zoomspd);
        zoomSpeed = zoomspd;
    }

    public void ResetPosition()
    {
        if (!transformLocal)
            anchor.transform.position = startPosition;
        else
            transform.localPosition = new Vector3(0, 0, -100);
    }

    public void ResetRotation()
    {
        cameraRotation = startRotation;
    }

    public void SetZoom(float ortho)
    {
        if (camera.isOrthoGraphic)
            nextOrthoValue = Mathf.Clamp(ortho, minZoom, maxZoom);

        ApplyZoom();
    }

    public void ApplyZoom()
    {
        camera.orthographicSize = nextOrthoValue;
    }
	
	public void ControlUpdate()
	{
	    if (!KeyInputs())
	    {
            //3D camera
            if (!disableRotation)
            {
                //Note: Mouse movement is frame-independent, thus you do not have to multiply by Time.deltaime.
                //Rotate camera key press
                if ((!isVirtualMachine && Input.GetMouseButton(0)) || (isVirtualMachine && Input.GetMouseButton(1)))
                {
                    var delta = new Vector2(MouseManager.GetMouseAxisY() * rotationSensitivity.x, MouseManager.GetMouseAxisX() * rotationSensitivity.y);

                    if (delta.magnitude > 0)
                    {
                        hasMoved = true;
                    }

                    cameraRotation.x -= delta.x;
                    cameraRotation.y += delta.y;
                }
                //Pan camera key press
                else if ((!isVirtualMachine && Input.GetMouseButton(1)) || (isVirtualMachine && Input.GetMouseButton(0)))
                {
                    //Written to enable rotations on VMs (left get mouse input to pan)
                    Vector3 translation = new Vector3(MouseManager.GetMouseAxisX() * translationSensitivity.x, MouseManager.GetMouseAxisY() * translationSensitivity.y, 0);

                    if (translation.magnitude > 0)
                    {
                        hasMoved = true;
                    }

                    if (!transformLocal)
                        anchor.Translate(-translation, Space.Self);
                    else
                        transform.Translate(-translation, Space.Self);
                }

                //Camera zoom key press
                if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
                {
                    if (!transformLocal)
                        anchor.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, Space.Self);
                    else
                        transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, Space.Self);

                    zoomChanged = true;
                    hasMoved = true;
                }
            }
            else //2D cameras
            {
                //Translation
                if ((!isVirtualMachine && Input.GetMouseButton(1)) || (isVirtualMachine && Input.GetMouseButton(0)))
                {
                    Vector3 translation = new Vector3(MouseManager.GetMouseAxisX() * translationSensitivity.x, MouseManager.GetMouseAxisY() * translationSensitivity.y, 0);

                    if (translation.magnitude > 0)
                    {
                        hasMoved = true;
                    }

                    anchor.Translate(-translation, Space.Self);
                }

                //Camera zoom key press
                if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
                {
                    nextOrthoValue -= Input.GetAxis("Mouse ScrollWheel") * translationSensitivity.z;

                    nextOrthoValue = Mathf.Clamp(nextOrthoValue, minZoom, maxZoom);

                    zoomChanged = true;
                    hasMoved = true;
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

            var view = GetComponent<ViewportControl>();

            if(view != null)
            {
                hasMoved = false;
                view.ZoomFit(true);
            }
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
            zoomChanged = true;
        }
        else
        {
            if (!transformLocal) anchor.Translate(Vector3.forward * direction * zoomSpeed, Space.Self);
            else transform.Translate(Vector3.forward * direction * zoomSpeed, Space.Self);
        }
    }

}
