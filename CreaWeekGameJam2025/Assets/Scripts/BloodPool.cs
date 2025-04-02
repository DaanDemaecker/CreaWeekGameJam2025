using System.Collections;
using UnityEngine;

public class BloodPool : MonoBehaviour
{
    [SerializeField]
    private bool _ignoreGrow = false;
    
    private float _growSpeed = 1.5f;

    private Vector3 _baseSize;
    
    void Start()
    {
        _baseSize = this.transform.localScale;
        
        if (!_ignoreGrow)
        {
            this.transform.localScale = Vector3.zero;

            StartCoroutine(ScaleToSize());
        }
        
    }

    private IEnumerator ScaleToSize()
    {
        while (transform.localScale.magnitude < _baseSize.magnitude)
        {
            this.transform.localScale += Vector3.one * _growSpeed * Time.deltaTime;
            yield return null;
        }

        
    }

}
