using UnityEngine;

public class SmallProjectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponentInParent<NPCController>();

        if (npc != null && !npc.IsDead)
        {

            Vector3 direction = npc.transform.position - transform.position;

            direction.y = 0;

            npc.StateMachine.MoveToState(new WanderingState(direction, npc));

            npc.Hit();

            Destroy(gameObject);
        }
    }
}
