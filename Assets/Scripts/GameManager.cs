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
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject pauseMenu;
     public enum GameState
    {
        Gameplay,
        Result,
        Victory,
        Pause
    };
    public GameState currentState;
    public TextMeshProUGUI timeSurvivedDisplay;

    [Header("Kết thúc game")]
    [SerializeField] private GameObject victoryPanel;
    [Header("Game Over")]
    [SerializeField] private GameObject resultPanel; // Kéo Panel chiến thắng vào đây trong Inspector

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
        currentState = GameState.Pause;
        MainMenu();
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
        if (currentState == GameState.Result) return;

        currentState = GameState.Result;

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        resultPanel.SetActive(true);
        Time.timeScale = 0f;
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

        if (newState == GameState.Result)
        {
            Time.timeScale = 0f;
        }

        if (newState == GameState.Victory)
        {
            if (victoryPanel != null)
                victoryPanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }
    public void MainMenu()
    {
        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        Time.timeScale = 0f;
    }
    public void PauseGameMenu()
    {
        pauseMenu.SetActive(true);
        mainMenu.SetActive(false);
        Time.timeScale = 0f;
    }
    public void StartGame()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        resultPanel.SetActive(false);

        stopwatchTime = 0f;

        Time.timeScale = 1f;
        currentState = GameState.Gameplay;
    }
    public void ResumeGame()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        currentState = GameState.Gameplay;
    }
}