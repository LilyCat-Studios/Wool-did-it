using NUnit.Framework;
using UnityEngine;

public class ChickenAttackTests
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
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_chickenObject);
        Object.DestroyImmediate(_playerObject);
    }

    [Test]
    public void StartAttack_SetsIsAttackingToTrue()
    {
        _chickenAttack.StartAttack(_playerObject.transform);

        Assert.That(_chickenAttack.IsAttacking, Is.True);
    }

    [Test]
    public void StartAttack_CalledAgain_KeepsAttacking()
    {
        _chickenAttack.StartAttack(_playerObject.transform);

        _chickenAttack.StartAttack(_playerObject.transform);

        Assert.That(_chickenAttack.IsAttacking, Is.True);
    }

    [Test]
    public void HandleAttack_WhenNotAttacking_ReturnsTrue()
    {
        bool result = _chickenAttack.HandleAttack();

        Assert.That(result, Is.True);
        Assert.That(_chickenAttack.IsAttacking, Is.False);
    }

    [Test]
    public void HandleAttack_WhenTargetIsMissing_KeepsAttackActive()
    {
        _chickenAttack.StartAttack(null);

        bool result = _chickenAttack.HandleAttack();

        Assert.That(result, Is.False);
        Assert.That(_chickenAttack.IsAttacking, Is.True);
    }
}