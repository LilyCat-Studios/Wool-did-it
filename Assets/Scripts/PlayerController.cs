using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SyncVar]
    public int health = 100;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Security PoC")]
    public bool enableValidation = false;
    
    private Vector2 serverMovement;

    private void Update()
    {
        if (isLocalPlayer)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector2 movement = new Vector2(horizontal, vertical);
            movement = Vector2.ClampMagnitude(movement, 1f);
            
            CmdSetMovement(movement);
        }

        // Only the server moves the players.
        if (isServer)
        {
            Vector3 direction = new Vector3(
                serverMovement.x,
                0f,
                serverMovement.y
            );

            transform.position +=
                direction * moveSpeed * Time.deltaTime;
        }
    }

    [Command]
    private void CmdSetMovement(Vector2 movement)
    {
        serverMovement = Vector2.ClampMagnitude(movement, 1f);
    }

    private void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        float panelWidth = 300f;
        float panelHeight = 220f;
        float margin = 10f;

        GUILayout.BeginArea(new Rect(
            Screen.width - panelWidth - margin,
            margin,
            panelWidth,
            panelHeight
        ));
        
        GUILayout.Label("Your health: " + health);
        GUILayout.Space(10);

        GUILayout.Label("Movement: WASD");

        if (GUILayout.Button("Normal attack (10 damage)"))
        {
            CmdRequestAttack(10);
        }

        if (GUILayout.Button("Invalid attack (9999 damage)"))
        {
            CmdRequestAttack(9999);
        }

        if (GUILayout.Button("Negative damage (-100)"))
        {
            CmdRequestAttack(-100);
        }

        GUILayout.EndArea();
    }

    [Command]
    public void CmdRequestAttack(int requestedDamage)
    {
        Debug.Log("Received attack request: " + requestedDamage);

        if (enableValidation)
        {
            if (requestedDamage <= 0 || requestedDamage > 20)
            {
                Debug.LogWarning(
                    "Attack rejected! Invalid damage: " + requestedDamage
                );
                return;
            }
        }

        PlayerController[] players =
            FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            if (player == this)
                continue;

            player.health -= requestedDamage;

            Debug.Log(
                "Player received " + requestedDamage +
                " damage. Remaining HP: " + player.health
            );
        }
    }
}
