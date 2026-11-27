using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ChickenEggSpawnerTests
{
    private GameObject _spawnerObject;
    private GameObject _eggPrefab;
    private GameObject _spawnPointObject;
    private GameObject _managerObject;

    private ChickenEggSpawner _spawner;
    private EggManager _eggManager;

    [SetUp]
    public void SetUp()
    {
        _spawnerObject = new GameObject("ChickenEggSpawner");
        _eggPrefab = new GameObject("EggPrefab");
        _spawnPointObject = new GameObject("SpawnPoint");
        _managerObject = new GameObject("EggManager");

        _eggPrefab.AddComponent<Egg>();

        _eggManager = _managerObject.AddComponent<EggManager>();
        _spawner = _spawnerObject.AddComponent<ChickenEggSpawner>();

        SetPrivateField(_spawner, "_eggPrefab", _eggPrefab);
        SetPrivateField(_spawner, "_eggSpawnPoint", _spawnPointObject.transform);
        SetPrivateField(_spawner, "_eggManager", _eggManager);
    }

    [TearDown]
    public void TearDown()
    {
        Egg[] eggs = Object.FindObjectsByType<Egg>(
            FindObjectsSortMode.None
        );

        foreach (Egg egg in eggs)
        {
            if (egg.gameObject != _eggPrefab)
                Object.DestroyImmediate(egg.gameObject);
        }

        Object.DestroyImmediate(_spawnerObject);
        Object.DestroyImmediate(_eggPrefab);
        Object.DestroyImmediate(_spawnPointObject);
        Object.DestroyImmediate(_managerObject);
    }

    [Test]
    public void SpawnEgg_WithValidReferences_SpawnsEgg()
    {
        int eggCountBefore = Object.FindObjectsByType<Egg>(
            FindObjectsSortMode.None
        ).Length;

        InvokeSpawnEgg();

        int eggCountAfter = Object.FindObjectsByType<Egg>(
            FindObjectsSortMode.None
        ).Length;

        Assert.That(eggCountAfter, Is.EqualTo(eggCountBefore + 1));
    }

    [Test]
    public void SpawnEgg_WithValidReferences_RegistersEgg()
    {
        InvokeSpawnEgg();

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(1));
        Assert.That(_eggManager.TotalEggCount, Is.EqualTo(1));
    }

    [Test]
    public void SpawnEgg_WhenPrefabIsMissing_DoesNotSpawnEgg()
    {
        SetPrivateField(_spawner, "_eggPrefab", null);

        int eggCountBefore = Object.FindObjectsByType<Egg>(
            FindObjectsSortMode.None
        ).Length;

        LogAssert.Expect(
            LogType.Error,
            $"Missing reference on ChickenEggSpawner: {_spawnerObject.name}"
        );

        InvokeSpawnEgg();

        int eggCountAfter = Object.FindObjectsByType<Egg>(
            FindObjectsSortMode.None
        ).Length;

        Assert.That(eggCountAfter, Is.EqualTo(eggCountBefore));
        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(0));
    }

    private void InvokeSpawnEgg()
    {
        MethodInfo spawnEggMethod = typeof(ChickenEggSpawner).GetMethod(
            "SpawnEgg",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(spawnEggMethod, Is.Not.Null);

        spawnEggMethod.Invoke(_spawner, null);
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