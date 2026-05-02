using UnityEngine;
using UnityEngine.UI;

public class WeaponUpgradeUI : MonoBehaviour
{
    [Header("Hiển thị dầu")]
    [SerializeField] private Text oilText;               // Text hiển thị số dầu

    [Header("Nút nâng cấp")]
    [SerializeField] private Button damageButton;        // Nút nâng sát thương
    [SerializeField] private Button magazineButton;      // Nút nâng băng đạn
    [SerializeField] private Button reloadButton;        // Nút nâng tốc độ nạp

    [Header("Chi phí")]
    [SerializeField] private Text damageCostText;       // Text chi phí sát thương
    [SerializeField] private Text magazineCostText;     // Text chi phí băng đạn
    [SerializeField] private Text reloadCostText;       // Text chi phí nạp đạn

    [Header("Cấp độ")]
    [SerializeField] private Text damageLevelText;      // Text cấp sát thương
    [SerializeField] private Text magazineLevelText;    // Text cấp băng đạn
    [SerializeField] private Text reloadLevelText;      // Text cấp nạp đạn

    private Player player;

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        UpdateUI();
    }

    void Update()
    {
        // Chỉ cập nhật khi panel đang hiện để tiết kiệm hiệu năng
        if (gameObject.activeSelf)
            UpdateUI();
    }

    void UpdateUI()
    {
        if (player == null) return;

        // Hiển thị số dầu
        oilText.text = "Dầu: " + player.crudeOil;

        // === Sát thương ===
        int dmgCost = player.damageLevel + 1;                                 // Chi phí = cấp hiện tại + 1
        damageCostText.text = player.damageLevel < 3 ? "Cost: " + dmgCost : "MAX";
        damageLevelText.text = "Lv." + player.damageLevel + "/3";
        damageButton.interactable = (player.damageLevel < 3 && player.crudeOil >= dmgCost);

        // === Băng đạn ===
        int magCost = player.magazineLevel + 1;
        magazineCostText.text = player.magazineLevel < 3 ? "Cost: " + magCost : "MAX";
        magazineLevelText.text = "Lv." + player.magazineLevel + "/3";
        magazineButton.interactable = (player.magazineLevel < 3 && player.crudeOil >= magCost);

        // === Tốc độ nạp ===
        int relCost = player.reloadLevel + 1;
        reloadCostText.text = player.reloadLevel < 3 ? "Cost: " + relCost : "MAX";
        reloadLevelText.text = "Lv." + player.reloadLevel + "/3";
        reloadButton.interactable = (player.reloadLevel < 3 && player.crudeOil >= relCost);
    }

    // Các hàm gán cho sự kiện OnClick của Button
    public void UpgradeDamage()    { if (player.UpgradeDamage()) UpdateUI(); }
    public void UpgradeMagazine()  { if (player.UpgradeMagazine()) UpdateUI(); }
    public void UpgradeReload()    { if (player.UpgradeReloadSpeed()) UpdateUI(); }
}