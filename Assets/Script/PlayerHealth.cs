using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float flashInterval = 0.15f;
    private bool isInvincible = false;

    [Header("Visual References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("UI HP Settings")]
    [SerializeField] private UnityEngine.UI.Image[] hpImages;
    [SerializeField] private Color lostHealthColor = Color.gray;
    [SerializeField] private Color fullHealthColor = Color.white;

    void Start()
    {
        currentHealth = maxHealth;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateHPUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        UpdateHPUI();
        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            GameOverManager.Instance.PlayerDied();
        }
        else
        {
            StartCoroutine(BecomeInvincible());
        }
    }

    private void UpdateHPUI()
    {
        if (hpImages == null) return;

        for (int i = 0; i < hpImages.Length; i++)
        {
            if (i < currentHealth)
            {
                hpImages[i].color = fullHealthColor;
            }
            else
            {
                hpImages[i].color = lostHealthColor;
            }
        }
    }

    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        
        // Efek visual: Berkedip Opacity
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;
        Color flashColor = originalColor;
        flashColor.a = 0.3f; 

        while (elapsed < invincibilityDuration)
        {
            // Toggle antara opacity normal dan transparan
            spriteRenderer.color = (spriteRenderer.color.a > 0.5f) ? flashColor : originalColor;
            
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        spriteRenderer.color = originalColor; 
        isInvincible = false;
    }
}
