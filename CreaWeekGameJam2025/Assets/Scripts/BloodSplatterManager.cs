using System.Collections.Generic;
using UnityEngine;

public class BloodSplatterManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _splatterPrefabs = new List<GameObject>();


    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit result;

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out result))
            {
                SpawnBloodSplatter(result.point);
            }

        }
    }

    private void SpawnBloodSplatter(Vector3 pos)
    {
        if (_splatterPrefabs.Count > 0)
        {
            Instantiate(_splatterPrefabs[Random.Range(0, _splatterPrefabs.Count)], pos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
    }
}
