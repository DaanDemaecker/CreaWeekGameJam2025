using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerMovement : MonoBehaviour, PlayerInput.IMoveActions, PlayerInput.IJumpActions
{
    private PlayerInput _controls = null;

    private Rigidbody _rigidbody = null;

    private Vector3 _moveDirection = Vector3.zero;
    private float _moveMagnitude = 0f;

    private PlayerShooting _playerShooting = null;

    [SerializeField]
    private PlayerCamera _camera = null;

    [SerializeField]
    private float _moveSpeed = 10.0f;

    [SerializeField]
    private float _jumpDistance = 5;

    [SerializeField]
    private float _epsilon = 0.1f;

    [SerializeField]
    private AudioSource _jumpSound;
    [SerializeField]
    private AudioSource _moveSound;
    private float _moveVolume = 0.1f;

    [SerializeField]
    private VisualEffect _bloodSplash;

    [SerializeField]
    private float _jumpDuration = 0.5f;

    [SerializeField]
    private float _jumpHeight = 0.5f;

    [SerializeField]
    private AnimationCurve _jumpCurve;

    [SerializeField]
    private float _jumpCooldown = 0.25f;

    private bool _isJumping = false;

    private float _jumpTimer = 0.0f;

    private Vector3 _jumpStart = Vector3.zero;
    private Vector3 _jumpEnd = Vector3.zero;

    int _bloodLayerMask = 0;

    bool _canMove = true;
    bool _canJump = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controls = new PlayerInput();
        _controls.Move.SetCallbacks(this);
        _controls.Jump.SetCallbacks(this);

        _playerShooting = GetComponent<PlayerShooting>();

        if (_playerShooting != null)
        {
            _controls.Shoot.SetCallbacks(_playerShooting);
        }

        if(_camera != null)
        {
            _controls.RotateCamera.SetCallbacks(_camera);
        }

        _controls.Enable();

        _rigidbody = GetComponent<Rigidbody>();

        _bloodLayerMask = LayerMask.GetMask("Blood");

        _bloodSplash.Stop();
    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(!_canMove)
        {
            return;
        }

        var input = context.ReadValue<Vector2>();

        _moveMagnitude = input.magnitude;

        var direction = new Vector3(input.x, 0, input.y);

        if(_camera != null)
        {
            direction = _camera.RotateToCamera(direction);
        }

        _moveDirection = direction;

        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

    }

    void FixedUpdate()
    {
        _moveSound.volume = _moveDirection.magnitude * _moveVolume;

        if (_isJumping)
        {
            float lerpFactor = _jumpTimer / _jumpDuration;
            float jumpHeight = _jumpCurve.Evaluate(lerpFactor) * _jumpHeight;

            Vector3 newPos = Vector3.Lerp(_jumpStart, _jumpEnd, lerpFactor);

            newPos.y = _jumpStart.y + jumpHeight;
            
            transform.position = newPos;


            _jumpTimer += Time.fixedDeltaTime;

            if (_jumpTimer >= _jumpDuration)
            {
                StopJump();
            }
        }
        else if (_rigidbody != null)
        {
            _moveDirection.y = 0;
            _moveDirection.Normalize();
            _moveDirection *= _moveMagnitude * _moveSpeed;

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

        bool result = Physics.SphereCast(ray, _epsilon, 50, _bloodLayerMask);

        return result;
    }

    public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(!_canJump || !context.started)
        {
            return;
        }

        // Check for nearby bloodpool, if one is found, start jump
        var nextBloodPool = FindBloodPoolLocation();

        if (nextBloodPool != Vector3.zero)
        {
            StartJump(nextBloodPool);
        }
    }

    private Vector3 FindBloodPoolLocation()
    {
        // This function will check if a bloodpool is in the direction the player is looking
        // Before registering a bloodpool, we need to find a floor piece first

        int bloodDistance = 0;

        bool floorFound = false;

        for (int i = 0; i < (int)(_jumpDistance / _epsilon); i+=2)
        {
            Ray ray = new Ray(transform.position + transform.forward * i * _epsilon*2 + Vector3.up * 2, Vector3.down);

            bool result = Physics.SphereCast(ray, _epsilon/2, 50, _bloodLayerMask);

            if(!floorFound && !result)
            {
                floorFound = true;
                continue;
            }

            if(floorFound && result)
            {
                bloodDistance = i;
                break;
            }
        }

        if(!floorFound)
        {
            bloodDistance = (int)(_jumpDistance / _epsilon);
        }

        if (bloodDistance > 0)
        {
            return transform.position + transform.forward * bloodDistance * _epsilon * 2;
        }


        return Vector3.zero;
    }

    public IEnumerator DisableMovement(float duration)
    {
        _canMove = false;
        _moveDirection = Vector3.zero;
        yield return new WaitForSeconds(duration);
        _canMove = true;
    }

    public IEnumerator JumpCooldown(float duration)
    {
        yield return new WaitForSeconds(duration);
        _canJump = true;
    }

    private void StartJump(Vector3 nextBloodPool)
    {
        _jumpSound.Play();

        StartCoroutine(DisableMovement(_jumpDuration));
        _isJumping = true;
        _jumpStart = transform.position;
        _jumpEnd = nextBloodPool;
        _canJump = false;

        if(_playerShooting)
        {
            _playerShooting.CanShoot = false;
        }
    }

    private void StopJump()
    {
        transform.position = _jumpEnd;
        _isJumping = false;
        _jumpTimer = 0;
        StartCoroutine(JumpCooldown(_jumpCooldown));

        if (_playerShooting)
        {
            _playerShooting.CanShoot = true;
        }
    }
}
