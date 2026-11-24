using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Owner-authoritative third-person movement for a p2p setup.
/// The owner simulates and NetworkTransform syncs the result. Remote copies
/// derive their animation values from the synced motion, so this script
/// sends no network traffic of its own.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SheepMovement : NetworkBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private string _mapName = "Player";

    [Header("Camera")]
    [SerializeField] private Transform _cameraTarget; // empty child at head height

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _sprintMultiplier = 1.8f;
    [SerializeField] private float _acceleration = 20f;
    [SerializeField] private float _turnSpeed = 720f; // degrees per second

    [Header("Jump / Gravity")]
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private float _gravity = -20f;

    [Header("Remote Smoothing")]
    [SerializeField] private float _remoteSmoothing = 10f;
    [SerializeField] private float _teleportDistance = 3f; // ignore bigger jumps per frame

    // Animation values. 1 = walk speed, _sprintMultiplier = sprint.
    // Forward: +forward / -backward. Sid: +right / -left.
    public float ForwardSpeed { get; private set; }
    public float SideSpeed { get; private set; }
    public float VerticalSpeed { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsGrounded { get; private set; }

    /// <summary>Set false while a menu, notebook or map is open. Gravity still applies.</summary>
    public bool InputEnabled { get; set; } = true;

    private const float _groundStick = -2f;

    private CharacterController _controller;
    private InputAction _move, _sprint, _jump;
    private Transform _cam;

    private Vector3 _velocity;          // horizontal, world space
    private float _verticalVelocity;
    private Vector3 _lastPosition;      // remote copies only
    private bool _active;               // true between spawn and despawn
    private bool _inputReady;

    private void Awake() => _controller = GetComponent<CharacterController>();

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);

        // Only the owner simulates; NetworkTransform drives everyone else.
        _controller.enabled = isOwner;
        _lastPosition = transform.position;
        _active = true;

        // A host gets OnSpawned twice, so initialise the owner side once.
        if (!isOwner || _inputReady) return;

        InputActionMap map = _inputActions.FindActionMap(_mapName, true);
        _move = map.FindAction("Move", true);
        _sprint = map.FindAction("Sprint", true);
        _jump = map.FindAction("Jump", true);
        _move.Enable();
        _sprint.Enable();
        _jump.Enable();
        _inputReady = true;

        Camera mainCam = Camera.main;
        _cam = mainCam != null ? mainCam.transform : null;

        CinemachineCamera vcam = FindAnyObjectByType<CinemachineCamera>();
        if (vcam != null)
            vcam.Target.TrackingTarget = _cameraTarget != null ? _cameraTarget : transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected override void OnDespawned(bool asServer)
    {
        _active = false;

        if (_inputReady)
        {
            _move.Disable();
            _sprint.Disable();
            _jump.Disable();
            _inputReady = false;
        }

        base.OnDespawned(asServer);
    }

    private void Update()
    {
        if (!_active) return;

        if (isOwner) OwnerUpdate(Time.deltaTime);
        else RemoteUpdate(Time.deltaTime);
    }

    private void OwnerUpdate(float dt)
    {
        if (!_inputReady) return;

        // Zero input while a menu is open; gravity and deceleration still run.
        Vector2 input = InputEnabled
            ? Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f)
            : Vector2.zero;

        // Sprint only counts while moving forward.
        IsSprinting = InputEnabled && input.y > 0.1f && _sprint.IsPressed();
        float speed = _walkSpeed * (IsSprinting ? _sprintMultiplier : 1f);
        // Camera-relative directions, flattened to the ground plane.
        Vector3 camForward = Flatten(_cam != null ? _cam.forward : transform.forward);
        Vector3 camRight = Flatten(_cam != null ? _cam.right : transform.right);
        Vector3 target = (camForward * input.y + camRight * input.x) * speed;

        _velocity = Vector3.MoveTowards(_velocity, target, _acceleration * dt);

        // Face the camera direction (strafe style), so the animation values make sense.
        if (camForward.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, Quaternion.LookRotation(camForward), _turnSpeed * dt);
        }

        // Gravity and jump.
        bool grounded = _controller.isGrounded;
        if (grounded && _verticalVelocity < 0f)
            _verticalVelocity = _groundStick;

        if (grounded && InputEnabled && _jump.WasPressedThisFrame())
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

        _verticalVelocity += _gravity * dt;

        _controller.Move(new Vector3(_velocity.x, _verticalVelocity, _velocity.z) * dt);

        IsGrounded = _controller.isGrounded;
        UpdateAnimationValues(_velocity, _verticalVelocity);
    }

    // Non-owners: measure the motion that NetworkTransform produced.
    private void RemoteUpdate(float dt)
    {
        if (dt <= 0f) return;

        Vector3 delta = transform.position - _lastPosition;
        _lastPosition = transform.position;

        // Skip teleports (respawn, late-join snap) so animations don't spike.
        if (delta.sqrMagnitude > _teleportDistance * _teleportDistance) return;

        Vector3 measured = delta / dt;
        float blend = Mathf.Clamp01(_remoteSmoothing * dt);

        _velocity = Vector3.Lerp(_velocity, new Vector3(measured.x, 0f, measured.z), blend);
        _verticalVelocity = Mathf.Lerp(_verticalVelocity, measured.y, blend);

        // Halfway between walk and sprint speed.
        float threshold = _walkSpeed * (1f + _sprintMultiplier) * 0.5f;
        IsSprinting = _velocity.sqrMagnitude > threshold * threshold;
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

    private static Vector3 Flatten(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }
}