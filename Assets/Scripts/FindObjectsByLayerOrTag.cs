using UnityEngine;
using UnityEditor;

public class FindObjectsByLayerOrTag
{
    [MenuItem("Tools/List Objects in Layer")]
    static void ListLayer()
    {
        string layerName = "Building";
        int layer = LayerMask.NameToLayer(layerName);

        foreach(GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            if (go.layer == layer) Debug.Log(go.name, go);
    }

    [MenuItem("Tools/List Objects with Tag")]
    static void ListTag()
    {
        string tag = "Building";

        foreach(GameObject go in GameObject.FindGameObjectsWithTag(tag))
            Debug.Log(go.name, go);
    }
}
