
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SheepControllerTests
{
    private GameObject _sheepObject;
    private SheepController _sheep;

    [SetUp]
    public void SetUp()
    {
        _sheepObject = new GameObject("Test Sheep");
        _sheep = _sheepObject.AddComponent<SheepController>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_sheepObject != null)
            Object.DestroyImmediate(_sheepObject);
    }

    #region Initial State

    [Test]
    public void Sheep_WhenCreated_HasNoCollectedClues()
    {
        Assert.That(_sheep.ClueIds, Is.Empty);
    }

    [Test]
    public void Sheep_WhenCreated_HasNoCarriedClue()
    {
        Assert.That(_sheep.Carried, Is.Null);
    }

    [Test]
    public void Sheep_WhenCreated_HasDefaultData()
    {
        Assert.That(_sheep.Data, Is.EqualTo(SheepData.Default));
    }

    #endregion

    #region Clue Queries

    [Test]
    public void HasClue_WhenClueDoesNotExist_ReturnsFalse()
    {
        Assert.That(_sheep.HasClue(10), Is.False);
    }

    [Test]
    public void ClueIds_WhenCreated_IsEmpty()
    {
        Assert.That(_sheep.ClueIds.Count, Is.EqualTo(0));
    }

    #endregion

    #region Customisation

    [Test]
    public void SetHat_WhenCalled_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _sheep.SetHat(1));
    }

    [Test]
    public void SetWoolShape_WhenCalled_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _sheep.SetWoolShape(1));
    }

    [Test]
    public void SetFootprint_WhenCalled_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _sheep.SetFootprint(1));
    }

    [Test]
    public void SetWoolColor_WhenCalled_DoesNotThrow()
    {
        Assert.DoesNotThrow(
            () => _sheep.SetWoolColor(Color.red));
    }

    #endregion

    #region Interaction Requests

    [Test]
    public void RequestPickupClue_WithNullClue_DoesNotThrow()
    {
        Assert.DoesNotThrow(
            () => _sheep.RequestPickupClue(null));
    }

    [Test]
    public void RequestDestroyClue_WithNullClue_DoesNotThrow()
    {
        Assert.DoesNotThrow(
            () => _sheep.RequestDestroyClue(null));
    }

    [Test]
    public void RequestCheckClue_WithNullClue_DoesNotThrow()
    {
        Assert.DoesNotThrow(
            () => _sheep.RequestCheckClue(null));
    }

    [Test]
    public void RequestDropClue_WhenNothingIsCarried_DoesNotThrow()
    {
        Assert.DoesNotThrow(
            () => _sheep.RequestDropClue());
    }

    #endregion
}
