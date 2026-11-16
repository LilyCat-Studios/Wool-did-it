using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovementScript : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private Camera playerCamera;

    private CharacterController controller;
    private AudioListener audioListener;
    private Camera fallbackSceneCamera;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>(true);

        if (playerCamera != null)
        {
            audioListener = playerCamera.GetComponent<AudioListener>();
            playerCamera.gameObject.SetActive(false);
        }
    }

    public override void OnStartLocalPlayer()
    {
        // Turn off any default scene camera so it doesn't override the player's camera
        fallbackSceneCamera = Camera.main;
        if (fallbackSceneCamera != null && fallbackSceneCamera != playerCamera)
        {
            fallbackSceneCamera.gameObject.SetActive(false);
        }

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
            if (audioListener != null) audioListener.enabled = true;
        }

        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Enable();
        }
        else
        {
            Debug.LogError($"[{nameof(PlayerMovementScript)}] 'Move Action' is not assigned on {gameObject.name}!", this);
        }
    }

    public override void OnStopLocalPlayer()
    {
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }

        if (fallbackSceneCamera != null)
        {
            fallbackSceneCamera.gameObject.SetActive(true);
        }

        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Disable();
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (moveAction == null || moveAction.action == null) return;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 horizontalMove = new Vector3(input.x, 0f, input.y) * moveSpeed;

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 finalVelocity = horizontalMove + (Vector3.up * verticalVelocity);

        controller.Move(finalVelocity * Time.deltaTime);
    }
}
