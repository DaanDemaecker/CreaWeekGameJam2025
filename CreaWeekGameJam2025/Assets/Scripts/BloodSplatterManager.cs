using System.Collections.Generic;
using UnityEngine;

public class BloodSplatterManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _splatterPrefabs = new List<GameObject>();

    int layerMask;

    private void Start()
    {
       layerMask = LayerMask.GetMask("Ground");
    }

    private void OnEnable()
    {
        NPCController.onSmallBloodDropped += SpawnBloodSplatter;
    }

    private void OnDisable()
    {
        NPCController.onSmallBloodDropped -= SpawnBloodSplatter;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit result;

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out result, 100, layerMask))
            {
                SpawnBloodSplatter(result.point, 2);
            }

        }
    }

    private void SpawnBloodSplatter(Vector3 pos, float size)
    {
        if (_splatterPrefabs.Count > 0)
        {
            var gameObject = Instantiate(_splatterPrefabs[Random.Range(0, _splatterPrefabs.Count)], pos, Quaternion.Euler(0, Random.Range(0, 360), 0));
            if (gameObject != null)
            {
                gameObject.transform.localScale = Vector3.one * size;
            }
        }
    }
}
