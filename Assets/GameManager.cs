using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int currentEnergy;
    [SerializeField] private int energyThreshold = 3;
    private bool bossCalled = false;

    [SerializeField] private GameObject boss;
    [SerializeField] private GameObject enemySpawner;

    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject pauseMenu;

    void Start()
    {
        currentEnergy = 0;
        if (boss != null)
            boss.SetActive(false);
        MainMenu();
    }

    void Update()
    {
 
    }

    public void AddEnergy()
    {
        if (bossCalled)
        {
            return;
        }

        currentEnergy++;

        if (currentEnergy >= energyThreshold)
        {
            CallBoss();
        }
    }

    private void UpdateEnergyBar()
    {

    }

    private void CallBoss()
    {
        bossCalled = true;
        if (boss != null)
            boss.SetActive(true);
        if (enemySpawner != null)
            enemySpawner.SetActive(false);
    }

    public void MainMenu()
    {
        gameUI.SetActive(true);
        mainMenu.SetActive(true);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);

        Time.timeScale = 0f;
    }

    public void GameOverMenu()
    {
        gameUI.SetActive(false);
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        pauseMenu.SetActive(false);

        Time.timeScale = 0f;
    }

    public void PauseGameMenu()
    {
        gameUI.SetActive(true);
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(true);

        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        gameUI.SetActive(true);
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ResumeGame()
    {
        gameUI.SetActive(true);
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);

        Time.timeScale = 1f;
    }

    public int GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public int GetEnergyThreshold()
    {
        return energyThreshold;
    }

    public bool IsBossCalled()
    {
        return bossCalled;
    }
}