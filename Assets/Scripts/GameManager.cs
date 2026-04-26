using UnityEngine; // Bắt buộc để dùng các lớp của Unity (MonoBehaviour, GameObject...)
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    // Singleton: cho phép truy cập GameManager từ bất kỳ script nào bằng GameManager.Instance
    public static GameManager Instance;
    [Header("Stopwatch")]
    public float timeLimit;
    float stopwatchTime;
    public TextMeshProUGUI stopwatchDisplay;
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject upgradePanel;
     public enum GameState
    {
        Gameplay,
        GameOver,
        Victory,
        Pause
    };
    public GameState currentState;
    public TextMeshProUGUI timeSurvivedDisplay;

    [Header("Kết thúc game")]
    [SerializeField] private GameObject victoryPanel;
    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel; // Kéo Panel chiến thắng vào đây trong Inspector

    void Update()
    {
        if (currentState == GameState.Gameplay)
        {
            UpdateStopwatch();
        }
    }
    void Awake()
    {
        // Đảm bảo chỉ có 1 GameManager tồn tại
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Nếu đã có thì hủy bản mới
    }
    void Start()
    {
        currentState = GameState.Gameplay;
        Time.timeScale = 1f;
    }

    // Hàm được gọi khi người chơi thắng (nhặt chìa khóa boss)
    public void WinGame()
    {
        Debug.Log("Người chơi thắng!");
        currentState = GameState.Victory;
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }
    public void GameOver()
    {
        if (currentState != GameState.Gameplay) return;
        currentState = GameState.GameOver;
        if (timeSurvivedDisplay != null && stopwatchDisplay != null)
        {
            timeSurvivedDisplay.text = stopwatchDisplay.text;
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;
        if (gameplayUI != null)
            gameplayUI.SetActive(false);
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
    void UpdateStopwatch()
    {
        stopwatchTime += Time.deltaTime;
        UpdateStopwatchUI();
        if (stopwatchTime >= timeLimit && currentState == GameState.Gameplay)
        {
            GameOver();
        }
    }
    void UpdateStopwatchUI()
    {
        int minutes = Mathf.FloorToInt(stopwatchTime / 60);
        int seconds = Mathf.FloorToInt(stopwatchTime % 60);

        stopwatchDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    void ChangeState(GameState newState)
    {
        currentState = newState;

        if (newState == GameState.GameOver)
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            Time.timeScale = 0f;
        }

        if (newState == GameState.Victory)
        {
            if (victoryPanel != null)
                victoryPanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }
}