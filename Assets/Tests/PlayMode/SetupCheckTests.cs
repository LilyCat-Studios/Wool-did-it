using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class SetupCheckTests
{
    [UnityTest]
    public IEnumerator PlayMode_TestSetup()
    {
        yield return null;
        Assert.That(true, Is.True);
    }
}