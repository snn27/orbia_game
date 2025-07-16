using UnityEngine;

public class Planet : MonoBehaviour
{
    // Sadece gorsel ayarlari kaldi.
    [Header("Visual Settings")]
    [SerializeField] private Color defaultColor = Color.green;
    [SerializeField] private Color activeColor = Color.red;

    private SpriteRenderer spriteRenderer;
    private bool hasBeenActivated = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor; // Her gezegen yesil baslar.
        }
    }

    // Bu fonksiyon artik SADECE rengi degistirir.
    public void Activate()
    {
        if (hasBeenActivated) return;

        hasBeenActivated = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = activeColor;
        }
    }
}