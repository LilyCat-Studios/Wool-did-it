
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class EggManagerPlayModeTests
{
    private GameObject _managerObject;
    private GameObject _playerObject;
    private GameObject _secondPlayerObject;

    private EggManager _eggManager;

    private readonly List<GameObject> _createdObjects = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        _createdObjects.Clear();

        _managerObject = new GameObject("EggManager");
        _playerObject = new GameObject("Player");
        _secondPlayerObject = new GameObject("Second Player");

        _eggManager = _managerObject.AddComponent<EggManager>();

        _createdObjects.Add(_managerObject);
        _createdObjects.Add(_playerObject);
        _createdObjects.Add(_secondPlayerObject);

        SetPrivateField(_eggManager, "_angerRadius", 10f);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject createdObject in _createdObjects)
        {
            if (createdObject != null)
                Object.Destroy(createdObject);
        }

        _createdObjects.Clear();
    }

    [Test]
    public void HandleEggStepped_ChickenWithinRadius_BecomesAngry()
    {
        ChickenAI chicken = CreateChicken(Vector3.one);

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _playerObject.transform
        );

        Assert.That(
            chicken.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );
    }

    [Test]
    public void HandleEggStepped_ChickenOutsideRadius_DoesNotBecomeAngry()
    {
        ChickenAI chicken = CreateChicken(new Vector3(20f, 0f, 0f));

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _playerObject.transform
        );

        Assert.That(
            chicken.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Idle)
        );
    }

    [Test]
    public void HandleEggStepped_MultipleChickensWithinRadius_AllBecomeAngry()
    {
        ChickenAI firstChicken = CreateChicken(Vector3.one);
        ChickenAI secondChicken = CreateChicken(new Vector3(5f, 0f, 0f));

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _playerObject.transform
        );

        Assert.That(
            firstChicken.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );

        Assert.That(
            secondChicken.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );
    }

    [Test]
    public void HandleEggStepped_ChickenWithinRadius_SetsCorrectAttackTarget()
    {
        ChickenAttack chickenAttack;

        CreateChicken(Vector3.one, out chickenAttack);

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _playerObject.transform
        );

        Assert.That(
            GetAttackTarget(chickenAttack),
            Is.EqualTo(_playerObject.transform)
        );
    }

    [Test]
    public void HandleEggStepped_ChickenAlreadyAngry_ChangesAttackTarget()
    {
        ChickenAttack chickenAttack;
        ChickenAI chicken = CreateChicken(Vector3.one, out chickenAttack);

        chicken.BecomeAngry(_playerObject.transform);

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _secondPlayerObject.transform
        );

        Assert.That(
            chicken.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );

        Assert.That(
            GetAttackTarget(chickenAttack),
            Is.EqualTo(_secondPlayerObject.transform)
        );
    }

    [Test]
    public void HandleEggStepped_ChickenOutsideRadius_KeepsCurrentAttackTarget()
    {
        ChickenAttack chickenAttack;
        ChickenAI chicken = CreateChicken(new Vector3(20f, 0f, 0f), out chickenAttack);

        chicken.BecomeAngry(_playerObject.transform);

        _eggManager.HandleEggStepped(
            Vector3.zero,
            _secondPlayerObject.transform
        );

        Assert.That(
            GetAttackTarget(chickenAttack),
            Is.EqualTo(_playerObject.transform)
        );
    }

    [Test]
    public void HandleEggStepped_WhenPlayerIsNull_DoesNotDecreaseActiveEggCount()
    {
        _eggManager.RegisterEgg();

        _eggManager.HandleEggStepped(Vector3.zero, null);

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(1));
    }

    private ChickenAI CreateChicken(Vector3 position)
    {
        ChickenAttack chickenAttack;

        return CreateChicken(position, out chickenAttack);
    }

    private ChickenAI CreateChicken(Vector3 position, out ChickenAttack chickenAttack)
    {
        GameObject chickenObject = new GameObject("Chicken");
        chickenObject.transform.position = position;

        _createdObjects.Add(chickenObject);

        ChickenAI chicken = chickenObject.AddComponent<ChickenAI>();
        chickenAttack = chickenObject.AddComponent<ChickenAttack>();

        SetPrivateField(chicken, "_chickenAttack", chickenAttack);

        return chicken;
    }

    private Transform GetAttackTarget(ChickenAttack chickenAttack)
    {
        FieldInfo field = typeof(ChickenAttack).GetField(
            "_target",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(field, Is.Not.Null);

        return (Transform)field.GetValue(chickenAttack);
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
