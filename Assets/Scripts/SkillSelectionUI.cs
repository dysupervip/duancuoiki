using UnityEngine;
using UnityEngine.UI;

public class SkillSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class SkillOption
    {
        public string skillName;   // "SpeedBoost", "Dash", "Pet"
        public Button button;
        public Text label;
    }

    [SerializeField] private SkillOption[] options;
    public bool HasChosen { get; private set; }

    private Player player;

    void Start() { player = FindAnyObjectByType<Player>(); }

    public void Show()
    {
        HasChosen = false;
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        foreach (var opt in options)
        {
            opt.button.onClick.RemoveAllListeners();
            opt.button.onClick.AddListener(() => OnChoose(opt.skillName));
            opt.button.interactable = true;
        }
    }

    void OnChoose(string skillName)
    {
        if (HasChosen) return;
        HasChosen = true;

        switch (skillName)
        {
            case "SpeedBoost": player.UnlockSpeedBoost(); break;
            case "Dash": player.UnlockDash(); break;
            case "Pet": player.UnlockPet(); break;
        }

        foreach (var opt in options) opt.button.interactable = false;
    }

    public void Hide() { gameObject.SetActive(false); }
}