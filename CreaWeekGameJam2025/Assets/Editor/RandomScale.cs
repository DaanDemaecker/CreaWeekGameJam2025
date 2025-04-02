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

            float randomScale = Random.Range(0.15f, 0.27f);
            obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }
    }
}
