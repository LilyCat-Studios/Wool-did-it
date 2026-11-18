using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[Category("Clues")]
public class SelectImagesTests
{
    [TestCase(0, 3, 1)] // normal next index
    [TestCase(1, 3, 2)] // normal next index
    [TestCase(2, 3, 0)] // wrap around next index
    [TestCase(0, 1, 0)] // only one clue
    public void GetNextIndex_ReturnsNextOrWrapsAround(int current, int count, int expected)
    {
        Assert.That(SelectImages.GetNextIndex(current, count), Is.EqualTo(expected));
    }

    [TestCase(2, 3, 1)] // normal previous index
    [TestCase(1, 3, 0)] // normal previous index
    [TestCase(0, 3, 2)] // wrap around previous index
    [TestCase(0, 1, 0)] // only one clue
    public void GetPreviousIndex_ReturnsPreviousOrWrapsAround(int current, int count, int expected)
    {
        Assert.That(SelectImages.GetPrevIndex(current, count), Is.EqualTo(expected));
    }

    [Test]
    public  void GetRandomIndex_StaysWithinBounds()
    {
        Random.InitState(0); // Set seed for deterministic behavior

        for (int i = 0; i < 1000; i++)
        {
            Assert.That(SelectImages.GetRandomIndex(3), Is.GreaterThanOrEqualTo(0).And.LessThan(3));
        }
    }

    [Test]
    [Ignore("Known bug: Random.Range(0, count - 1) does not include the upper bound. Ignored for example purposes")]
    public void GetRandomIndex_CanSelectUpperBound()
    {
        Random.InitState(0); // Set seed for deterministic behavior
        HashSet<int> selected = new HashSet<int>();

        for (int i = 0; i < 1000; i++)
        {
            selected.Add(SelectImages.GetRandomIndex(3));
        }

        Assert.That(selected, Does.Contain(2));
    }
}