using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Phase
    {
        public bool isBossPhase;
        public GameObject bossPrefab;
        public int totalEnemies = 10;               // Tổng số enemy sẽ spawn trong phase này
        public float spawnDelay = 1.5f;
        public int oilDropQuota = 3;
        public bool spawnMiniBoss;
        public GameObject miniBossPrefab;

        // Danh sách các prefab enemy được phép spawn trong phase này
        public List<GameObject> enemyPrefabs;
    }

    [SerializeField] private List<Phase> phases;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private Player player;
    [SerializeField] private SkillSelectionUI skillSelectionUI;

    private int currentPhaseIndex = 0;
    private int enemiesKilled;
    private int enemiesSpawned;
    private int oilRemaining;

    public static WaveManager Instance;

    void Awake() { Instance = this; }

    void Start()
    {
        if (phases.Count == 0)
        {
            Debug.LogError("[WaveManager] Không có phase nào được thiết lập!");
            return;
        }
        StartCoroutine(RunPhase(0));
    }

    IEnumerator RunPhase(int phaseIndex)
    {
        if (phaseIndex >= phases.Count)
        {
            Debug.Log("[WaveManager] Hoàn thành tất cả phase.");
            yield break;
        }

        Phase phase = phases[phaseIndex];
        oilRemaining = phase.oilDropQuota;

        Debug.Log($"[WaveManager] ===== BẮT ĐẦU PHASE {phaseIndex + 1} =====");

        if (ThanhTienTrinhUI.Instance != null)
        {
            ThanhTienTrinhUI.Instance.ResetBar();
            ThanhTienTrinhUI.Instance.SetColor(phase.isBossPhase ? new Color(0.6f, 0f, 1f) : Color.yellow);
        }

        if (!phase.isBossPhase)
        {
            // Kiểm tra danh sách enemy của phase
            if (phase.enemyPrefabs == null || phase.enemyPrefabs.Count == 0)
            {
                Debug.LogError($"[WaveManager] Phase {phaseIndex + 1}: Danh sách enemy trống! Hãy thêm ít nhất 1 prefab.");
                yield break;
            }

            // Lọc bỏ phần tử null (nếu có) để tránh lỗi
            List<GameObject> validEnemies = phase.enemyPrefabs.FindAll(e => e != null);
            if (validEnemies.Count == 0)
            {
                Debug.LogError($"[WaveManager] Phase {phaseIndex + 1}: Tất cả prefab trong danh sách đều bị null!");
                yield break;
            }

            enemiesKilled = 0;
            enemiesSpawned = 0;

            // Spawn MiniBoss nếu có
            if (phase.spawnMiniBoss && phase.miniBossPrefab != null)
            {
                spawner.SpawnSpecificEnemy(phase.miniBossPrefab);
                enemiesSpawned++;
                Debug.Log($"[WaveManager] Phase {phaseIndex + 1}: Đã spawn MiniBoss.");
            }

            // Vòng lặp spawn enemy
            while (enemiesKilled < phase.totalEnemies)
            {
                if (enemiesSpawned < phase.totalEnemies)
                {
                    yield return new WaitForSeconds(phase.spawnDelay);

                    // Chọn ngẫu nhiên một prefab từ danh sách của phase
                    GameObject prefab = validEnemies[Random.Range(0, validEnemies.Count)];
                    spawner.SpawnSpecificEnemy(prefab);
                    enemiesSpawned++;

                    Debug.Log($"[WaveManager] Phase {phaseIndex + 1}: Spawn {prefab.name} ({enemiesSpawned}/{phase.totalEnemies})");
                }
                else
                {
                    // Đã spawn đủ số lượng, kiểm tra nếu không còn enemy thì kết thúc sớm
                    if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
                    {
                        Debug.LogWarning($"[WaveManager] Phase {phaseIndex + 1}: Không còn enemy, kết thúc sớm.");
                        break;
                    }
                    yield return null;
                }
            }

            // Chọn kỹ năng nếu còn
            if (skillSelectionUI != null && skillSelectionUI.HasAvailableSkills())
            {
                yield return StartCoroutine(ShowSkillSelection());
            }
            else
            {
                Debug.Log("[WaveManager] Tất cả kỹ năng đã mở khóa hoặc không có UI.");
            }
        }
        else
        {
            // Phase Boss
            yield return new WaitForSeconds(1f);
            GameObject boss = spawner.SpawnBoss(phase.bossPrefab);
            if (boss != null)
            {
                Enemy bossEnemy = boss.GetComponent<Enemy>();
                if (bossEnemy != null)
                {
                    bossEnemy.HideHpBar();
                    while (bossEnemy != null)
                    {
                        ThanhTienTrinhUI.Instance?.UpdateBossHP(bossEnemy.GetCurrentHP(), bossEnemy.GetMaxHP());
                        yield return null;
                    }
                }
            }
        }

        Debug.Log($"[WaveManager] ===== KẾT THÚC PHASE {phaseIndex + 1} =====");
        currentPhaseIndex = phaseIndex + 1;
        StartCoroutine(RunPhase(currentPhaseIndex));
    }

    IEnumerator ShowSkillSelection()
    {
        Time.timeScale = 0f;
        skillSelectionUI.Show();
        yield return new WaitUntil(() => skillSelectionUI.HasChosen);
        skillSelectionUI.Hide();
        Time.timeScale = 1f;
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