using System.Collections.Generic;
using UnityEngine;

public class LeaderManager : MonoBehaviour
{
    [SerializeField] List<Transform> PossiblePositions = new List<Transform>();
    [SerializeField] Transform Leader;

    private void Start()
    {
        Leader.position = PossiblePositions[(int)Random.Range(0, PossiblePositions.Count - 1)].position;
        Leader.GetComponent<NPCController>().enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (PossiblePositions.Contains(transform.GetChild(i))){
                Gizmos.DrawSphere(transform.GetChild(i).position, 2);
            }
        }
    }
}
