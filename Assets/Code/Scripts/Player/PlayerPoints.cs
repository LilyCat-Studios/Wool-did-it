
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class PlayerPoints : MonoBehaviour
{
    public int CurrentPoints { get; private set; }
    
    public event Action<int> OnPointsChanged;
    
    
    private const int MinPoints = 0;
    private const int MaxPoints = 9999;

    private string _playerId;
    private bool _isInitialized;

    private string SavePath
    {
        get
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

    [Serializable]
    private class PointSaveData
    {
        public int Points;
    }
    
    public void Initialize(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogError("Player ID cannot be empty.");
            return;
        }

        _playerId = id;
        _isInitialized = true;

        LoadPoints();

        OnPointsChanged?.Invoke(CurrentPoints);
    }

    public bool TryAddPoints(int pointsToAdd)
    {
        if (!_isInitialized || pointsToAdd <= 0)
            return false;

        if ((long)CurrentPoints + pointsToAdd > MaxPoints)
            return false;

        int previousPoints = CurrentPoints;
        CurrentPoints += pointsToAdd;

        if (!SavePoints())
        {
            CurrentPoints = previousPoints;
            return false;
        }

        OnPointsChanged?.Invoke(CurrentPoints);
        return true;
    }

    public bool TrySpendPoints(int pointsToSpend)
    {
        if (!_isInitialized || pointsToSpend <= 0)
            return false;

        if (CurrentPoints - (long)pointsToSpend < MinPoints)
            return false;

        int previousPoints = CurrentPoints;
        CurrentPoints -= pointsToSpend;

        if (!SavePoints())
        {
            CurrentPoints = previousPoints;
            return false;
        }

        OnPointsChanged?.Invoke(CurrentPoints);
        return true;
    }

    private bool SavePoints()
    {
        PointSaveData data = new PointSaveData
        {
            Points = CurrentPoints
        };

        string json = JsonUtility.ToJson(data);

        try
        {
            File.WriteAllText(SavePath, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save points: " + e.Message);
            return false;
        }
    }

    private void LoadPoints()
    {
        if (!File.Exists(SavePath))
        {
            CurrentPoints = MinPoints;
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            PointSaveData data = JsonUtility.FromJson<PointSaveData>(json);

            if (data == null)
            {
                CurrentPoints = MinPoints;
                return;
            }

            CurrentPoints = Mathf.Clamp(
                data.Points, MinPoints, MaxPoints);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load points: " + e.Message);
            CurrentPoints = MinPoints;
        }
    }
}
