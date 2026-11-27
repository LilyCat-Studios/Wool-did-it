using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Owner-authoritative third-person movement for a p2p setup.
/// NetworkTransform syncs the Position.
/// Remote copies derive their animation values from the synced motion.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class SheepMovement : NetworkBehaviour
{
    [Header("Camera")]
    [Tooltip("Empty child transform at head height used as the Cinemachine tracking target.")]
    [SerializeField] private Transform _cameraTarget;

    [Tooltip("speed for turning toward the camera yaw. Higher values turn faster.")]
    [SerializeField] private float _rotationSpeed = 20f;

    [Header("Movement")]
    [Tooltip("Base horizontal movement speed.")]
    [SerializeField] private float _walkSpeed = 3f;

    [Tooltip("Speed multiplier applied when sprinting forward.")]
    [SerializeField] private float _sprintMultiplier = 1.8f;

    [Tooltip("Rate at which horizontal velocity accelerates or decelerates.")]
    [SerializeField] private float _acceleration = 20f;

    [Header("Jump / Gravity")]
    [Tooltip("Maximum jump height.")]
    [SerializeField] private float _jumpHeight = 1.2f;

    [Tooltip("Constant downward acceleration applied to the character.")]
    [SerializeField] private float _gravity = -20f;

    [Header("Remote Smoothing")]
    [Tooltip("Rate at which remote clients smooth their estimated velocity and vertical position.")]
    [SerializeField] private float _remoteSmoothing = 10f;

    [Tooltip("Distance threshold for ignoring large position snaps (e.g., spawns or teleports) when estimating remote velocity.")]
    [SerializeField] private float _teleportDistance = 3f;

    // Normalized forward/backward speed for animator blend trees
    public float ForwardSpeed { get; private set; }

    // Normalized right/left strafe speed for animator blend trees
    public float SideSpeed { get; private set; }
    public float VerticalSpeed { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsGrounded { get; private set; }

    // Set to false when UI menus or overlays are open to block movement input while keeping gravity active
    public bool InputEnabled { get; set; } = true;

    // Constant downward force applied while grounded to keep the character attached to the ground.
    private const float _groundStick = -2f;

    private CharacterController _controller;
    private PlayerInput _playerInput;
    private Transform _cam;
    private CinemachineOrbitalFollow _orbitalFollow;

    // Cached input values populated by PlayerInput SendMessages callbacks.
    private Vector2 _moveInput;
    private bool _sprintPressed;
    private bool _jumpPressed;

    private Vector3 _velocity;
    private float _verticalVelocity;
    private Vector3 _lastPosition;
    private bool _active;
    private bool _ownerInitialised;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
    }

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);

        // Only the owning client simulates physics and reads local input devices.
        _controller.enabled = isOwner;
        _playerInput.enabled = isOwner;
        _lastPosition = transform.position;
        _active = true;

        if (!isOwner || _ownerInitialised) return;
        _ownerInitialised = true;

        if (Camera.main != null)
            _cam = Camera.main.transform;

        // Bind the local Cinemachine camera to this player's tracking target.
        CinemachineCamera vcam = FindAnyObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Target.TrackingTarget = _cameraTarget != null ? _cameraTarget : transform;
            _orbitalFollow = vcam.GetComponent<CinemachineOrbitalFollow>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected override void OnDespawned(bool asServer)
    {
        _active = false;
        base.OnDespawned(asServer);
    }

    // ---- PlayerInput Callbacks (Requires Behavior = Send Messages) ----
    private void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();
    private void OnSprint(InputValue value) => _sprintPressed = value.isPressed;
    private void OnJump(InputValue value) { if (value.isPressed) _jumpPressed = true; }

    private void Update()
    {
        if (!_active) return;

        if (isOwner) OwnerUpdate(Time.deltaTime);
        else RemoteUpdate(Time.deltaTime);
    }

    // Owner-authoritative movement, rotation and gravity.
    private void OwnerUpdate(float dt)
    {
        if (!_ownerInitialised || dt <= 0f) return;

        bool jumpPressed = _jumpPressed;
        _jumpPressed = false;

        Vector3 camForward = _cam != null ? _cam.forward : transform.forward;
        Vector3 camRight = _cam != null ? _cam.right : transform.right;

        Vector3 targetVelocity = CalculateTargetVelocity(_moveInput, _sprintPressed, camForward, camRight);
        _velocity = Vector3.MoveTowards(_velocity, targetVelocity, _acceleration * dt);

        UpdateRotation(dt);

        _verticalVelocity = CalculateVerticalVelocity(_verticalVelocity, _controller.isGrounded, jumpPressed, dt);
        _controller.Move(new Vector3(_velocity.x, _verticalVelocity, _velocity.z) * dt);

        IsGrounded = _controller.isGrounded;
        UpdateAnimationValues(_velocity, _verticalVelocity);
    }

    // Smoothly aligns character yaw with the orbital camera's horizontal axis.
    private void UpdateRotation(float dt)
    {
        float targetYaw = _orbitalFollow != null
            ? _orbitalFollow.HorizontalAxis.Value
            : (_cam != null ? _cam.eulerAngles.y : transform.eulerAngles.y);

        Quaternion targetRotation = Quaternion.Euler(0f, targetYaw, 0f);
        float blend = 1f - Mathf.Exp(-_rotationSpeed * dt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, blend);
    }

    // Estimates velocity and grounded state on non-owning clients from position changes.
    private void RemoteUpdate(float dt)
    {
        if (dt <= 0f) return;

        Vector3 delta = transform.position - _lastPosition;
        _lastPosition = transform.position;

        // Ignore large position snaps (e.g., spawns or teleports)
        if (delta.sqrMagnitude > _teleportDistance * _teleportDistance) return;

        Vector3 measured = delta / dt;
        float blend = Mathf.Clamp01(_remoteSmoothing * dt);

        _velocity = Vector3.Lerp(_velocity, new Vector3(measured.x, 0f, measured.z), blend);
        _verticalVelocity = Mathf.Lerp(_verticalVelocity, measured.y, blend);

        float sprintThreshold = _walkSpeed * (1f + _sprintMultiplier) * 0.5f;
        IsSprinting = _velocity.sqrMagnitude > sprintThreshold * sprintThreshold;
        IsGrounded = Mathf.Abs(_verticalVelocity) < 0.5f;

        UpdateAnimationValues(_velocity, _verticalVelocity);
    }

    private void UpdateAnimationValues(Vector3 worldVelocity, float vertical)
    {
        Vector3 local = transform.InverseTransformDirection(worldVelocity);
        ForwardSpeed = local.z / _walkSpeed;
        SideSpeed = local.x / _walkSpeed;
        VerticalSpeed = vertical;
    }

    // Projects a direction vector onto the horizontal XZ plane and normalizes it.
    private static Vector3 Flatten(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }

    internal Vector3 CalculateTargetVelocity(Vector2 input, bool sprintPressed, Vector3 camForward, Vector3 camRight)
    {
        Vector2 clampedInput = InputEnabled ? Vector2.ClampMagnitude(input, 1f) : Vector2.zero;
        IsSprinting = InputEnabled && clampedInput.y > 0.1f && sprintPressed;
        float speed = _walkSpeed * (IsSprinting ? _sprintMultiplier : 1f);

        return (Flatten(camForward) * clampedInput.y + Flatten(camRight) * clampedInput.x) * speed;
    }

    internal float CalculateVerticalVelocity(float currentVertical, bool isGrounded, bool jumpPressed, float dt)
    {
        if (isGrounded && currentVertical < 0f)
            currentVertical = _groundStick;

        if (isGrounded && InputEnabled && jumpPressed)
            currentVertical = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

        return currentVertical + (_gravity * dt);
    }

    // ---- Unit Test Helpers ----
    internal void InitializeForTest(Vector3 initialPosition)
    {
        _lastPosition = initialPosition;
        _active = true;
    }

    internal void SimulateRemoteStep(float dt) => RemoteUpdate(dt);
    internal void UpdateAnimationValuesInternal(Vector3 worldVelocity, float vertical) => UpdateAnimationValues(worldVelocity, vertical);
}
