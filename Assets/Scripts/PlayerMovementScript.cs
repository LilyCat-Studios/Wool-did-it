using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMovementScript : NetworkBehaviour
{
    [Header("Movement & Look")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float mouseSensitivity = 0.15f;

    [Header("References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private Camera playerCamera;

    private CharacterController controller;
    private Camera fallbackSceneCamera;
    private float verticalVelocity;
    private float cameraPitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.enabled = false;

        if (!playerCamera) playerCamera = GetComponentInChildren<Camera>(true);
        if (playerCamera) playerCamera.gameObject.SetActive(false);
    }

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);
        if (!asServer && isOwner) EnableLocalPlayer();
    }

    protected override void OnDespawned(bool asServer)
    {
        base.OnDespawned(asServer);
        if (!asServer && isOwner) DisableLocalPlayer();
    }

    private void EnableLocalPlayer()
    {
        controller.enabled = true;

        fallbackSceneCamera = Camera.main;
        if (fallbackSceneCamera && fallbackSceneCamera != playerCamera)
            fallbackSceneCamera.gameObject.SetActive(false);

        if (playerCamera) playerCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        moveAction?.action?.Enable();
        lookAction?.action?.Enable();
        jumpAction?.action?.Enable();
    }

    private void DisableLocalPlayer()
    {
        controller.enabled = false;

        if (playerCamera) playerCamera.gameObject.SetActive(false);
        if (fallbackSceneCamera) fallbackSceneCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        moveAction?.action?.Disable();
        lookAction?.action?.Disable();
        jumpAction?.action?.Disable();
    }

    private void Update()
    {
        if (!isOwner || !controller.enabled) return;

        Vector2 move = moveAction ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        Vector2 look = lookAction ? lookAction.action.ReadValue<Vector2>() * mouseSensitivity : Vector2.zero;
        bool jump = jumpAction && jumpAction.action.WasPressedThisFrame();

        cameraPitch = Mathf.Clamp(cameraPitch - look.y, -89f, 89f);
        if (playerCamera) playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * look.x);

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;
            if (jump) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 moveDir = transform.right * move.x + transform.forward * move.y;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        controller.Move(((moveDir * moveSpeed) + (Vector3.up * verticalVelocity)) * Time.deltaTime);
    }
}
