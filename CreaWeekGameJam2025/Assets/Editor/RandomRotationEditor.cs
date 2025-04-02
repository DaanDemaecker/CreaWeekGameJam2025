using UnityEditor;
using UnityEngine;

public class RandomRotationEditor : MonoBehaviour
{
    [MenuItem("Tools/Randomize Rotation %r")] // Shortcut: Ctrl+R (Windows) / Cmd+R (Mac)
    static void RandomizeRotation()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj.transform, "Random Rotation");
            obj.transform.rotation = Quaternion.Euler(
                Random.Range(0f, 0f),
                Random.Range(0f, 360f),
                Random.Range(0f, 0f)
            );
        }
    }
}
