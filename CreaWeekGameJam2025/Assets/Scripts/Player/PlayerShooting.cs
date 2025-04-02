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

    int _shootInhibitor = 0;

    public int ShootInhibitor
    {
        set { _shootInhibitor = value; }
        get { return _shootInhibitor; }
    }

    public void OnShootBig(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //if (_shootInhibitor == 0 && context.started)
        //{
        //    Shoot(_bigProjectile);
        //}
    }

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
            gameObject.GetComponent<Projectile>().SetVelocity();
        }

        StartCoroutine(DisableShootRoutine(_shootCooldown));
    }

    private IEnumerator DisableShootRoutine(float duration)
    {
        _shootInhibitor++;

        yield return new WaitForSeconds(duration);

        _shootInhibitor--;
    }
}
