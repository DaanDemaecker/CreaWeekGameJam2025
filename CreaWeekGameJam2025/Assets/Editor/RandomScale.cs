using UnityEditor;
using UnityEngine;

public class RandomScaleEditor : MonoBehaviour
{
    [MenuItem("Tools/Randomize Scale %s")] // Shortcut: Ctrl+S (Windows) / Cmd+S (Mac)
    static void RandomizeScale()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj.transform, "Random Scale");

            float randomScale = Random.Range(.3f, .6f);
            obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }
    }
}
