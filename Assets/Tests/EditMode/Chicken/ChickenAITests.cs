using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ChickenAITests
{
    private GameObject _chickenObject;
    private ChickenAI _chickenAI;
    private ChickenAttack _chickenAttack;

    [SetUp]
    public void SetUp()
    {
        _chickenObject = new GameObject("Test Chicken");

        _chickenAI = _chickenObject.AddComponent<ChickenAI>();
        _chickenAttack = _chickenObject.AddComponent<ChickenAttack>();

        SerializedObject serializedAI = new SerializedObject(_chickenAI);

        serializedAI.FindProperty("_chickenAttack")
            .objectReferenceValue = _chickenAttack;

        serializedAI.ApplyModifiedPropertiesWithoutUndo();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_chickenObject);
    }

    [Test]
    public void InitialState_IsIdle()
    {
        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Idle)
        );
    }

    [Test]
    public void BecomeAngry_ChangesStateToAngry()
    {
        _chickenAI.BecomeAngry();

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );
    }

    [Test]
    public void BecomeAngry_StartsChickenAttack()
    {
        _chickenAI.BecomeAngry();

        Assert.That(_chickenAttack.IsAttacking, Is.True);
    }

    [Test]
    public void BecomeAngry_WhenAlreadyAngry_DoesNotStopAttack()
    {
        _chickenAI.BecomeAngry();

        _chickenAI.BecomeAngry();

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );

        Assert.That(_chickenAttack.IsAttacking, Is.True);
    }
}