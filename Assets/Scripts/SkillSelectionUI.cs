using UnityEngine;
using UnityEngine.UI;

public class SkillSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class SkillOption
    {
        public string skillName;   // "Pet", "EnergyBeam", "DualWield"
        public Button button;
        public Text label;
    }

    [SerializeField] private SkillOption[] options;
    public bool HasChosen { get; private set; }

    // Không dùng Start nữa, tìm Player mỗi lần cần

    /// <summary>
    /// Trả về true nếu còn ít nhất 1 kỹ năng chưa được mở khóa.
    /// </summary>
    public bool HasAvailableSkills()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player == null)
        {
            Debug.LogWarning("[SkillSelectionUI] HasAvailableSkills: Player is NULL!");
            return false; // Nếu không tìm thấy Player, coi như không có skill nào để chọn
        }
        bool available = !player.HasPet || !player.HasBeam || !player.HasDualWield;
        Debug.Log($"[SkillSelectionUI] HasAvailableSkills: HasPet={player.HasPet}, HasBeam={player.HasBeam}, HasDualWield={player.HasDualWield} => {available}");
        return available;
    }

    public void Show()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("[SkillSelectionUI] Show: Player is NULL! Không thể hiển thị.");
            gameObject.SetActive(false);
            return;
        }

        HasChosen = false;
        gameObject.SetActive(true);

        foreach (var opt in options)
        {
            bool alreadyUnlocked = false;
            switch (opt.skillName)
            {
                case "Pet": alreadyUnlocked = player.HasPet; break;
                case "EnergyBeam": alreadyUnlocked = player.HasBeam; break;
                case "DualWield": alreadyUnlocked = player.HasDualWield; break;
            }

            Debug.Log($"[SkillSelectionUI] Skill '{opt.skillName}' - đã mở khóa: {alreadyUnlocked}");

            if (alreadyUnlocked)
            {
                opt.button.gameObject.SetActive(false);
            }
            else
            {
                opt.button.gameObject.SetActive(true);
                string skillNameLocal = opt.skillName;
                opt.button.onClick.RemoveAllListeners();
                opt.button.onClick.AddListener(() => OnChoose(skillNameLocal));
                opt.button.interactable = true;
            }
        }
    }

    void OnChoose(string skillName)
    {
        Debug.Log($"[SkillSelectionUI] OnChoose: {skillName}");
        if (HasChosen) return;
        HasChosen = true;

        Player player = FindAnyObjectByType<Player>();
        if (player == null) return;

        switch (skillName)
        {
            case "Pet": player.UnlockPet(); break;
            case "EnergyBeam": player.UnlockBeam(); break;
            case "DualWield": player.UnlockDualWield(); break;
        }

        foreach (var opt in options)
            opt.button.interactable = false;

        gameObject.SetActive(false); // Tự ẩn sau khi chọn
    }

    public void Hide()
    {
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }
}