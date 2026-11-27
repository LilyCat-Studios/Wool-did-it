using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EggPlayModeTests
{
    private GameObject _eggObject;
    private GameObject _playerObject;
    private GameObject _managerObject;

    private EggManager _eggManager;
    private Egg _egg;

    [SetUp]
    public void SetUp()
    {
        _managerObject = new GameObject("EggManager");
        _eggManager = _managerObject.AddComponent<EggManager>();

        _eggObject = new GameObject("Egg");
        _eggObject.transform.position = Vector3.zero;

        SphereCollider eggCollider = _eggObject.AddComponent<SphereCollider>();
        eggCollider.isTrigger = true;

        _egg = _eggObject.AddComponent<Egg>();
        _egg.Initialize(_eggManager);

        _playerObject = new GameObject("Player");
        _playerObject.tag = "Player";
        _playerObject.transform.position = new Vector3(5f, 0f, 0f);

        _playerObject.AddComponent<SphereCollider>();

        Rigidbody playerRigidbody = _playerObject.AddComponent<Rigidbody>();
        playerRigidbody.isKinematic = true;
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_eggObject);
        Object.Destroy(_playerObject);
        Object.Destroy(_managerObject);
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter_WhenPlayerTouchesEgg_DestroysEgg()
    {
        _eggManager.RegisterEgg();

        _playerObject.transform.position = Vector3.zero;
        Physics.SyncTransforms();

        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.That(_eggObject == null, Is.True);
        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(0));
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter_WhenOtherObjectTouchesEgg_DoesNotDestroyEgg()
    {
        _playerObject.tag = "Untagged";

        _playerObject.transform.position = Vector3.zero;
        Physics.SyncTransforms();

        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.That(_eggObject != null, Is.True);
        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(0));
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter_WhenManagerIsMissing_DoesNotDestroyEgg()
    {
        _egg.Initialize(null);
        _eggManager.RegisterEgg();

        LogAssert.Expect(
            LogType.Error,
            "EggManager reference is missing!"
        );

        _playerObject.transform.position = Vector3.zero;
        Physics.SyncTransforms();

        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.That(_eggObject != null, Is.True);
        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter_WhenOtherObjectTouchesEgg_DoesNotChangeEggCount()
    {
        _eggManager.RegisterEgg();

        _playerObject.tag = "Untagged";
        _playerObject.transform.position = Vector3.zero;
        Physics.SyncTransforms();

        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.That(_eggObject != null, Is.True);
        Assert.That(_eggManager.ActiveEggCount, Is.EqualTo(1));
    }
}