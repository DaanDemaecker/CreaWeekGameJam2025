using UnityEngine;

public class PlayerCamera : MonoBehaviour, PlayerInput.IRotateCameraActions
{
    private PlayerMovement _player = null;

    private Vector3 _cameraOffset = Vector3.zero;

    private float _yPos = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = FindFirstObjectByType<PlayerMovement>();

        _yPos = transform.position.y;

        if (_player != null)
        {
            _cameraOffset = transform.position - _player.transform.position;
        }
    }


    void LateUpdate()
    {
        if (_player != null)
        {
            transform.position = _player.transform.position + _cameraOffset;
            transform.position = new Vector3(transform.position.x, _yPos, transform.position.z);
        }
    }

    public void OnNewaction(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        float movement = context.ReadValue<float>();        
    }
}
