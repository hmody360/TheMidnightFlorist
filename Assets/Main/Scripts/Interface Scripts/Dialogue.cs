using UnityEngine;

[System.Serializable] //This Makes it so that when we add this to an object, we can Modify its contents within the gameObject we added it to, in the inspector.
public class Dialogue
{
    public string name;

    [TextArea(3,10)]
    public string[] sentences;
}
