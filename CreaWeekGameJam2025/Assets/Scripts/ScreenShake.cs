using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField]
    private bool _start = false;

    [SerializeField]
    private float _duration = 1f;

    [SerializeField]
    private AnimationCurve _strength;

    private void Update()
    {
        if (_start)
        {
            _start = false;
            StartCoroutine(Shaking());
        }
    }

    public void StartShake(float duration)
    {
        _start = false;
        _duration = duration;
        StartCoroutine(Shaking());
    }

    IEnumerator Shaking()
    {
        Vector3 startPosition = transform.position;
        float elapsedTIme = 0f;

        while (elapsedTIme < _duration)
        {
            elapsedTIme += Time.deltaTime;
            
            float strength = _strength.Evaluate(elapsedTIme / _duration);

            transform.position = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = startPosition;
    }
}
