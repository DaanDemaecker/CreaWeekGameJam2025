using UnityEngine;

public class PlayerMovement : MonoBehaviour, PlayerInput.IMoveActions, PlayerInput.IJumpActions
{
    private PlayerInput _controls = null;

    private Rigidbody _rigidbody = null;

    private Vector3 _moveDirection = Vector3.zero;

    [SerializeField]
    private float _moveSpeed = 10.0f;

    int _bloodLayerMask = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controls = new PlayerInput();
        _controls.Move.SetCallbacks(this);
        _controls.Jump.SetCallbacks(this);
        _controls.Enable();

        _rigidbody = GetComponent<Rigidbody>();

        _bloodLayerMask = LayerMask.GetMask("Blood");
    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>();

        _moveDirection = new Vector3(direction.x, 0, direction.y);

        if (direction != Vector2.zero)
        {
            transform.forward = new Vector3(direction.x, 0, direction.y);
        }
    }

    void FixedUpdate()
    {
        if (_rigidbody != null)
        {
            _moveDirection.y = 0;
            _moveDirection.Normalize();
            _moveDirection *= _moveSpeed;

            if(!IsMoveDirectionValid())
            {
                _moveDirection.x = 0;
                _moveDirection.z = 0;
            }

            _moveDirection.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = _moveDirection;
        }
    }

    bool IsMoveDirectionValid()
    {
        Ray ray = new Ray(transform.position + _moveDirection * Time.fixedDeltaTime + Vector3.up * 2, Vector3.down);

        bool result = Physics.SphereCast(ray, 0.1f, 50, _bloodLayerMask);

        return result;
    }

    public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        
    }
}
