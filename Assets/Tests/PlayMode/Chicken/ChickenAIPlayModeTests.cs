
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ChickenAIPlayModeTests
{
    private GameObject _chickenObject;
    private GameObject _playerObject;
    private GameObject _secondPlayerObject;

    private ChickenAI _chickenAI;
    private ChickenAttack _chickenAttack;

    [SetUp]
    public void SetUp()
    {
        _chickenObject = new GameObject("Chicken");
        _playerObject = new GameObject("Player");
        _secondPlayerObject = new GameObject("Second Player");

        _chickenAI = _chickenObject.AddComponent<ChickenAI>();
        _chickenAttack = _chickenObject.AddComponent<ChickenAttack>();

        SetPrivateField(_chickenAI, "_chickenAttack", _chickenAttack);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_chickenObject);
        Object.Destroy(_playerObject);
        Object.Destroy(_secondPlayerObject);
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

        Assert.That(
            GetAttackTarget(),
            Is.EqualTo(_playerObject.transform)
        );
    }

    [Test]
    public void BecomeAngry_WhenAlreadyAngry_ChangesAttackTarget()
    {
        _chickenAI.BecomeAngry(_playerObject.transform);

        _chickenAI.BecomeAngry(_secondPlayerObject.transform);

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );

        Assert.That(_chickenAttack.IsAttacking, Is.True);

        Assert.That(
            GetAttackTarget(),
            Is.EqualTo(_secondPlayerObject.transform)
        );
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

    [UnityTest]
    public IEnumerator BecomeAngry_ChangesStateDuringPlayMode()
    {
        _chickenAI.BecomeAngry(_playerObject.transform);

        yield return null;

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );

        Assert.That(_chickenAttack.IsAttacking, Is.True);
    }

    private Transform GetAttackTarget()
    {
        FieldInfo field = typeof(ChickenAttack).GetField(
            "_target",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(field, Is.Not.Null);

        return (Transform)field.GetValue(_chickenAttack);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(field, Is.Not.Null);

        field.SetValue(target, value);
    }
}
