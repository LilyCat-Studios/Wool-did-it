using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ChickenAttackPlayModeTests
{
    private GameObject _chickenObject;
    private GameObject _playerObject;

    private ChickenAttack _chickenAttack;

    [SetUp]
    public void SetUp()
    {
        _chickenObject = new GameObject("Chicken");
        _playerObject = new GameObject("Player");

        _chickenAttack = _chickenObject.AddComponent<ChickenAttack>();

        SetPrivateField(_chickenAttack, "_attackDuration", 0.1f);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_chickenObject);
        Object.Destroy(_playerObject);
    }

    [UnityTest]
    public IEnumerator HandleAttack_WhenDurationExpires_StopsAttack()
    {
        _chickenAttack.StartAttack(_playerObject.transform);

        bool result = false;

        while (!result)
        {
            yield return null;
            result = _chickenAttack.HandleAttack();
        }

        Assert.That(result, Is.True);
        Assert.That(_chickenAttack.IsAttacking, Is.False);
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