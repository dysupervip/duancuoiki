using UnityEngine; // Bắt buộc để dùng các lớp của Unity (MonoBehaviour, GameObject...)
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    // Singleton: cho phép truy cập GameManager từ bất kỳ script nào bằng GameManager.Instance
    public static GameManager Instance;
    [Header("Stopwatch")]
    public float timeLimit;
    float stopwatchTime;
    public TextMeshProUGUI stopwatchDisplay;
    [SerializeField] private GameObject gameplayUI;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject pauseMenu;
    public static bool isFromMenu = true;
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
        if (isFromMenu)
        {
            MainMenu();
        }
        else
        {
            currentState = GameState.Gameplay;
            Time.timeScale = 1f;

            mainMenu.SetActive(false);
            pauseMenu.SetActive(false);
            resultPanel.SetActive(false);
            victoryPanel.SetActive(false);
        }
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
    public void ResumeGame()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        currentState = GameState.Gameplay;
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;

        resultPanel.SetActive(false);
        victoryPanel.SetActive(false);
        pauseMenu.SetActive(false);
        mainMenu.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void StartGame()
    {
        isFromMenu = false;
        RestartGame();
    }
}