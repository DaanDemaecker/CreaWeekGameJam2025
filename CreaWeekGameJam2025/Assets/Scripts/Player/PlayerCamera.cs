using UnityEngine;

public class PlayerCamera : MonoBehaviour, PlayerInput.IRotateCameraActions
{
    private Transform _player = null;
    public Transform Player
    {
        set
        {
            _player = value;

            if (_player != null)
            {
                _cameraOffset = transform.position - _player.position;
                _playerYPos = _player.position.y;
            }
        }
    }

    private Vector3 _cameraOffset = Vector3.zero;

    private float _yPos = 0f;
    private float _playerYPos = 0f;

    [SerializeField]
    private float _rotationSpeed = 20;

    private float _updateAngle = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        _yPos = transform.position.y;
    }


    void LateUpdate()
    {
        if (_player != null)
        {
            _cameraOffset = Quaternion.AngleAxis(_updateAngle, Vector3.up) * _cameraOffset;

            transform.position = Vector3.Lerp(transform.position, _player.position, Time.deltaTime * 3);

            //transform.LookAt(new Vector3(_player.position.x, _playerYPos, _player.position.z));
        }
    }

    public void OnNewaction(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        float movement = context.ReadValue<float>();

        _updateAngle = movement * _rotationSpeed;
    }

    public Vector3 RotateToCamera(Vector3 input)
    {
        float angle = Vector3.SignedAngle(Vector3.forward, new Vector3(transform.forward.x, 0, transform.forward.z), Vector3.up);

        Vector3 output = Quaternion.AngleAxis(angle, Vector3.up) * input;

        return output;
    }
}
