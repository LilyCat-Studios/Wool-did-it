using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class SheepMovementTests
{
    private GameObject _sheepObject;
    private SheepMovement _movement;

    [SetUp]
    public void SetUp()
    {
        _sheepObject = new GameObject("TestSheep");
        // CharacterController is required by [RequireComponent]
        _movement = _sheepObject.AddComponent<SheepMovement>();
        _movement.InitializeForTest(Vector3.zero);
    }

    [TearDown]
    public void TearDown()
    {
        if (_sheepObject != null)
        {
            Object.DestroyImmediate(_sheepObject);
        }
    }

    #region Target Velocity & Sprinting (Owner Logic)

    [Test]
    public void CalculateTargetVelocity_WalkForward_ReturnsWalkSpeedInCameraDirection()
    {
        Vector3 target = _movement.CalculateTargetVelocity(
            input: Vector2.up,
            sprintPressed: false,
            camForward: Vector3.forward,
            camRight: Vector3.right);

        Assert.That(_movement.IsSprinting, Is.False);
        Assert.That(target.z, Is.EqualTo(3f).Within(0.001f));
        Assert.That(target.x, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void CalculateTargetVelocity_SprintWhileMovingForward_AppliesSprintMultiplier()
    {
        Vector3 target = _movement.CalculateTargetVelocity(
            input: Vector2.up,
            sprintPressed: true,
            camForward: Vector3.forward,
            camRight: Vector3.right);

        // Default _walkSpeed (3f) * _sprintMultiplier (1.8f) = 5.4f
        Assert.That(_movement.IsSprinting, Is.True);
        Assert.That(target.z, Is.EqualTo(5.4f).Within(0.001f));
    }

    [Test]
    public void CalculateTargetVelocity_SprintWhileStrafingOrBackward_DoesNotSprint()
    {
        // Alternate flow: Sprint button held while moving backward (y <= 0.1f)
        Vector3 target = _movement.CalculateTargetVelocity(
            input: Vector2.down,
            sprintPressed: true,
            camForward: Vector3.forward,
            camRight: Vector3.right);

        Assert.That(_movement.IsSprinting, Is.False);
        Assert.That(target.z, Is.EqualTo(-3f).Within(0.001f));
    }

    [Test]
    public void CalculateTargetVelocity_InputDisabled_ReturnsZeroVelocity()
    {
        // Alternate flow: Menu/map open
        _movement.InputEnabled = false;

        Vector3 target = _movement.CalculateTargetVelocity(
            input: Vector2.up,
            sprintPressed: true,
            camForward: Vector3.forward,
            camRight: Vector3.right);

        Assert.That(_movement.IsSprinting, Is.False);
        Assert.That(target, Is.EqualTo(Vector3.zero));
    }

    #endregion

    #region Jump & Gravity

    [Test]
    public void CalculateVerticalVelocity_JumpWhenGrounded_AppliesUpwardVelocity()
    {
        float dt = 0.02f;
        // Expected jump velocity: Sqrt(1.2 * -2 * -20) = Sqrt(48) ≈ 6.9282, minus gravity * dt (-0.4)
        float expected = Mathf.Sqrt(1.2f * -2f * -20f) + (-20f * dt);

        float result = _movement.CalculateVerticalVelocity(
            currentVertical: -2f,
            isGrounded: true,
            jumpPressed: true,
            dt: dt);

        Assert.That(result, Is.EqualTo(expected).Within(0.001f));
    }

    [Test]
    public void CalculateVerticalVelocity_JumpWhileAirborne_IgnoresJumpAndAppliesGravity()
    {
        // Alternate flow: Mid-air jump press
        float dt = 0.02f;
        float initialVertical = 2f;

        float result = _movement.CalculateVerticalVelocity(
            currentVertical: initialVertical,
            isGrounded: false,
            jumpPressed: true,
            dt: dt);

        Assert.That(result, Is.EqualTo(initialVertical + (-20f * dt)).Within(0.001f));
    }

    [Test]
    public void CalculateVerticalVelocity_JumpWhileInputDisabled_IgnoresJumpButAppliesGravity()
    {
        _movement.InputEnabled = false;
        float dt = 0.02f;

        float result = _movement.CalculateVerticalVelocity(
            currentVertical: -5f,
            isGrounded: true,
            jumpPressed: true,
            dt: dt);

        // Clamps to _groundStick (-2f) then applies gravity (-0.4f)
        Assert.That(result, Is.EqualTo(-2.4f).Within(0.001f));
    }

    #endregion

    #region Animation Values

    [Test]
    public void UpdateAnimationValues_NormalizedToWalkSpeed()
    {
        // Rotate sheep 90 degrees right (facing +X)
        _sheepObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        // Moving +X (forward for sheep) at 3f, +Z (left for sheep) at 1.5f
        _movement.UpdateAnimationValuesInternal(new Vector3(3f, 0f, 1.5f), vertical: -2f);

        Assert.That(_movement.ForwardSpeed, Is.EqualTo(1f).Within(0.001f));
        Assert.That(_movement.SideSpeed, Is.EqualTo(-0.5f).Within(0.001f));
        Assert.That(_movement.VerticalSpeed, Is.EqualTo(-2f).Within(0.001f));
    }

    #endregion

    #region Remote Smoothing & Teleport Guard

    [Test]
    public void SimulateRemoteStep_NormalMovement_UpdatesVelocityAndAnimation()
    {
        // Move 0.3m forward in 0.1s -> measured speed = 3 m/s (walk speed)
        // Blend = Clamp01(10f * 0.1f) = 1.0f
        _sheepObject.transform.position = new Vector3(0f, 0f, 0.3f);

        _movement.SimulateRemoteStep(0.1f);

        Assert.That(_movement.ForwardSpeed, Is.EqualTo(1f).Within(0.001f));
        Assert.That(_movement.IsSprinting, Is.False);
        Assert.That(_movement.IsGrounded, Is.True);
    }

    [Test]
    public void SimulateRemoteStep_TeleportExceedingThreshold_IgnoresSpike()
    {
        // Alternate flow: Distance > _teleportDistance (3f)
        _sheepObject.transform.position = new Vector3(0f, 0f, 10f);

        _movement.SimulateRemoteStep(0.1f);

        Assert.That(_movement.ForwardSpeed, Is.EqualTo(0f).Within(0.001f));
        Assert.That(_movement.IsSprinting, Is.False);
    }

    [Test]
    public void SimulateRemoteStep_ZeroDeltaTime_DoesNotDivideByZero()
    {
        // Alternate flow: dt <= 0
        _sheepObject.transform.position = new Vector3(0f, 0f, 1f);

        Assert.DoesNotThrow(() => _movement.SimulateRemoteStep(0f));
        Assert.That(_movement.ForwardSpeed, Is.EqualTo(0f).Within(0.001f));
    }

    #endregion
}
