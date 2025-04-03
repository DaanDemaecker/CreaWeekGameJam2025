using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerMovement : MonoBehaviour, PlayerInput.IMoveActions, PlayerInput.IJumpActions
{
    // Input and component references
    private PlayerInput _controls;
    private Rigidbody _rigidbody;
    private PlayerShooting _playerShooting;
    private PlayerCamera _playerCamera;

    // Movement variables
    [SerializeField] private float _maxMoveSpeed = 10f;
    [SerializeField] private float _acceleration = 30f;
    private Vector3 _moveDirection = Vector3.zero;
    private float _moveMagnitude = 0f;
    private bool _isDirectionHeld = false;

    // Jump variables
    [SerializeField] private float _jumpDistance = 5f;
    [SerializeField] private float _jumpDuration = 0.5f;
    [SerializeField] private float _jumpHeight = 0.5f;
    [SerializeField] private AnimationCurve _jumpCurve;
    [SerializeField] private float _bodyPartOffsetStep = 0.55f;
    private bool _isJumping = false;
    private bool _canJump = true;

    // Collision and environment
    [SerializeField] private float _collisionRadius = 0.1f; // Previously _epsilon
    private int _bloodLayerMask;
    private int _wallLayerMask;

    // Audio and visual effects
    [SerializeField] private AudioSource _jumpSound;
    [SerializeField] private AudioSource _moveSound;
    [SerializeField] private float _moveSoundVolumeScale = 0.1f;
    [SerializeField] private AudioSource _deathSound;
    [SerializeField] private VisualEffect _bloodSplash;
    [SerializeField] private VisualEffect _deathEffect;

    // Body parts for animation
    [SerializeField] private List<Transform> _bodyParts = new List<Transform>();

    void Start()
    {
        // Initialize components and input system
        _rigidbody = GetComponent<Rigidbody>();
        _playerShooting = GetComponent<PlayerShooting>();
        _playerCamera = FindFirstObjectByType<PlayerCamera>();

        _controls = new PlayerInput();
        _controls.Move.SetCallbacks(this);
        _controls.Jump.SetCallbacks(this);

        if (_playerShooting != null)
            _controls.Shoot.SetCallbacks(_playerShooting);

        if (_playerCamera != null)
        {
            _controls.RotateCamera.SetCallbacks(_playerCamera);
            if (_bodyParts.Count > 0)
                _playerCamera.Player = transform;
        }

        if (TryGetComponent<PlayerTaunt>(out var taunt))
            _controls.Taunt.SetCallbacks(taunt);

        if (TryGetComponent<PlayerMelee>(out var melee))
            _controls.Melee.SetCallbacks(melee);

        _controls.Enable();

        // Set up layer masks
        _bloodLayerMask = LayerMask.GetMask("Blood");
        _wallLayerMask = 1 << 16; // Assuming layer 16 is for walls/obstacles

        // Initialize effects
        _bloodSplash.Stop();
        _deathEffect.Stop();

        // Start initial animation
        StartCoroutine(InitialAnimation());
    }

    /// <summary>
    /// Plays an initial animation for the player, such as emerging from a surface.
    /// </summary>
    private IEnumerator InitialAnimation()
    {
        _canJump = false;
        ResetBodyParts();

        float duration = 0.6f;
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            _bodyParts[0].localPosition = Vector3.Lerp(Vector3.down * 0.5f, Vector3.zero, t);
            _bodyParts[0].localRotation = Quaternion.Lerp(Quaternion.Euler(-90, 0, 0), Quaternion.Euler(0, 0, 0), t);
            yield return null;
        }
        _canJump = true;
    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isDirectionHeld = true;
            Vector2 input = context.ReadValue<Vector2>();
            _moveMagnitude = input.magnitude;
            Vector3 direction = new Vector3(input.x, 0, input.y).normalized;
            _moveDirection = _playerCamera != null ? _playerCamera.RotateToCamera(direction) : direction;
        }
        else if (context.canceled)
        {
            _isDirectionHeld = false;
        }
    }

    void FixedUpdate()
    {
        Vector3 velocity = _rigidbody.linearVelocity;

        // Apply acceleration or deceleration
        if (_isDirectionHeld)
        {
            velocity += _moveDirection * _acceleration * Time.fixedDeltaTime;
            velocity = Vector3.ClampMagnitude(velocity, _maxMoveSpeed);
        }
        else
        {
            float magnitude = velocity.magnitude - _acceleration * Time.fixedDeltaTime;
            velocity = velocity.normalized * Mathf.Max(magnitude, 0);
        }

        // Adjust velocity if move direction is invalid (e.g., hitting a wall or sliding off blood)
        if (!_isJumping && !IsMoveDirectionValid(_moveDirection, out Vector3 adjustedVelocity))
            velocity = adjustedVelocity;

        _rigidbody.linearVelocity = velocity;

        // Rotate player to face movement direction
        if (velocity != Vector3.zero)
            transform.forward = Vector3.Lerp(transform.forward, velocity, Time.fixedDeltaTime * 4);

        // Update movement sound volume
        _moveSound.volume = _isJumping ? 0 : velocity.magnitude * _moveSoundVolumeScale;
    }

    /// <summary>
    /// Checks if the intended move direction is valid and adjusts velocity if necessary.
    /// Returns true if the direction keeps the player on a blood pool, false otherwise.
    /// </summary>
    private bool IsMoveDirectionValid(Vector3 direction, out Vector3 adjustedVelocity)
    {
        adjustedVelocity = Vector3.zero;
        float stepDistance = 0.3f;

        // Check for wall collision
        if (Physics.Raycast(transform.position, direction, stepDistance, _wallLayerMask))
            return false;

        Vector3 nextPosition = transform.position + direction * Time.fixedDeltaTime;
        Ray ray = new Ray(nextPosition + Vector3.up * 2, Vector3.down);

        // Check if there's a blood pool under the next position
        bool onBloodPool = Physics.SphereCast(ray, _collisionRadius, 4f, _bloodLayerMask);
        if (!onBloodPool && direction != Vector3.zero)
        {
            // Check for nearby blood piles to slide along
            Collider[] bloodPiles = Physics.OverlapSphere(transform.position, _collisionRadius * 2, _bloodLayerMask);
            if (bloodPiles.Length > 0)
            {
                Collider closestPile = GetClosestCollider(bloodPiles);
                Vector3 normal = (transform.position - closestPile.transform.position).normalized;
                if (Vector3.Dot(direction, normal) < -0.11f)
                {
                    adjustedVelocity = Vector3.Reflect(direction, normal).normalized * _maxMoveSpeed;
                    return false;
                }
            }
        }
        return onBloodPool;
    }

    private Collider GetClosestCollider(Collider[] colliders)
    {
        float minDistance = Mathf.Infinity;
        Collider closest = null;
        foreach (var collider in colliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = collider;
            }
        }
        return closest;
    }

    /// <summary>
    /// Handles the player's jump to a target blood pool position.
    /// </summary>
    private IEnumerator Jump(Vector3 targetPosition)
    {
        _isJumping = true;
        _canJump = false;

        // Takeoff effects
        _bloodSplash.SetBool("HeavySplash", false);
        _bloodSplash.Play();
        _jumpSound.Play();

        Vector3 startPosition = transform.position;
        float startTime = Time.time;

        while (Time.time < startTime + _jumpDuration)
        {
            float t = (Time.time - startTime) / _jumpDuration;
            Vector3 position = Vector3.Lerp(startPosition, targetPosition, t);
            position.y += _jumpCurve.Evaluate(t) * _jumpHeight;
            transform.position = position;

            // Animate body parts with a trailing effect
            for (int i = 0; i < _bodyParts.Count; i++)
            {
                float offset = i * _bodyPartOffsetStep;
                float partT = Mathf.Clamp01(t - offset);
                Vector3 partPos = Vector3.Lerp(startPosition, targetPosition, partT);
                partPos.y += _jumpCurve.Evaluate(partT) * _jumpHeight;
                _bodyParts[i].position = partPos;
            }
            yield return null;
        }

        // Ensure final position
        transform.position = targetPosition;

        // Check landing on blood pool
        Ray ray = new Ray(transform.position + Vector3.up * 2, Vector3.down);
        if (!Physics.SphereCast(ray, _collisionRadius, 4f, _bloodLayerMask))
        {
            // Death sequence
            ResetBodyParts();
            if (Camera.main.TryGetComponent<ScreenShake>(out var cameraShake))
                cameraShake.StartShake(0.2f);
            _deathSound.Play();
            _deathEffect.Play();
            transform.position = startPosition;
        }
        else
        {
            // Landing effects
            _bloodSplash.SetBool("HeavySplash", true);
            _bloodSplash.Stop();
            _bloodSplash.Play();
            _jumpSound.pitch = Random.Range(0.5f, 1.5f);
            _jumpSound.Play();
        }

        // Reset body parts with animation
        yield return StartCoroutine(ResetBodyPartsSmoothly(0.6f));
        _isJumping = false;
        _canJump = true;
    }

    private void ResetBodyParts()
    {
        for (int i = 0; i < _bodyParts.Count; i++)
        {
            _bodyParts[i].localPosition = Vector3.down * 0.4f * i;
            _bodyParts[i].localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    private IEnumerator ResetBodyPartsSmoothly(float duration)
    {
        float startTime = Time.time;
        Vector3[] startPositions = _bodyParts.Select(bp => bp.localPosition).ToArray();
        Quaternion[] startRotations = _bodyParts.Select(bp => bp.localRotation).ToArray();

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            for (int i = 0; i < _bodyParts.Count; i++)
            {
                _bodyParts[i].localPosition = Vector3.Lerp(startPositions[i], Vector3.down * 0.4f * i, t);
                _bodyParts[i].localRotation = Quaternion.Lerp(startRotations[i], Quaternion.Euler(Vector3.zero), t);
            }
            yield return null;
        }
        ResetBodyParts();
    }

    public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!_canJump || !context.started)
            return;

        Vector3 targetBloodPool = FindBloodPoolLocation();
        if (targetBloodPool != Vector3.zero)
            StartCoroutine(Jump(targetBloodPool));
    }

    /// <summary>
    /// Finds the next valid blood pool position in the forward direction.
    /// </summary>
    private Vector3 FindBloodPoolLocation()
    {
        int floorLayerMask = LayerMask.GetMask("Floor"); // Define in Unity editor
        Vector3 direction = transform.forward;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, _jumpDistance, floorLayerMask))
        {
            Vector3 hitPoint = hit.point;
            if (Physics.CheckSphere(hitPoint, _collisionRadius, _bloodLayerMask))
                return hitPoint;
        }
        return Vector3.zero; // No valid jump target found
    }

    private void OnDisable()
    {
        _controls?.Disable();
        StopAllCoroutines();
    }
}
