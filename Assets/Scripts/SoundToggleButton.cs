using UnityEngine;
using UnityEngine.UI;

public class SoundToggleButton : MonoBehaviour
{
    [SerializeField] private Image buttonImage;

    [Header("Sprites")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    private bool isMuted = false;

    void Start()
    {
        UpdateIcon();
    }

    public void ToggleSound()
    {
        isMuted = !isMuted;

        AudioListener.volume = isMuted ? 0f : 1f;

        UpdateIcon();
    }

    void UpdateIcon()
    {
        buttonImage.sprite = isMuted
            ? soundOffSprite
            : soundOnSprite;
    }
}