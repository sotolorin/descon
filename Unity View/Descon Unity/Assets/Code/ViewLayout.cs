using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Each member will store a view layout object per view
//Each view layout will contain label objects, dimensions, callouts, and line objects
[System.Serializable]
public class ViewLayout : System.Object
{
    public List<LabelTextObject> labelObjects; //Stores a list of all the label objects which have a text component
}
