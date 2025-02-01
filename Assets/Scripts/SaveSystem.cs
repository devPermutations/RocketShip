using UnityEngine;
using System;

/// <summary>
/// Data structure for saving game state
/// </summary>
[Serializable]
public class GameData
{
    public float gold;         // This will now store total gold
    public float baseSpeed;    // Saved rocket speed
    public float maxFuel;      // Saved fuel capacity
    public float turnSpeed;    // Saved turn speed
    public float maxHeight;    // Best height ever achieved
}

/// <summary>
/// Handles saving and loading game data using PlayerPrefs
/// </summary>
public static class SaveSystem
{
    private const string c_SaveKey = "RocketGameSave";    // Key for PlayerPrefs storage

    /// <summary>
    /// Saves the current game state
    /// </summary>
    /// <param name="_totalGold">Current total gold amount</param>
    /// <param name="_rocket">Reference to the rocket to save stats from</param>
    public static void SaveGame(float _totalGold, RocketController _rocket)
    {
        GameData data = new GameData
        {
            gold = _totalGold,
            maxHeight = _rocket.MaxHeight  // Save best height
        };

        if (_rocket != null)
        {
            data.maxFuel = _rocket.MaxFuel;
            data.baseSpeed = _rocket.BaseSpeed;
            data.turnSpeed = _rocket.TurnSpeed;
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(c_SaveKey, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the saved game state
    /// </summary>
    /// <returns>Saved game data or null if no save exists</returns>
    public static GameData LoadGame()
    {
        if (PlayerPrefs.HasKey(c_SaveKey))
        {
            string json = PlayerPrefs.GetString(c_SaveKey);
            return JsonUtility.FromJson<GameData>(json);
        }
        return null;
    }

    /// <summary>
    /// Clears all saved game data
    /// </summary>
    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(c_SaveKey);
        PlayerPrefs.Save();
    }
} 