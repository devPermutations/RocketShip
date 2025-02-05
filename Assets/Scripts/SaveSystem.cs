using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Data structure for saving game state
/// </summary>
[Serializable]
public class GameData
{
    public float gold;         // This will now store total gold
    public float metal;        // New field
    public float energy;       // New field
    public float baseSpeed;    // Saved rocket speed
    public float maxFuel;      // Saved fuel capacity
    public float turnSpeed;    // Saved turn speed
    public float maxHeight;    // Best height ever achieved
    public float maxHealth;    // Add health tracking

    public GameData(float _gold, float _metal, float _energy, RocketController _rocket)
    {
        gold = _gold;
        metal = _metal;
        energy = _energy;
        if (_rocket != null)
        {
            baseSpeed = _rocket.BaseSpeed;
            maxFuel = _rocket.MaxFuel;
            turnSpeed = _rocket.TurnSpeed;
            maxHeight = _rocket.MaxHeight;
            maxHealth = _rocket.MaxHealth;
        }
    }
}

/// <summary>
/// Handles saving and loading game data using PlayerPrefs
/// </summary>
public static class SaveSystem
{
    private const string c_SaveKey = "RocketGameSave";    // Key for PlayerPrefs storage
    private const string SAVE_FILE = "gamesave.json";

    /// <summary>
    /// Saves the current game state
    /// </summary>
    /// <param name="_gold">Current total gold amount</param>
    /// <param name="_metal">Current metal amount</param>
    /// <param name="_energy">Current energy amount</param>
    /// <param name="_rocket">Reference to the rocket to save stats from</param>
    public static void SaveGame(float _gold, float _metal, float _energy, RocketController _rocket)
    {
        GameData data = new GameData(_gold, _metal, _energy, _rocket);
        string json = JsonUtility.ToJson(data);
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Loads the saved game state
    /// </summary>
    /// <returns>Saved game data or null if no save exists</returns>
    public static GameData LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<GameData>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading save file: {e.Message}");
                return new GameData(0f, 0f, 0f, null); // Return new data with zeroed values
            }
        }
        return new GameData(0f, 0f, 0f, null); // Return new data if no save exists
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