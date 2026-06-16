using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class LeaderboardEntry
{
    public int score;
    public string date;

    public LeaderboardEntry(int score, string date)
    {
        this.score = score;
        this.date = date;
    }
}

[System.Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class LeaderboardManager : MonoBehaviour
{
    private const string LeaderboardKey = "LeaderboardData";
    private const int MaxEntries = 10;

    public static void SubmitScore(int score)
    {
        if (score <= 0) return;

        LeaderboardData data = LoadLeaderboard();
        string currentDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        data.entries.Add(new LeaderboardEntry(score, currentDate));

        // Sort descending
        data.entries.Sort((a, b) => b.score.CompareTo(a.score));

        // Keep top 10
        if (data.entries.Count > MaxEntries)
        {
            data.entries.RemoveRange(MaxEntries, data.entries.Count - MaxEntries);
        }

        SaveLeaderboard(data);
    }

    public static LeaderboardData LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString(LeaderboardKey, "");
        if (string.IsNullOrEmpty(json))
        {
            return new LeaderboardData();
        }

        try
        {
            return JsonUtility.FromJson<LeaderboardData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing leaderboard data: " + ex.Message);
            return new LeaderboardData();
        }
    }

    private static void SaveLeaderboard(LeaderboardData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LeaderboardKey, json);
        PlayerPrefs.Save();
    }
}
