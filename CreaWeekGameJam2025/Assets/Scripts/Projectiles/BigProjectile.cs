using UnityEngine;

public class BigProjectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponentInParent<NPCController>();

        if (npc != null)
        {
            npc.StateMachine.MoveToState(new DyingState(npc));
        }
    }
}
