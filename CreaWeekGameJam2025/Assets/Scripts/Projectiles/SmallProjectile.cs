using UnityEngine;

public class SmallProjectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponentInParent<NPCController>();

        if (npc != null)
        {
            npc.IsBleeding = true;
        }
    }
}
