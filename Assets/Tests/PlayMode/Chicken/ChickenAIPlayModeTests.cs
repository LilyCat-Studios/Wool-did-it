
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ChickenAIPlayModeTests
{
    private GameObject _chickenObject;
    private GameObject _playerObject;

    private ChickenAI _chickenAI;
    private ChickenAttack _chickenAttack;

    [SetUp]
    public void SetUp()
    {
        _chickenObject = new GameObject("Chicken");
        _playerObject = new GameObject("Player");

        _chickenAI = _chickenObject.AddComponent<ChickenAI>();
        _chickenAttack = _chickenObject.AddComponent<ChickenAttack>();

        SetPrivateField(_chickenAI, "_player", _playerObject.transform);
        SetPrivateField(_chickenAI, "_chickenAttack", _chickenAttack);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_chickenObject);
        Object.Destroy(_playerObject);
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
    public void BecomeAngry_WhenAlreadyAngry_DoesNotRestartAttack()
    {
        _chickenAI.BecomeAngry();

        _chickenAI.BecomeAngry();

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );

        Assert.That(_chickenAttack.IsAttacking, Is.True);
    }

    [UnityTest]
    public IEnumerator BecomeAngry_ChangesStateDuringPlayMode()
    {
        _chickenAI.BecomeAngry();

        yield return null;

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );
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
