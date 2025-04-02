using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerMovement : MonoBehaviour, PlayerInput.IMoveActions, PlayerInput.IJumpActions
{
    private PlayerInput _controls = null;

    private Rigidbody _rigidbody = null;

    private Vector3 _moveDirection = Vector3.zero;
    private float _moveMagnitude = 0f;

    private bool _directionHeld = false;

    private PlayerShooting _playerShooting = null;

    private PlayerCamera _camera = null;

    [SerializeField]
    private float _maxMoveSpeed = 10.0f;

    [SerializeField]
    private float _accelerationIncrease = 30f;


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
    private List<Transform> BodyParts = new List<Transform>();


    [SerializeField]
    private float _jumpCooldown = 0.25f;

    private bool _isJumping = false;


    int _bloodLayerMask = 0;

    bool _canMove = true;
    bool _canJump = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controls = new PlayerInput();
        _controls.Move.SetCallbacks(this);
        _controls.Jump.SetCallbacks(this);

        _playerShooting = GetComponentInChildren<PlayerShooting>();

        if (_playerShooting != null)
        {
            _controls.Shoot.SetCallbacks(_playerShooting);
        }


        _camera = FindFirstObjectByType<PlayerCamera>();
        if(_camera != null)
        {
            _controls.RotateCamera.SetCallbacks(_camera);
        
            if(BodyParts.Count > 0)
            {
                _camera.Player = transform;
            }
        }

        var taunt = FindFirstObjectByType<PlayerTaunt>();
        if (taunt != null)
        {
            _controls.Taunt.SetCallbacks(taunt);
        }

        _controls.Enable();

        _rigidbody = GetComponent<Rigidbody>();

        _bloodLayerMask = LayerMask.GetMask("Blood");

        _bloodSplash.Stop();
    }
    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _directionHeld = true;

            var input = context.ReadValue<Vector2>();

            _moveMagnitude = input.magnitude;

            var direction = new Vector3(input.x, 0, input.y);

            if (_camera != null)
            {
                direction = _camera.RotateToCamera(direction);
            }

            _moveDirection = direction;

            if (direction != Vector3.zero)
            {
                transform.forward = direction;
            }
        }
        else if(context.canceled)
        {
            _directionHeld = false;
        }
    }

    void FixedUpdate()
    {
        if (_rigidbody != null)
        {
            var velocity = _rigidbody.linearVelocity;

            if (_directionHeld)
            {
                velocity += _moveDirection * Time.fixedDeltaTime * _accelerationIncrease;

                var magnitude = velocity.magnitude;

                magnitude = Mathf.Clamp(magnitude, 0, _maxMoveSpeed);

                velocity = velocity.normalized * magnitude;
            }
            else
            {
                var magnitude = velocity.magnitude;

                magnitude -= _accelerationIncrease * Time.fixedDeltaTime;

                magnitude = Mathf.Clamp(magnitude, 0, _maxMoveSpeed);

                velocity = velocity.normalized * magnitude;
            }

            if(!_isJumping && !IsMoveDirectionValid(velocity))
            {
               velocity = Vector3.zero;
            }

            _rigidbody.linearVelocity = velocity;

        }
    }

    bool IsMoveDirectionValid(Vector3 direction)
    {
        Ray ray = new Ray(transform.position + direction * Time.fixedDeltaTime + Vector3.up * 2, Vector3.down);

        bool result = Physics.SphereCast(ray, _epsilon, 50, _bloodLayerMask);
        return result;
    }

    [SerializeField]
    float _offsetStep = .55f;
    IEnumerator Jump(Vector3 nextBloodpool)
    {

        // start jumping
        _isJumping = true;
        _canJump = false;
        //_canMove = false;

        if (_playerShooting)
        {
            _playerShooting.ShootInhibitor += 1;
        }

        Vector3 _jumpStart = transform.position;

        bool animate = true;

        // actual jumping
        float startTime = Time.time;
        while(animate && startTime + _jumpDuration + .4f >= Time.time)
        {
            float lerpFactor = (Time.time - startTime) / _jumpDuration;

            if (lerpFactor >= 1)
            {
                _isJumping = false;
                Ray ray = new Ray(transform.position + Vector3.up * 2, Vector3.down);
                if (!Physics.SphereCast(ray, _epsilon, 50, _bloodLayerMask))
                {
                    for (int i = 0; i < BodyParts.Count; i++)
                    {
                        BodyParts[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
                        BodyParts[i].transform.localPosition = Vector3.zero +
                            (i * Vector3.down * .4f);
                    }

                    transform.position = _jumpStart;
                    animate = false;


                }
            }

            if(lerpFactor >= 1.2f)
            {
                _isJumping = false;
            }

            for (int i = 0; i < BodyParts.Count; i++)
            {
                float offset = i * _offsetStep;
                
                Vector3 newPos = Vector3.Lerp(_jumpStart,transform.position, lerpFactor - offset);
                float jumpHeight = _jumpCurve.Evaluate(lerpFactor - offset) * _jumpHeight;
                newPos.y = _jumpStart.y + jumpHeight;

                float velOffset = _rigidbody.linearVelocity.magnitude * .03f * i;
                BodyParts[i].transform.localPosition = new Vector3(0,newPos.y,  -velOffset);

                Vector3 targetDir = new Vector3(0, _jumpCurve.Evaluate(lerpFactor - offset), lerpFactor - offset) -
                    new Vector3(0, _jumpCurve.Evaluate(lerpFactor - .1f - offset), lerpFactor - .1f - offset);
                Quaternion targetRotation = Quaternion.LookRotation(targetDir, Vector3.up);
                BodyParts[i].transform.localRotation = targetRotation;

            }
            yield return null;

        }

        for (int i = 0; i < BodyParts.Count; i++)
        {
            BodyParts[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
            BodyParts[i].transform.localPosition = Vector3.zero +
                (i * Vector3.down * .4f);
        }

        _canJump = true;

        startTime = Time.time;
        while (!_isJumping && startTime + .6f >= Time.time)
        {
            Vector3 startPos = Vector3.down * .5f;
            Vector3 endPos = Vector3.zero;

            BodyParts[0].transform.localPosition = Vector3.Lerp(startPos, endPos, (Time.time - startTime) / .6f);

            Quaternion startRot = Quaternion.Euler(-90, 0, 0);
            Quaternion endRot = Quaternion.Euler(0, 0, 0);

            BodyParts[0].transform.localRotation = Quaternion.Lerp(startRot, endRot, (Time.time - startTime) / .6f);
            yield return null;
        }

        if (_playerShooting)
        {
            _playerShooting.ShootInhibitor -= 1;
        }


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
            //StartCoroutine(DisableMovement(_jumpDuration));
            StartCoroutine(Jump(nextBloodPool));
            
        }
    }

    private Vector3 FindBloodPoolLocation()
    {
        // This function will check if a bloodpool is in the direction the player is looking
        // Before registering a bloodpool, we need to find a floor piece first

        int bloodDistance = 0;

        bool floorFound = false;

        bool bloodFound = false;

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
                bloodFound = true;
                break;
            }
        }

        if(!floorFound)
        {
            bloodDistance = (int)(_jumpDistance / _epsilon);
        }
        else if(!bloodFound)
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
        
        _moveDirection = Vector3.zero;

        if(_rigidbody)
        {
            _rigidbody.linearVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(duration);
    }
}
