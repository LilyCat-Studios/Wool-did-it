using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ChickenAITests
{
    private GameObject _chickenObject;
    private GameObject _playerObject;

    private ChickenAI _chickenAI;
    private ChickenAttack _chickenAttack;

    [SetUp]
    public void SetUp()
    {
        _chickenObject = new GameObject("Test Chicken");
        _playerObject = new GameObject("Test Player");

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
        Object.DestroyImmediate(_playerObject);
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
        _chickenAI.BecomeAngry(_playerObject.transform);

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );
    }

    [Test]
    public void BecomeAngry_StartsChickenAttack()
    {
        _chickenAI.BecomeAngry(_playerObject.transform);

        Assert.That(_chickenAttack.IsAttacking, Is.True);
    }

    [Test]
    public void BecomeAngry_SetsCorrectAttackTarget()
    {
        _chickenAI.BecomeAngry(_playerObject.transform);

        Transform target = GetAttackTarget();

        Assert.That(target, Is.EqualTo(_playerObject.transform));
    }

    [Test]
    public void BecomeAngry_WhenAlreadyAngry_ChangesAttackTarget()
    {
        GameObject secondPlayerObject = new GameObject("Second Player");

        try
        {
            _chickenAI.BecomeAngry(_playerObject.transform);

            _chickenAI.BecomeAngry(secondPlayerObject.transform);

            Assert.That(
                _chickenAI.CurrentState,
                Is.EqualTo(ChickenAI.ChickenState.Angry)
            );

            Assert.That(_chickenAttack.IsAttacking, Is.True);

            Assert.That(
                GetAttackTarget(),
                Is.EqualTo(secondPlayerObject.transform)
            );
        }
        finally
        {
            Object.DestroyImmediate(secondPlayerObject);
        }
    }

    [Test]
    public void BecomeAngry_WhenTargetIsNull_DoesNotStartAttack()
    {
        _chickenAI.BecomeAngry(null);

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Idle)
        );

        Assert.That(_chickenAttack.IsAttacking, Is.False);
    }

    private Transform GetAttackTarget()
    {
        var targetField = typeof(ChickenAttack).GetField(
            "_target",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic
        );

        Assert.That(targetField, Is.Not.Null);

        return (Transform)targetField.GetValue(_chickenAttack);
    }
}
