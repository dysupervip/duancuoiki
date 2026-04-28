using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Phase
    {
        public bool isBossPhase;              // True nếu là phase boss
        public GameObject bossPrefab;         // Prefab boss
        public int totalEnemies = 10;         // Số quái cần tiêu diệt (phase thường)
        public float spawnDelay = 1.5f;        // Thời gian giữa các lần spawn
        public int oilDropQuota = 3;           // Lượng dầu tối đa có thể rơi
        // Không còn newWeaponPrefab
    }

    [SerializeField] private List<Phase> phases;                  // Danh sách phase (3 thường + 1 boss)
    [SerializeField] private EnemySpawner spawner;                // Spawner
    [SerializeField] private Player player;                       // Player
    [SerializeField] private SkillSelectionUI skillSelectionUI;   // UI chọn kỹ năng (mới)

    [Header("Mở khóa enemy theo phase")]
    [SerializeField] private List<GameObject> enemyLibrary;       // Các loại enemy mở dần

    private List<GameObject> unlockedEnemyPrefabs = new List<GameObject>(); // Enemy đã mở
    private int currentPhaseIndex = 0;   // Phase hiện tại
    private int enemiesKilled;           // Số quái đã chết trong phase
    private int enemiesSpawned;          // Số quái đã spawn
    private int oilRemaining;            // Dầu còn lại trong phase

    public static WaveManager Instance;

    void Awake() { Instance = this; }

    void Start()
    {
        StartCoroutine(RunPhase(currentPhaseIndex)); // Bắt đầu phase 0
    }

    IEnumerator RunPhase(int index)
    {
        if (index >= phases.Count) yield break; // Hết phase

        Phase phase = phases[index];
        // Reset thanh tiến trình
        if (ThanhTienTrinhUI.Instance != null)
        {
            ThanhTienTrinhUI.Instance.ResetBar();
            ThanhTienTrinhUI.Instance.SetColor(phase.isBossPhase ? new Color(0.6f, 0f, 1f) : Color.yellow);
        }
        oilRemaining = phase.oilDropQuota; // Đặt quota dầu

        if (!phase.isBossPhase)
        {
            // === PHASE THƯỜNG ===
            // Mở khóa enemy mới (nếu còn)
            if (currentPhaseIndex < enemyLibrary.Count)
            {
                GameObject newEnemy = enemyLibrary[currentPhaseIndex];
                if (!unlockedEnemyPrefabs.Contains(newEnemy))
                {
                    unlockedEnemyPrefabs.Add(newEnemy);
                    Debug.Log($"Mở khóa enemy mới: {newEnemy.name}");
                }
            }

            enemiesKilled = 0;
            enemiesSpawned = 0;

            // Vòng lặp: tiếp tục cho đến khi giết đủ quái
            while (enemiesKilled < phase.totalEnemies)
            {
                // Nếu chưa spawn đủ số quái cần
                if (enemiesSpawned < phase.totalEnemies)
                {
                    yield return new WaitForSeconds(phase.spawnDelay);
                    if (unlockedEnemyPrefabs.Count > 0)
                    {
                        // Chọn ngẫu nhiên một loại enemy đã mở
                        GameObject prefab = unlockedEnemyPrefabs[Random.Range(0, unlockedEnemyPrefabs.Count)];
                        spawner.SpawnSpecificEnemy(prefab);
                        enemiesSpawned++;
                    }
                }
                else
                {
                    // Đã spawn đủ, chỉ chờ tiêu diệt hết
                    if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
                        break; // Không còn enemy, kết thúc phase
                    yield return null;
                }
            }

            // Sau phase thường -> hiện UI chọn kỹ năng
            yield return StartCoroutine(ShowSkillSelection());
        }
        else
        {
            // === PHASE BOSS ===
            yield return new WaitForSeconds(1f);
            GameObject boss = spawner.SpawnBoss(phase.bossPrefab);
            Enemy bossEnemy = boss.GetComponent<Enemy>();
            bossEnemy.HideHpBar(); // Ẩn thanh máu nhỏ của boss

            // Cập nhật thanh boss liên tục
            while (bossEnemy != null)
            {
                ThanhTienTrinhUI.Instance.UpdateBossHP(
                    bossEnemy.GetCurrentHP(),
                    bossEnemy.GetMaxHP() / 3f
                );
                yield return null;
            }
        }

        currentPhaseIndex++;
        StartCoroutine(RunPhase(currentPhaseIndex)); // Phase tiếp theo
    }

    IEnumerator ShowSkillSelection()
    {
        Time.timeScale = 0f;                // Tạm dừng game
        skillSelectionUI.Show();             // Hiện bảng
        yield return new WaitUntil(() => skillSelectionUI.HasChosen); // Chờ chọn
        skillSelectionUI.Hide();             // Ẩn
        Time.timeScale = 1f;                // Tiếp tục
    }

    public void HandleEnemyDeath()
    {
        enemiesKilled++;
        if (ThanhTienTrinhUI.Instance != null)
            ThanhTienTrinhUI.Instance.UpdateProgress(enemiesKilled, phases[currentPhaseIndex].totalEnemies);
    }

    public bool TryDropOil(Vector3 position)
    {
        if (oilRemaining > 0)
        {
            oilRemaining--;
            GameObject oilPrefab = Resources.Load<GameObject>("CrudeOilItem");
            if (oilPrefab != null) Instantiate(oilPrefab, position, Quaternion.identity);
            return true;
        }
        return false;
    }
}