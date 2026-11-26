using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

public class ChickenEggSpawnerPlayModeTests
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
        SetPrivateField(_spawner, "_eggSpawnInterval", 0.1f);
    }

    [TearDown]
    public void TearDown()
    {
        Object[] objects = Object.FindObjectsOfType<Egg>();

        foreach (Object egg in objects)
        {
            if (egg != null && egg != _eggPrefab)
                Object.Destroy(egg);
        }

        Object.Destroy(_spawnerObject);
        Object.Destroy(_eggPrefab);
        Object.Destroy(_spawnPointObject);
        Object.Destroy(_managerObject);
    }

    [UnityTest]
    public IEnumerator Update_WhenSpawnIntervalExpires_SpawnsEgg()
    {
        yield return new WaitForSeconds(0.15f);
        yield return null;

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(1));

        Egg[] eggs = Object.FindObjectsOfType<Egg>();

        Assert.That(eggs.Length, Is.EqualTo(2));
    }

    [UnityTest]
    public IEnumerator Update_WhenSpawnIntervalIsZero_DoesNotSpawnEgg()
    {
        SetPrivateField(_spawner, "_eggSpawnInterval", 0f);

        yield return new WaitForSeconds(0.2f);

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(0));
    }

    [Test]
    public void SpawnEgg_WhenPrefabIsMissing_DoesNotSpawnEgg()
    {
        // Arrange
        SetPrivateField(_spawner, "_eggPrefab", null);

        int eggCountBefore = Object.FindObjectsByType<Egg>(
            FindObjectsSortMode.None
        ).Length;

        MethodInfo spawnEggMethod = typeof(ChickenEggSpawner).GetMethod(
            "SpawnEgg",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(spawnEggMethod, Is.Not.Null);

        LogAssert.Expect(
            LogType.Error,
            $"Missing reference on ChickenEggSpawner: {_spawner.gameObject.name}"
        );

        // Act
        spawnEggMethod.Invoke(_spawner, null);

        // Assert
        int eggCountAfter = Object.FindObjectsByType<Egg>(
            FindObjectsSortMode.None
        ).Length;

        Assert.That(eggCountAfter, Is.EqualTo(eggCountBefore));
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
