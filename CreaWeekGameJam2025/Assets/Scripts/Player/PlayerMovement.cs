using UnityEngine;

public class PlayerMovement : MonoBehaviour, PlayerInput.IMoveActions
{
    private PlayerInput _controls = null;

    private Rigidbody _rigidbody = null;

    private Vector3 _moveDirection = Vector3.zero;

    [SerializeField]
    private float _moveSpeed = 10.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controls = new PlayerInput();
        _controls.Move.SetCallbacks(this);
        _controls.Enable();

        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>();

        Debug.Log(direction);

        _moveDirection = new Vector3(direction.x, 0, direction.y);

    }

    void FixedUpdate()
    {
        if(_rigidbody != null)
        {
            if (IsMoveDirectionValid())
            {
                _moveDirection.y = 0;
                _moveDirection.Normalize();
                _moveDirection *= _moveSpeed;
            }
            else
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


        return true;
    }
}
