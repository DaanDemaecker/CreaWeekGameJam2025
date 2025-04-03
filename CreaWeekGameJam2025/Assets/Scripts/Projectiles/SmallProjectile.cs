using UnityEngine;

public class SmallProjectile : MonoBehaviour
{
    public delegate void EnemyHit();
    public static event EnemyHit onEnemyHit;

    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponentInParent<NPCController>();

        if (npc != null && !npc.IsDead)
        {
            npc.IsBleeding = true;

            npc.HitFX();

            Vector3 direction = npc.transform.position - transform.position;

            direction.y = 0;

            npc.StateMachine.MoveToState(new WanderingState(direction, npc));

            onEnemyHit.Invoke();

            Destroy(gameObject);
        }
    }
}
