
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class EggManagerTests
{
    private GameObject _managerObject;
    private GameObject _playerObject;
    private GameObject _secondPlayerObject;

    private GameObject _chickenObject;
    private GameObject _secondChickenObject;

    private EggManager _eggManager;
    private ChickenAI _chickenAI;
    private ChickenAI _secondChickenAI;

    private ChickenAttack _chickenAttack;
    private ChickenAttack _secondChickenAttack;

    [SetUp]
    public void SetUp()
    {
        _managerObject = new GameObject("Test EggManager");
        _playerObject = new GameObject("Test Player");
        _secondPlayerObject = new GameObject("Second Test Player");

        _eggManager = _managerObject.AddComponent<EggManager>();

        _chickenObject = new GameObject("Test Chicken");
        _chickenAI = _chickenObject.AddComponent<ChickenAI>();
        _chickenAttack = _chickenObject.AddComponent<ChickenAttack>();

        SetChickenAttack(_chickenAI, _chickenAttack);

        _secondChickenObject = new GameObject("Second Test Chicken");
        _secondChickenAI = _secondChickenObject.AddComponent<ChickenAI>();
        _secondChickenAttack = _secondChickenObject.AddComponent<ChickenAttack>();

        SetChickenAttack(_secondChickenAI, _secondChickenAttack);

        SetPrivateField(_eggManager, "_angerRadius", 10f);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_managerObject);
        Object.DestroyImmediate(_playerObject);
        Object.DestroyImmediate(_secondPlayerObject);
        Object.DestroyImmediate(_chickenObject);
        Object.DestroyImmediate(_secondChickenObject);
    }

    [Test]
    public void RegisterEgg_IncreasesActiveEggCount()
    {
        _eggManager.RegisterEgg();

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(1));
    }

    [Test]
    public void RegisterEgg_IncreasesTotalEggCount()
    {
        _eggManager.RegisterEgg();

        Assert.That(_eggManager.TotalEggCount, Is.EqualTo(1));
    }

    [Test]
    public void HandleEggStepped_DecreasesActiveEggCount()
    {
        _eggManager.RegisterEgg();

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _playerObject.transform
        );

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(0));
    }

    [Test]
    public void HandleEggStepped_DoesNotDecreaseActiveEggCountBelowZero()
    {
        _eggManager.HandleEggStepped(
            Vector3.zero,
            _playerObject.transform
        );

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(0));
    }

    [Test]
    public void HandleEggStepped_WhenChickenIsWithinRadius_MakesChickenAngry()
    {
        _chickenObject.transform.position = new Vector3(5f, 0f, 0f);

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _playerObject.transform
        );

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );
    }

    [Test]
    public void HandleEggStepped_WhenChickenIsOutsideRadius_DoesNotMakeChickenAngry()
    {
        _chickenObject.transform.position = new Vector3(15f, 0f, 0f);

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _playerObject.transform
        );

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Idle)
        );
    }

    [Test]
    public void HandleEggStepped_WhenChickenIsWithinRadius_SetsCorrectAttackTarget()
    {
        _chickenObject.transform.position = new Vector3(5f, 0f, 0f);

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _playerObject.transform
        );

        Assert.That(
            GetAttackTarget(_chickenAttack),
            Is.EqualTo(_playerObject.transform)
        );
    }

    [Test]
    public void HandleEggStepped_WhenChickenIsOutsideRadius_DoesNotChangeAttackTarget()
    {
        _chickenObject.transform.position = new Vector3(15f, 0f, 0f);

        _chickenAI.BecomeAngry(_playerObject.transform);

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _secondPlayerObject.transform
        );

        Assert.That(
            GetAttackTarget(_chickenAttack),
            Is.EqualTo(_playerObject.transform)
        );
    }

    [Test]
    public void HandleEggStepped_WhenChickenIsAlreadyAngry_ChangesAttackTarget()
    {
        _chickenObject.transform.position = new Vector3(5f, 0f, 0f);

        _chickenAI.BecomeAngry(_playerObject.transform);

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _secondPlayerObject.transform
        );

        Assert.That(
            GetAttackTarget(_chickenAttack),
            Is.EqualTo(_secondPlayerObject.transform)
        );
    }

    [Test]
    public void HandleEggStepped_WhenPlayerIsNull_DoesNotChangeEggCount()
    {
        _eggManager.RegisterEgg();

        _eggManager.HandleEggStepped(Vector3.zero, null);

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(1));
    }

    private void SetChickenAttack(ChickenAI chickenAI, ChickenAttack chickenAttack)
    {
        SerializedObject serializedAI = new SerializedObject(chickenAI);

        serializedAI.FindProperty("_chickenAttack")
            .objectReferenceValue = chickenAttack;

        serializedAI.ApplyModifiedPropertiesWithoutUndo();
    }

    private Transform GetAttackTarget(ChickenAttack chickenAttack)
    {
        FieldInfo targetField = typeof(ChickenAttack).GetField(
            "_target",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(targetField, Is.Not.Null);

        return (Transform)targetField.GetValue(chickenAttack);
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
