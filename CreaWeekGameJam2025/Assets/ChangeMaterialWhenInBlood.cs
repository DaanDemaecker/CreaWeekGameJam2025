using UnityEngine;

public class ChangeMaterialWhenInBlood : MonoBehaviour
{
    [SerializeField] Material material;

    Renderer r;

    private void Start()
    {
        r = GetComponent<Renderer>();
    }
    private void FixedUpdate()
    {
        if(r.sharedMaterial != material && Physics.CheckSphere(transform.position,.1f,1 << 7))
        {
            r.sharedMaterial = material;
        }
    }
}
