using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

[Category("Sheep AI")]
public class SheepAITests
{
    private GameObject sheep;

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (sheep != null)
        {
            Object.Destroy(sheep);
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator Start_WithoutNavMeshAgent_LogsMissingComponentError()
    {
        sheep = new GameObject("Sheep");
        sheep.AddComponent<SheepAI>();
        
        LogAssert.Expect(LogType.Exception, new Regex("Missing NavMeshAgent"));

        // Expect an exception for missing NavMeshAgent component
        LogAssert.Expect(LogType.Exception, new Regex("no 'NavMeshAgent' attached"));

        yield return null;

        // Deactivate to stop constant exception logging
        sheep.SetActive(false);
    }
}