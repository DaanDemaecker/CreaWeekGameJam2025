using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class PlayerTaunt : MonoBehaviour, PlayerInput.ITauntActions
{
    private bool _canTaunt = true;

    [SerializeField]
    private float _tauntCooldown = 5.0f;

    [SerializeField]
    private float _tauntRange = 15.0f;

    [SerializeField]
    private VisualEffect _taunt;

    [SerializeField]
    private AudioSource _tauntSound;


    public void Start()
    {
        _taunt.Stop();
    }

    public void OnTaunt(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
       if(context.performed && _canTaunt)
        {
            Taunt();
        }
    }

    private void Taunt()
    {
        var npcsInRange = new List<NPCController>();

        var targets = Physics.OverlapSphere(transform.position, _tauntRange);
        foreach (var target in targets)
        {
            var npc = target.GetComponentInParent<NPCController>();
            if(npc && !(npc.IsBleeding || npc.IsDead))
            {
                npcsInRange.Add(npc);
            }
        }

        foreach (var npc in npcsInRange)
        {
            if (!npc.IsBleeding)
            {
                npc.StateMachine.MoveToState(new ChasingState(Vector3.zero,transform, npc));
            }
        }

        if(npcsInRange.Count == 0)
        {
            var npc = GetClosestNpc();
            if (npc != null)
            {
                npc.StateMachine.MoveToState(new ChasingState(Vector3.zero, transform, npc));
            }
        }

        _taunt.Play();

        _tauntSound.Play();

        StartCoroutine(TauntCooldown(_tauntCooldown));

    }

    NPCController GetClosestNpc()
    {

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
    public UnityEvent<float> UpdateTauntUI;
    private IEnumerator TauntCooldown(float duration)
    {
        _canTaunt = false;
        UpdateTauntUI.Invoke(0);
        float startTime = Time.time;
        while(startTime + duration >= Time.time)
        {
            UpdateTauntUI.Invoke((Time.time - startTime) / duration);
            yield return null;
        }
        UpdateTauntUI.Invoke(1);
        _canTaunt = true;
    }
}
