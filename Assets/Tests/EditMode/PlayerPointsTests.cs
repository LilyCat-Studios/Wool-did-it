using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;

[Category("PointSystem")]
public class PlayerPointsTests
{
    private GameObject _playerObject;
    private PlayerPoints _playerPoints;
    private string _playerId;
    
    [SetUp]
    public void SetUp()
    {
        _playerId = Guid.NewGuid().ToString();

        _playerObject = new GameObject("TestPlayer");
        _playerPoints = _playerObject.AddComponent<PlayerPoints>();

        _playerPoints.Initialize(_playerId);
    }
    
    [TearDown]
    public void TearDown()
    {
        if (_playerObject != null)
            UnityEngine.Object.DestroyImmediate(_playerObject);

        if (File.Exists(GetSavePath()))
            File.Delete(GetSavePath());
    }
    
    [Test]
    public void Initialize_NewPlayer_StartsWithZeroPoints()
    {
        Assert.That(_playerPoints.CurrentPoints, Is.EqualTo(0));
    }
    
    [Test]
    public void TryAddPoints_ValidAmount_AddsPoints()
    {
        bool result = _playerPoints.TryAddPoints(100);

        Assert.That(result, Is.True);
        Assert.That(_playerPoints.CurrentPoints, Is.EqualTo(100));
    }
    
    [Test]
    public void TryAddPoints_ExceedsMaximum_DoesNotAddPoints()
    {
        bool result = _playerPoints.TryAddPoints(100000);

        Assert.That(result, Is.False);
        Assert.That(_playerPoints.CurrentPoints, Is.EqualTo(0));
    }
    
    [Test]
    public void TryAddPoints_NegativeAmount_DoesNotAddPoints()
    {
        bool result = _playerPoints.TryAddPoints(-10);

        Assert.That(result, Is.False);
        Assert.That(_playerPoints.CurrentPoints, Is.EqualTo(0));
    }
    
    [Test]
    public void TrySpendPoints_ValidAmount_SubtractsPoints()
    {
        _playerPoints.TryAddPoints(100);

        bool result = _playerPoints.TrySpendPoints(40);

        Assert.That(result, Is.True);
        Assert.That(_playerPoints.CurrentPoints, Is.EqualTo(60));
    }
    
    [Test]
    public void TrySpendPoints_InsufficientPoints_DoesNotSubtractPoints()
    {
        _playerPoints.TryAddPoints(50);

        bool result = _playerPoints.TrySpendPoints(100);

        Assert.That(result, Is.False);
        Assert.That(_playerPoints.CurrentPoints, Is.EqualTo(50));
    }
    
    [Test]
    public void TrySpendPoints_ZeroAmount_DoesNotSubtractPoints()
    {
        _playerPoints.TryAddPoints(50);

        bool result = _playerPoints.TrySpendPoints(0);

        Assert.That(result, Is.False);
        Assert.That(_playerPoints.CurrentPoints, Is.EqualTo(50));
    }
    
    [Test]
    public void Initialize_ExistingPlayer_LoadsSavedPoints()
    {
        _playerPoints.TryAddPoints(100);

        UnityEngine.Object.DestroyImmediate(_playerObject);
        _playerObject = null;

        _playerObject = new GameObject("TestPlayer");
        _playerPoints = _playerObject.AddComponent<PlayerPoints>();

        _playerPoints.Initialize(_playerId);

        Assert.That(_playerPoints.CurrentPoints, Is.EqualTo(100));
    }
    
    private string GetSavePath()
    {
        string safeId = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(_playerId))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return Path.Combine(
            Application.persistentDataPath,
            "PlayerPoints_" + safeId + ".json");
    }
}
