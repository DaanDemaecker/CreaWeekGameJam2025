using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerTaunt : MonoBehaviour, PlayerInput.ITauntActions
{
    private List<NPCController> _npcsInRange = new List<NPCController>();

    private bool _canTaunt = true;

    [SerializeField]
    private float _tauntCooldown = 5.0f;

    public void OnTaunt(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
       if(context.performed && _canTaunt)
        {
            Taunt();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponentInParent<NPCController>();

        if (npc != null)
        {
            _npcsInRange.Add(npc);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var npc = other.GetComponentInParent<NPCController>();

        if (npc != null)
        {
            _npcsInRange.Remove(npc);
        }
    }

    private void Taunt()
    {
        _npcsInRange.RemoveAll(x => x == null);

        foreach (var npc in _npcsInRange)
        {
            if (!npc.IsBleeding)
            {
                npc.StateMachine.MoveToState(new ChasingState(transform, npc));
            }
        }

        if(_npcsInRange.Count == 0)
        {
            var npc = GetClosestNpc();
            if (npc != null)
            {
                npc.StateMachine.MoveToState(new ChasingState(transform, npc));
            }
        }
    }

    NPCController GetClosestNpc()
    {
        StartCoroutine(TauntCooldown(_tauntCooldown));

        var npcs = FindObjectsByType<NPCController>(FindObjectsSortMode.None);

        if(npcs.Length <0) return null;

        int closestNpc = 0;
        float closestDistance = float.MaxValue;

        for(int i = 0; i < npcs.Length; i++)
        {
            var npc = npcs[i];

            if(npc.IsBleeding)
            {
                continue;
            }    

            var distance = Vector3.Distance(transform.position, npc.transform.position);

            if(distance < closestDistance)
            {
                closestNpc = i;
                closestDistance = distance;
            }
        }

        return npcs[closestNpc];
    }

    private IEnumerator TauntCooldown(float duration)
    {
        _canTaunt = false;
        yield return new WaitForSeconds(duration);
        _canTaunt = true;
    }
}
