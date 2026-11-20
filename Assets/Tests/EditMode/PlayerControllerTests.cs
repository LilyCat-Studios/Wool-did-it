using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

[Category("Player")]
public class PlayerControllerTests
{
    [Test]
    public void CalculateMoveDirection_Forward_UsesFullSpeed()
    {
        Vector3 direction = playerController.CalculateMoveDirection(new Vector2(0f, 1f), 4f);

        Assert.That(direction, Is.EqualTo(new Vector3(0f, 0f, 4f)).Using(Vector3EqualityComparer.Instance));
    }

    [Test]
    public void CalculateMoveDirection_Backward_UsesHalfSpeed()
    {
        Vector3 direction = playerController.CalculateMoveDirection(new Vector2(0f, -1f), 4f);

        Assert.That(direction, Is.EqualTo(new Vector3(0f, 0f, -2f)).Using(Vector3EqualityComparer.Instance));
    }

    [Test]
    public void CalculateMoveDirection_Sideways_UsesHalfSpeed()
    {
        Vector3 direction = playerController.CalculateMoveDirection(new Vector2(1f, 0f), 4f);

        Assert.That(direction, Is.EqualTo(new Vector3(2f, 0f, 0f)).Using(Vector3EqualityComparer.Instance));
    }

    [Test]
    public void CalculateMoveDirection_DiagonalBackward_HalvesBothDirections()
    {
        Vector3 direction = playerController.CalculateMoveDirection(new Vector2(-1f, -1f), 4f);

        Assert.That(direction, Is.EqualTo(new Vector3(-2f, 0f, -2f)).Using(Vector3EqualityComparer.Instance));
    }

    [Test]
    public void CalculateMoveDirection_NoInput_DoesNotMove()
    {
        Vector3 direction = playerController.CalculateMoveDirection(Vector2.zero, 4f);

        Assert.That(direction, Is.EqualTo(Vector3.zero).Using(Vector3EqualityComparer.Instance));
    }

    [TestCase(-50f, -30f)]   // below the minimum
    [TestCase(80f, 60f)]     // above the maximum
    [TestCase(10f, 10f)]     // within the range
    [TestCase(-30f, -30f)]   // exactly on the minimum
    [TestCase(60f, 60f)]     // exactly on the maximum
    public void ClampLook_KeepsVerticalAngleWithinRange(float lookY, float expectedY)
    {
        Vector2 look = playerController.ClampLook(new Vector2(0f, lookY), -30f, 60f);

        Assert.That(look.y, Is.EqualTo(expectedY));
    }

    [Test]
    public void ClampLook_DoesNotChangeHorizontalAngle()
    {
        Vector2 look = playerController.ClampLook(new Vector2(270f, 100f), -30f, 60f);

        Assert.That(look.x, Is.EqualTo(270f));
    }

    [Test]
    public void SmoothVelocity_NoTimePassed_StaysTheSame()
    {
        Vector2 result = playerController.SmoothVelocity(new Vector2(1f, 1f), new Vector3(4f, 0f, 4f), 0f);

        Assert.That(result.x, Is.EqualTo(1f).Within(0.001f));
        Assert.That(result.y, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void SmoothVelocity_HalfStep_MovesHalfwayToTarget()
    {
        // deltaTime 0.05 * 10 = 0.5, so the velocity moves halfway
        Vector2 result = playerController.SmoothVelocity(Vector2.zero, new Vector3(2f, 0f, 4f), 0.05f);

        Assert.That(result.x, Is.EqualTo(1f).Within(0.001f));
        Assert.That(result.y, Is.EqualTo(2f).Within(0.001f));
    }

    [Test]
    public void SmoothVelocity_UsesForwardMovementForY()
    {
        // velocity.y follows moveDirection.z (forward), not moveDirection.y
        Vector2 result = playerController.SmoothVelocity(Vector2.zero, new Vector3(0f, 9f, 4f), 0.1f);

        Assert.That(result.y, Is.EqualTo(4f).Within(0.001f));
    }

    [TestCase(90f, 90f, 0.1f, 5f, 90f)]    // already facing the camera direction
    [TestCase(90f, 0f, 0.1f, 10f, 90f)]    // deltaTime * turnSpeed = 1: full turn in one step
    [TestCase(90f, 0f, 0.1f, 5f, 45f)]     // deltaTime * turnSpeed = 0.5: half the turn
    [TestCase(90f, 0f, 0f, 5f, 0f)]        // no time passed: no turn
    [TestCase(370f, 0f, 0.1f, 5f, 365f)]   // more than a full circle: only the remaining 10 degrees are turned
    public void CalculateCharacterAngle_TurnsTowardsCameraDirection(
        float lookAngle, float characterAngle, float deltaTime, float turnSpeed, float expected)
    {
        float result = playerController.CalculateCharacterAngle(lookAngle, characterAngle, deltaTime, turnSpeed);

        Assert.That(result, Is.EqualTo(expected).Within(0.001f));
    }

    [TestCase(false, false, false)]
    [TestCase(true, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, true, true)]
    public void ShouldPauseTime_PausesWhenBoardOrMenuIsOpen(bool inBoard, bool pauseMenu, bool expected)
    {
        Assert.That(playerController.ShouldPauseTime(inBoard, pauseMenu), Is.EqualTo(expected));
    }

    [TestCase("Weapon", "shovel")]
    [TestCase("Wool", "Wool")]
    [TestCase("Location", "bodytape")]
    public void ClueMatchesPage_ClueOnItsOwnPage_ReturnsTrue(string page, string clue)
    {
        Assert.That(playerController.ClueMatchesPage(page, clue), Is.True);
    }

    [TestCase("Weapon", "Wool")]        // existing clue, wrong page
    [TestCase("Location", "shovel")]    // existing clue, wrong page
    [TestCase("Wool", "rock")]          // not a clue
    [TestCase("Weapon", "Shovel")]      // names are case-sensitive
    public void ClueMatchesPage_ClueOnOtherPage_ReturnsFalse(string page, string clue)
    {
        Assert.That(playerController.ClueMatchesPage(page, clue), Is.False);
    }
}