using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerMelee : MonoBehaviour, PlayerInput.IMeleeActions
{
    private bool _canAttack = true;

    [SerializeField]
    private float _meleeCooldown = 5.0f;

    [SerializeField]
    private float _meleeRange = 7.5f;

    [SerializeField]
    private VisualEffect _melee;

    [SerializeField]
    private AudioSource _attackSound;

    public void OnMelee(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.performed && _canAttack)
        {
            Attack();
        }
    }

    private void Start()
    {
        _melee.Stop();
    }

    private void Attack()
    {
        var npcsInRange = new List<NPCController>();

        var targets = Physics.OverlapSphere(transform.position, _meleeRange);
        foreach (var target in targets)
        {
            var npc = target.GetComponentInParent<NPCController>();
            if (npc && !npc.IsDead)
            {
                npcsInRange.Add(npc);
            }
        }

        foreach (var npc in npcsInRange)
        {

            npc.StateMachine.MoveToState(new DeadNPCState(transform.position, npc));

        }

        //animation and SFX
        _melee.Play();
        _attackSound.Play();

        StartCoroutine(MeleeCooldown(_meleeCooldown));
    }

    private IEnumerator MeleeCooldown(float duration)
    {
        _canAttack = false;
        yield return new WaitForSeconds(duration);
        _canAttack = true;
    }
}
