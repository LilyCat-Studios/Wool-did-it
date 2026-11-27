using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

public class ChickenAIPlayModeTests
{
    private GameObject _chickenObject;
    private GameObject _roamingCenterObject;

    private ChickenAI _chickenAI;
    private NavMeshAgent _agent;

    [SetUp]
    public void SetUp()
    {
        _chickenObject = new GameObject("Test Chicken");
        _roamingCenterObject = new GameObject("Roaming Center");

        _agent = _chickenObject.AddComponent<NavMeshAgent>();
        _chickenAI = _chickenObject.AddComponent<ChickenAI>();

        SetPrivateField(_chickenAI, "_agent", _agent);
        SetPrivateField(_chickenAI, "_roamingCenter", _roamingCenterObject.transform);
        SetPrivateField(_chickenAI, "_roamingStartDelay", 0.1f);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_chickenObject);
        Object.Destroy(_roamingCenterObject);
    }

    [UnityTest]
    public IEnumerator Update_AfterRoamingStartDelay_ChangesStateToRoaming()
    {
        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Idle)
        );

        yield return new WaitForSeconds(0.2f);

        Assert.That(
            _chickenAI.CurrentState,
            Is.EqualTo(ChickenAI.ChickenState.Roaming)
        );
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