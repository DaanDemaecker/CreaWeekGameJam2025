using System.Collections;
using UnityEngine;

public class BloodPool : MonoBehaviour
{
    [SerializeField]
    private bool _ignoreGrow = false;
    
    private float _growSpeed = 1f;
    
    void OnEnable()
    {
        
        if (!_ignoreGrow)
        {
            StartCoroutine(ScaleToSize());
        }
        
    }

    private IEnumerator ScaleToSize()
    {
        while (transform.GetChild(0).localScale.x < 1)
        {
            this.transform.GetChild(0).localScale += Vector3.one * _growSpeed * Time.deltaTime;
            yield return null;
        }

        
    }

}
