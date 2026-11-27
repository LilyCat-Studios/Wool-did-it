using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[Category("Player Input")]
public class SheepInputTests : InputTestFixture
{
    private const string ScenePath = "Assets/Level/Scenes/SheepExampleScene.unity";

    private Keyboard keyboard;

    private SheepMovement sheep;

    private IEnumerator Load()
    {
        keyboard = InputSystem.AddDevice<Keyboard>();

#if UNITY_EDITOR
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
        yield return null;

        // wait for the sheep to spawn
        float timeout = Time.realtimeSinceStartup + 5f;
        while (sheep == null && Time.realtimeSinceStartup < timeout)
        {
            sheep = Object.FindAnyObjectByType<SheepMovement>();
            yield return null;
        }
        Assert.That(sheep, Is.Not.Null, "Sheep never spawned.");
        Assert.That(sheep.isOwner, Is.True, "Local client does not own the sheep.");

        PlayerInput pi = sheep.GetComponent<PlayerInput>();
        InputUser.PerformPairingWithDevice(keyboard, pi.user);

        // wait until the sheep lands on the ground
        yield return new WaitUntil(
            () => sheep.IsGrounded,
            System.TimeSpan.FromSeconds(3),
            () => Assert.Fail("Sheep never landed on the ground.")
        );
    }

    private static IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }

    [TearDown]
    public void Cleanup()
    {
        foreach (var pi in Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
            pi.enabled = false;
    }

    [UnityTest]
    public IEnumerator W_MovesForward()
    {
        yield return Load();
        Press(keyboard.wKey);
        yield return Frames(30);
        Assert.That(sheep.ForwardSpeed, Is.GreaterThan(0.5f));
    }

    [UnityTest]
    public IEnumerator S_MovesBackward()
    {
        yield return Load();
        Press(keyboard.sKey);
        yield return Frames(30);
        Assert.That(sheep.ForwardSpeed, Is.LessThan(-0.5f));
    }

    [UnityTest]
    public IEnumerator D_StrafesRight()
    {
        yield return Load();
        Press(keyboard.dKey);
        yield return Frames(30);
        Assert.That(sheep.SideSpeed, Is.GreaterThan(0.5f));
    }

    [UnityTest]
    public IEnumerator A_StrafesLeft()
    {
        yield return Load();
        Press(keyboard.aKey);
        yield return Frames(30);
        Assert.That(sheep.SideSpeed, Is.LessThan(-0.5f));
    }

    [UnityTest]
    public IEnumerator NoInput_StaysStill()
    {
        yield return Load();
        Vector3 start = sheep.transform.position;
        yield return Frames(30);
        Assert.That(Vector3.Distance(
            new Vector3(start.x, 0, start.z),
            new Vector3(sheep.transform.position.x, 0, sheep.transform.position.z)), Is.LessThan(0.01f));
    }

    [UnityTest]
    public IEnumerator Shift_WithW_Sprints()
    {
        yield return Load();
        Press(keyboard.wKey);
        yield return null;
        Press(keyboard.leftShiftKey);
        yield return Frames(40);
        Assert.That(sheep.IsSprinting, Is.True,
            $"Owner: {sheep.isOwner}, InputEnabled: {sheep.InputEnabled}, ForwardSpeed: {sheep.ForwardSpeed}");
    }

    [UnityTest]
    public IEnumerator Shift_Alone_DoesNotSprint()
    {
        yield return Load();
        Press(keyboard.leftShiftKey);
        yield return Frames(20);
        Assert.That(sheep.IsSprinting, Is.False);
    }

    [UnityTest]
    public IEnumerator Space_Jumps()
    {
        yield return Load();

        Assert.That(sheep.IsGrounded, Is.True, "Sheep isn't grounded before the jump.");

        float startY = sheep.transform.position.y;
        float maxY = startY;

        Press(keyboard.spaceKey);
        yield return null;
        Release(keyboard.spaceKey);

        for (int i = 0; i < 40; i++)
        {
            maxY = Mathf.Max(maxY, sheep.transform.position.y);
            yield return null;
        }
        Assert.That(maxY - startY, Is.GreaterThan(0.5f), "Sheep did not rise.");
    }

    [UnityTest]
    public IEnumerator ReleaseShift_WhileHoldingW_ReturnsToWalking()
    {
        yield return Load();

        // Start walking and then sprinting
        Press(keyboard.wKey);
        yield return null;
        Press(keyboard.leftShiftKey);

        // Wacht tot het schaap daadwerkelijk sprint-snelheid bereikt (max 2 sec)
        yield return new WaitUntil(
            () => sheep.IsSprinting && sheep.ForwardSpeed > 1.5f,
            System.TimeSpan.FromSeconds(2),
            () => Assert.Fail($"Sheep did not reach sprint speed (IsSprinting: {sheep.IsSprinting}, Speed: {sheep.ForwardSpeed}).")
        );

        // Release Shift while keeping W held
        Release(keyboard.leftShiftKey);

        // Wacht tot het schaap weer is afgeremd naar normale loopsnelheid (max 2 sec)
        yield return new WaitUntil(
            () => !sheep.IsSprinting && sheep.ForwardSpeed <= 1.05f,
            System.TimeSpan.FromSeconds(2),
            () => Assert.Fail($"Sheep did not return to walking speed (IsSprinting: {sheep.IsSprinting}, Speed: {sheep.ForwardSpeed}).")
        );
    }

    [UnityTest]
    public IEnumerator WAndD_MovesDiagonallyForwardRight()
    {
        yield return Load();
        Vector3 startPos = sheep.transform.position;

        Press(keyboard.wKey);
        yield return null;
        Press(keyboard.dKey);
        yield return Frames(30);

        // Controleer of zowel voorwaartse als zijwaartse snelheid actief zijn
        Assert.That(sheep.ForwardSpeed, Is.GreaterThan(0.5f), "Sheep should have forward speed.");
        Assert.That(sheep.SideSpeed, Is.GreaterThan(0.5f), "Sheep should have right strafe speed.");

        // Controleer de daadwerkelijke verplaatsing op het X/Z-vlak
        Vector3 delta = sheep.transform.position - startPos;
        delta.y = 0f;

        Assert.That(delta.magnitude, Is.GreaterThan(0.1f), "Sheep did not move significantly.");

        // Bereken de lokale bewegingsrichting ten opzichte van de oriëntatie van het schaap
        Vector3 localDirection = sheep.transform.InverseTransformDirection(delta.normalized);

        // Bij een diagonale beweging zijn zowel x als z groter dan 0 (ideaal rond ~0.707)
        Assert.That(localDirection.z, Is.GreaterThan(0.3f), "Movement should have a forward component.");
        Assert.That(localDirection.x, Is.GreaterThan(0.3f), "Movement should have a right component.");
    }

}