using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    [SerializeField]
    private float _speed = 1f;

    void Update()
    {
        this.transform.Rotate(new Vector3(0, Time.deltaTime * _speed, 0));
    }
}
