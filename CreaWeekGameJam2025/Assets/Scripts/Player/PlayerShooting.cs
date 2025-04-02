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

    private int _shootInhibitor = 0;

    [SerializeField]
    private float _maxAngle = 45.0f;

    [SerializeField]
    private float _maxDistance = 10.0f;

    public int ShootInhibitor
    {
        set { _shootInhibitor = value; }
        get { return _shootInhibitor; }
    }

    [SerializeField]
    private NPC _currentTarget = null;

    private List<NPC> _possibleTargets = new List<NPC>();

    public void OnShootSmall(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_shootInhibitor == 0 && context.started)
        {
            Shoot(_smallProjectile);
        }
    }

    private void Shoot(GameObject prefab)
    {
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
        _shootInhibitor++;

        yield return new WaitForSeconds(duration);

        _shootInhibitor--;
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

    private NPC FindTarget(float maxAngle, float maxDistance)
    {
        _possibleTargets.RemoveAll(x => x == null);

        NPC  targetGO = null;

        float minDotProduct = Mathf.Cos(maxAngle * Mathf.Deg2Rad);
        float targetDistanceSqrd = maxDistance * maxDistance;

        foreach (NPC go in _possibleTargets)
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

    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponentInParent<NPC>();

        if (npc != null)
        {
            _possibleTargets.Add(npc);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var npc = other.GetComponentInParent<NPC>();

        if (npc != null)
        {
            _possibleTargets.Remove(npc);
        }
    }
}