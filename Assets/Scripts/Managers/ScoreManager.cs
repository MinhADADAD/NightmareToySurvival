using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    const string HighScoreKey = "NightmareToySurvival.HighScore";
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    void Awake() { Score = 0; HighScore = PlayerPrefs.GetInt(HighScoreKey, 0); }
    public void Add(int value) { Score += Mathf.Max(0, value); }
    public void Save()
    {
        if (Score <= HighScore) return;
        HighScore = Score; PlayerPrefs.SetInt(HighScoreKey, HighScore); PlayerPrefs.Save();
    }
}
