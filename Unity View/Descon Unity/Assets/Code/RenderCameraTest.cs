using UnityEngine;
using System.Collections;
using Descon.Data;

public class RenderCameraTest : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Canvas canvas;

	// Use this for initialization
	void Start ()
    {
        //var screenShotSize = 2000;
        //renderTexture = new RenderTexture(screenShotSize, screenShotSize)
	}
	
	// Update is called once per frame
    //void Update ()
    //{
    //    if(Input.GetKeyDown(KeyCode.F))
    //    {
    //        StartCoroutine("TakeScreenshot");
    //    }
    //}

    //public IEnumerator TakeScreenshot()
    //{
    //    canvas.renderMode = RenderMode.WorldSpace;

    //    yield return new WaitForEndOfFrame();

    //    var screenShotSize = 2000;
    //    renderTexture = RenderTexture.GetTemporary(screenShotSize, screenShotSize, 24, RenderTextureFormat.ARGB32);
    //    Texture2D screenShot = new Texture2D(screenShotSize, screenShotSize, TextureFormat.RGB24, false);
    //    RenderTexture.active = renderTexture;

    //    camera.targetTexture = renderTexture;

    //    //Render the camera
    //    camera.Render();

    //    camera.targetTexture = null;

    //    screenShot.ReadPixels(new Rect(0, 0, screenShotSize, screenShotSize), 0, 0);
    //    screenShot.Apply();

    //    var bytes = screenShot.EncodeToPNG();
    //    Destroy(screenShot);
    //    System.IO.File.WriteAllBytes(ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_DRAWING_IMAGE, bytes);
    //    RenderTexture.ReleaseTemporary(renderTexture);

    //    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    //}
}
