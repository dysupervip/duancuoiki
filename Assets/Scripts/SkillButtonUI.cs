using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
    [SerializeField] private Player player;

    [Header("Q - Pet")]
    [SerializeField] private Image qImage;
    [SerializeField] private Sprite qLockedSprite;
    [SerializeField] private Sprite qUnlockedSprite;

    [Header("E - Beam")]
    [SerializeField] private Image eImage;
    [SerializeField] private Sprite eLockedSprite;
    [SerializeField] private Sprite eUnlockedSprite;

    [Header("R - Dual")]
    [SerializeField] private Image rImage;
    [SerializeField] private Sprite rLockedSprite;
    [SerializeField] private Sprite rUnlockedSprite;

    void Update()
    {
        qImage.sprite = player.HasPet ? qUnlockedSprite : qLockedSprite;

        eImage.sprite = player.HasBeam ? eUnlockedSprite : eLockedSprite;

        rImage.sprite = player.HasDualWield ? rUnlockedSprite : rLockedSprite;
    }
}