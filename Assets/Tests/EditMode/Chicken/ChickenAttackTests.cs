using NUnit.Framework;
using UnityEngine;

public class ChickenAttackTests
{
    private GameObject _chickenObject;
    private ChickenAttack _chickenAttack;

    [SetUp]
    public void SetUp()
    {
        _chickenObject = new GameObject("Chicken");
        _chickenAttack = _chickenObject.AddComponent<ChickenAttack>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_chickenObject);
    }

    [Test]
    public void StartAttack_SetsIsAttackingToTrue()
    {
        GameObject targetObject = new GameObject("Player");

        _chickenAttack.StartAttack(targetObject.transform);

        Assert.That(_chickenAttack.IsAttacking, Is.True);

        Object.DestroyImmediate(targetObject);
    }

    [Test]
    public void StartAttack_CalledAgain_KeepsAttacking()
    {
        GameObject firstTarget = new GameObject("FirstTarget");
        GameObject secondTarget = new GameObject("SecondTarget");

        _chickenAttack.StartAttack(firstTarget.transform);

        _chickenAttack.StartAttack(secondTarget.transform);

        Assert.That(_chickenAttack.IsAttacking, Is.True);

        Object.DestroyImmediate(firstTarget);
        Object.DestroyImmediate(secondTarget);
    }

    [Test]
    public void HandleAttack_WhenNotAttacking_ReturnsTrue()
    {
        bool result = _chickenAttack.HandleAttack();

        Assert.That(result, Is.True);
        Assert.That(_chickenAttack.IsAttacking, Is.False);
    }
}