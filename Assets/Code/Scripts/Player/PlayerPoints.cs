using System;
using System.IO;
using System.Text;
using UnityEngine;

// this is a script for managing player points and should be added to the player prefab

public class PlayerPoints : MonoBehaviour
{
    public int CurrentPoints { get; private set; }
    
    public event Action<int> OnPointsChanged;
    
    private const int MinPoints = 0;
    private const int MaxPoints = 10000;

    private string _playerId;
    private bool _isInitialized;

    // returns the path to the save file for the player's points
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
    
    // initializes the player's points system with the given player ID
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

    // tries to add "pointsToAdd" to the player's current points, returns true if successful
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

    // tries to spend "pointsToSpend" from the player's current points, returns true if successful
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

    // saves the player's current points to the save file, returns true if successful
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

    // loads the player's current points from the save file
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
