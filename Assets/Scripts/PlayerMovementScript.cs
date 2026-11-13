using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterController))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Input Reference")]
    [SerializeField] private InputActionReference moveAction;

    private PlayerInput playerInput;
    private CharacterController controller;
    private float verticalVelocity;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();

        playerInput.enabled = false;
    }

    public override void OnStartLocalPlayer()
    {
        playerInput.enabled = true;
    }

    public override void OnStopLocalPlayer()
    {
        playerInput.enabled = false;
    }

    private void Update()
    {
        if (!isLocalPlayer || moveAction == null) return;

        // Keep player grounded
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 horizontalMove = new Vector3(input.x, 0f, input.y) * moveSpeed;

        // Apply gravity
        verticalVelocity += gravity * Time.deltaTime;
        Vector3 finalVelocity = horizontalMove + (Vector3.up * verticalVelocity);

        controller.Move(finalVelocity * Time.deltaTime);
    }
}
