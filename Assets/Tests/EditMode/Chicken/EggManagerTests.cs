
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class EggManagerTests
{
    private GameObject _managerObject;
    private EggManager _eggManager;

    private readonly List<GameObject> _createdObjects = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        _managerObject = new GameObject("EggManager");
        _eggManager = _managerObject.AddComponent<EggManager>();

        _createdObjects.Add(_managerObject);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject createdObject in _createdObjects)
        {
            if (createdObject != null)
                Object.DestroyImmediate(createdObject);
        }

        _createdObjects.Clear();
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
    public void RegisterEgg_CalledMultipleTimes_IncreasesBothCounts()
    {
        _eggManager.RegisterEgg();
        _eggManager.RegisterEgg();
        _eggManager.RegisterEgg();

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(3));
        Assert.That(_eggManager.TotalEggCount, Is.EqualTo(3));
    }

    [Test]
    public void HandleEggStepped_DecreasesActiveEggCount()
    {
        _eggManager.RegisterEgg();
        _eggManager.RegisterEgg();

        _eggManager.HandleEggStepped(Vector3.zero);

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(1));
    }

    [Test]
    public void HandleEggStepped_DoesNotDecreaseTotalEggCount()
    {
        _eggManager.RegisterEgg();

        _eggManager.HandleEggStepped(Vector3.zero);

        Assert.That(_eggManager.TotalEggCount, Is.EqualTo(1));
    }

    [Test]
    public void HandleEggStepped_WhenNoActiveEggs_DoesNotDecreaseCountBelowZero()
    {
        _eggManager.HandleEggStepped(Vector3.zero);

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(0));
    }

    [Test]
    public void HandleEggStepped_ChickenWithinAngerRadius_BecomesAngry()
    {
        ChickenAI chicken = CreateChicken(Vector3.one);

        SetPrivateField(_eggManager, "_angerRadius", 10f);

        _eggManager.HandleEggStepped(Vector3.zero);

        Assert.That(
            chicken.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Angry)
        );
    }

    [Test]
    public void HandleEggStepped_ChickenOutsideAngerRadius_DoesNotBecomeAngry()
    {
        ChickenAI chicken = CreateChicken(new Vector3(20f, 0f, 0f));

        SetPrivateField(_eggManager, "_angerRadius", 10f);

        _eggManager.HandleEggStepped(Vector3.zero);

        Assert.That(
            chicken.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Idle)
        );
    }

    private ChickenAI CreateChicken(Vector3 position)
    {
        GameObject chickenObject = new GameObject("Chicken");
        chickenObject.transform.position = position;

        _createdObjects.Add(chickenObject);

        ChickenAI chicken = chickenObject.AddComponent<ChickenAI>();

        ChickenAttack chickenAttack =
            chickenObject.AddComponent<ChickenAttack>();

        SetPrivateField(chicken, "_chickenAttack", chickenAttack);

        return chicken;
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(field, Is.Not.Null, $"Field {fieldName} was not found.");

        field.SetValue(target, value);
    }
}
