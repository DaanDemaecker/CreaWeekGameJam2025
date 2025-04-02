using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float _projectileSpeed = 5f;

    [SerializeField]
    private float _lifeTime = 5f;

    private Transform _target;
    public Transform Target
    {
        set
        {
            _target = value;
        }
    }

    private void Start()
    {
        StartCoroutine(KillRoutine(_lifeTime));
    }

    public void SetVelocity()
    {
        var rb = GetComponent<Rigidbody>();
        if(rb != null )
        {
            rb.linearVelocity = transform.forward * _projectileSpeed;
        }
    }

    private IEnumerator KillRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (_target != null)
        {
            transform.forward = _target.position + Vector3.up - transform.position;
            SetVelocity();
        }
    }

}
