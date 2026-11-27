using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
        Object[] eggs = Object.FindObjectsOfType<Egg>();

        foreach (Object egg in eggs)
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
        int eggCountBefore = Object.FindObjectsOfType<Egg>().Length;

        yield return new WaitForSeconds(0.15f);

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(1));

        int eggCountAfter = Object.FindObjectsOfType<Egg>().Length;

        Assert.That(eggCountAfter, Is.EqualTo(eggCountBefore + 1));
    }

    [UnityTest]
    public IEnumerator Update_WhenSpawnIntervalIsZero_DoesNotSpawnEgg()
    {
        SetPrivateField(_spawner, "_eggSpawnInterval", 0f);

        yield return new WaitForSeconds(0.2f);

        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(0));
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic
        );

        Assert.That(field, Is.Not.Null);

        field.SetValue(target, value);
    }
}