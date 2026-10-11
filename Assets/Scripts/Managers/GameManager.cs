using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Playing, Paused, Won, Lost }

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsPlaying { get { return Instance && Instance.State == GameState.Playing; } }
    public PlayerHealth player;
    public EnemyManager waves;
    public ScoreManager score;
    public GameOverManager gameOver;
    public GameState State { get; private set; }
    static bool playOnLoad;
    void Awake()
    {
        Instance = this; Time.timeScale = 1; State = GameState.Menu;
        int playerLayer = LayerMask.NameToLayer("NightmarePlayer"), enemyLayer = LayerMask.NameToLayer("NightmareEnemy");
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        Physics.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
    }
    void Start() { if (playOnLoad) { playOnLoad = false; Play(); } }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }
    }
    public void Play()
    {
        if (State != GameState.Menu) return;
        State = GameState.Playing; Time.timeScale = 1; waves.Begin();
    }
    public void Pause() { if (!IsPlaying) return; State = GameState.Paused; Time.timeScale = 0; }
    public void Resume() { if (State != GameState.Paused) return; State = GameState.Playing; Time.timeScale = 1; }
    public void Finish(bool won)
    {
        if (!IsPlaying) return;
        State = won ? GameState.Won : GameState.Lost;
        // Keep death animation running. All gameplay components check IsPlaying.
        Time.timeScale = 1; waves.StopAllCoroutines(); score.Save(); gameOver.ShowResult();
    }
    public void Restart() { Reload(true); }
    public void Menu() { Reload(false); }
    void Reload(bool start)
    {
        score.Save(); playOnLoad = start; Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Quit()
    {
        score.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    void OnApplicationFocus(bool focus) { if (!focus && IsPlaying) Pause(); }
    void OnApplicationQuit() { if (score) score.Save(); }
    void OnDestroy() { if (Instance == this) { Instance = null; Time.timeScale = 1; } }
}
