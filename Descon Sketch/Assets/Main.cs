using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {

    public static Main instance;
    public GameObject mainCanvas;
    public Camera mainCamera;
    //public ViewportControl view;

    public static GameObject GetMainCanvas()
    {
        return instance.mainCanvas;
    }

    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ClearView()
    {
        //foreach(var item in view)
    }

    public static GameObject CreateTextObject(GameObject panel, string text, Vector3 position)
    {
        //var obj = (GameObject)Instantiate(instance.simpleLabel, position, Quaternion.identity);
        //obj.GetComponent<UnityEngine.UI.Text>().text = text;
        //obj.transform.position = position;
        //obj.transform.parent = panel.transform;
        //obj.transform.localRotation = Quaternion.identity;
        //obj.transform.localScale = Vector3.one;

        //return obj;
        return null;
    }

}
