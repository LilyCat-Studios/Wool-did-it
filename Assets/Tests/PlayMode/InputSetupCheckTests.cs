using NUnit.Framework;
using UnityEngine.InputSystem;

public class InputSetupCheckTests : InputTestFixture
{
    [Test]
    public void SimulatedKeyPress_PressesKey()
    {
        Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
        Press(keyboard.spaceKey);
        Assert.That(keyboard.spaceKey.isPressed);
    }
}