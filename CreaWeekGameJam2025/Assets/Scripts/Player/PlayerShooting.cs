using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour, PlayerInput.IShootActions
{
    [SerializeField]
    private GameObject _bigProjectile = null;

    [SerializeField]
    private GameObject _smallProjectile = null;

    [SerializeField]
    private float _shootCooldown = 0.5f;

    bool _canShoot = true;

    public bool CanShoot
    {
        set { _canShoot = value; }
        get { return _canShoot; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnShootBig(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_canShoot && context.started)
        {
            Shoot(_bigProjectile);
        }
    }

    public void OnShootSmall(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_canShoot && context.started)
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
            gameObject.GetComponent<Projectile>().SetVelocity();
        }

        StartCoroutine(DisableShootRoutine(_shootCooldown));
    }

    private IEnumerator DisableShootRoutine(float duration)
    {
        _canShoot = false;

        yield return new WaitForSeconds(duration);

        _canShoot = true;
    }
}
