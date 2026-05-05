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
    }

    void UpdateSkill(
        bool unlocked,
        GameObject lockObj,
        Image overlay,
        float remain,
        float maxCooldown)
    {
        if (lockObj != null)
            lockObj.SetActive(!unlocked);

        if (!unlocked)
        {
            overlay.gameObject.SetActive(false);
            return;
        }

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