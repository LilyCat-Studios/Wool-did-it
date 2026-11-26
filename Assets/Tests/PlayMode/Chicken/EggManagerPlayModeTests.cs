
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class EggManagerPlayModeTests
{
    private GameObject _managerObject;
    private GameObject _playerObject;

    private EggManager _eggManager;

    private readonly List<GameObject> _createdObjects = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        _managerObject = new GameObject("EggManager");
        _playerObject = new GameObject("Player");

        _eggManager = _managerObject.AddComponent<EggManager>();

        _createdObjects.Add(_managerObject);
        _createdObjects.Add(_playerObject);

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

        _eggManager.HandleEggStepped(Vector3.zero);

        Assert.That(
            chicken.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );
    }

    [Test]
    public void HandleEggStepped_ChickenOutsideRadius_DoesNotBecomeAngry()
    {
        ChickenAI chicken = CreateChicken(new Vector3(20f, 0f, 0f));

        _eggManager.HandleEggStepped(Vector3.zero);

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

        _eggManager.HandleEggStepped(Vector3.zero);

        Assert.That(
            firstChicken.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );

        Assert.That(
            secondChicken.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );
    }

    private ChickenAI CreateChicken(Vector3 position)
    {
        GameObject chickenObject = new GameObject("Chicken");
        chickenObject.transform.position = position;

        _createdObjects.Add(chickenObject);

        ChickenAI chicken = chickenObject.AddComponent<ChickenAI>();
        ChickenAttack chickenAttack = chickenObject.AddComponent<ChickenAttack>();

        SetPrivateField(chicken, "_chickenAttack", chickenAttack);
        SetPrivateField(chicken, "_player", _playerObject.transform);

        return chicken;
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
