using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerBloodTracker : MonoBehaviour
{
    [SerializeField] public UnityEvent<bool> OnDeath;

    private float _maxBlood = 100.0f;
    private float _currBlood = 100.0f;
    private float _depletionSpeed = 100.0f / 30.0f;

    private bool _isDead = false;

    [SerializeField]
    private bool _countEnemyHit = true;

    public float BloodPercentage
    {
        get
        {
            return _currBlood/_maxBlood;
        }
    }

    private void Start()
    {
        _currBlood = _maxBlood;

        StartCoroutine(BloodDepletion());
    }

    private void OnEnable()
    {
        SmallProjectile.onEnemyHit += EnemyHit;
        DeadNPCState.onEnemyKilled += EnemyKilled;
    }

    private void OnDisable()
    {
        SmallProjectile.onEnemyHit -= EnemyHit;
        DeadNPCState.onEnemyKilled -= EnemyKilled;
    }

    private IEnumerator BloodDepletion()
    {
        while(!_isDead)
        {
            _currBlood -= _depletionSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private void LateUpdate()
    {
        if(!_isDead)
        {
            CheckMeter();
        }
    }

    private void CheckMeter()
    {
        if(_currBlood <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died");
        OnDeath.Invoke(this);
        _isDead = true;

        StartCoroutine(DeathScreen(1.5f));
    }

    private void EnemyHit()
    {
        if(_countEnemyHit)
        {
            FillBar();
        }
    }

    private void EnemyKilled()
    {
        FillBar();
    }

    private void FillBar()
    {
        _currBlood = _maxBlood;
    }

    private IEnumerator DeathScreen(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("EndScene");
    }

}
