using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float _projectileSpeed = 5f;

    [SerializeField]
    private float _lifeTime = 5f;

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

}
