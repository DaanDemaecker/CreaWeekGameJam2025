using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

            npc.Hit();

        }

        //animation and SFX
        _melee.Play();
        _attackSound.Play();

        StartCoroutine(MeleeCooldown(_meleeCooldown));
    }
    public UnityEvent<float> UpdateUI;
    private IEnumerator MeleeCooldown(float duration)
    {
        _canAttack = false;
        UpdateUI.Invoke(0);
        float startTime = Time.time;
        while (startTime + duration >= Time.time)
        {
            UpdateUI.Invoke((Time.time - startTime) / duration);
            yield return null;
        }
        UpdateUI.Invoke(1);
        _canAttack = true;
    }
}
