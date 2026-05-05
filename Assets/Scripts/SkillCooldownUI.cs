using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    [SerializeField] private Player player;

    [Header("Q Skill - Pet")]
    [SerializeField] private Image qIcon;
    [SerializeField] private GameObject qLock;
    [SerializeField] private Image qCooldownOverlay;

    [Header("E Skill - Beam")]
    [SerializeField] private Image eIcon;
    [SerializeField] private GameObject eLock;
    [SerializeField] private Image eCooldownOverlay;

    [Header("R Skill - Dual Wield")]
    [SerializeField] private Image rIcon;
    [SerializeField] private GameObject rLock;
    [SerializeField] private Image rCooldownOverlay;

    [Header("SPACE Skill - Dash")]
    [SerializeField] private Image spaceCooldownOverlay;

    void Update()
    {
        if (player == null) return;

        // Q
        UpdateSkill(
            player.HasPet,
            qLock,
            qCooldownOverlay,
            player.GetPetCooldownRemaining(),
            player.GetPetCooldown()
        );

        // E
        UpdateSkill(
            player.HasBeam,
            eLock,
            eCooldownOverlay,
            player.GetBeamCooldownRemaining(),
            player.GetBeamCooldown()
        );

        // R
        UpdateSkill(
            player.HasDualWield,
            rLock,
            rCooldownOverlay,
            player.GetDualCooldownRemaining(),
            player.GetDualCooldown()
        );

        // SPACE (Dash luôn mở)
        float dashRemain = Mathf.Max(0, player.GetDashCooldownRemaining());
        float dashMax = player.GetDashCooldown();

        if (dashRemain > 0)
        {
            spaceCooldownOverlay.gameObject.SetActive(true);
            spaceCooldownOverlay.fillAmount = dashRemain / dashMax;
        }
        else
        {
            spaceCooldownOverlay.gameObject.SetActive(false);
        }
    }

    void UpdateSkill(
        bool unlocked,
        GameObject lockObj,
        Image overlay,
        float remain,
        float maxCooldown)
    {
        // LOCK
        if (lockObj != null)
            lockObj.SetActive(!unlocked);

        // Chưa mở skill
        if (!unlocked)
        {
            overlay.gameObject.SetActive(false);
            return;
        }

        // Đang cooldown
        if (remain > 0)
        {
            overlay.gameObject.SetActive(true);
            overlay.fillAmount = remain / maxCooldown;
        }
        else
        {
            overlay.gameObject.SetActive(false);
        }
    }
}