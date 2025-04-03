using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour, PlayerInput.IShootActions
{
    [SerializeField]
    private GameObject _bigProjectile = null;

    [SerializeField]
    private GameObject _smallProjectile = null;

    [SerializeField]
    private float _shootCooldown = 0.5f;

    private bool _canShoot = true;

    [SerializeField]
    private float _maxAngle = 45.0f;

    [SerializeField]
    private float _maxDistance = 10.0f;

    [SerializeField]
    private NPCController _currentTarget = null;

    [SerializeField]
    private AudioSource _attackSound;

    public void OnShootSmall(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_canShoot && context.started)
        {
            Shoot(_smallProjectile);
        }
    }

    private void Shoot(GameObject prefab)
    {
       _attackSound.Play();
        
        var gameObject = Instantiate(prefab, transform.position, Quaternion.identity);

        if (gameObject != null)
        {
            gameObject.transform.forward = transform.forward;
            if (_currentTarget != null)
            {
                gameObject.GetComponent<Projectile>().Target = _currentTarget.transform;
            }
            else
            {
                gameObject.GetComponent<Projectile>().SetVelocity();
            }
        }

        StartCoroutine(DisableShootRoutine(_shootCooldown));
    }

    private IEnumerator DisableShootRoutine(float duration)
    {
        _canShoot = false;
        yield return new WaitForSeconds(duration);
        _canShoot = true;
    }

    private void Update()
    {
       _currentTarget = FindTarget(_maxAngle, _maxDistance);

    }

    private void OnDrawGizmos()
    {
        if (_currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_currentTarget.transform.position + Vector3.up * 2, 0.2f);
        }
    }

    private NPCController FindTarget(float maxAngle, float maxDistance)
    {
        var possibleTargets = new List<NPCController>();

        var targets = Physics.OverlapSphere(transform.position, _maxDistance);
        foreach (var target in targets)
        {
            var npc = target.GetComponentInParent<NPCController>();
            if (npc)
            {
                if (!npc.IsDead)
                {
                    possibleTargets.Add(npc);
                }
            }
        }

        NPCController targetGO = null;

        float minDotProduct = Mathf.Cos(maxAngle * Mathf.Deg2Rad);
        float targetDistanceSqrd = maxDistance * maxDistance;

        foreach (NPCController go in possibleTargets)
        {
            Vector3 relativePosition = go.transform.position - transform.position;

            float distanceSqrd = relativePosition.sqrMagnitude;
            if ((distanceSqrd < targetDistanceSqrd) &&
                 (Vector3.Dot(relativePosition.normalized, transform.forward) > minDotProduct))
            {
                targetDistanceSqrd = distanceSqrd;
                targetGO = go;
            }
        }
        return targetGO;
    }
}