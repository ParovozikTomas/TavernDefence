using UnityEngine;

namespace TavernDefense
{
    public class Tavern : MonoBehaviour
    {
        [Header("Tavern Properties")]
        public float health = 1000f;
        public float maxHealth = 1000f;
        public SpriteRenderer spriteRenderer;
        public Color healthyColor = Color.white;
        public Color damagedColor = Color.red;

        private GameManager gameManager;

        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                health = gameManager.tavernHealth;
                maxHealth = gameManager.maxTavernHealth;
            }
        }

        void Update()
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer != null)
            {
                float healthRatio = health / maxHealth;
                
                // Change color based on health percentage
                spriteRenderer.color = Color.Lerp(damagedColor, healthyColor, healthRatio);
                
                // Optional: Add pulsing effect when health is low
                if (healthRatio < 0.3f)
                {
                    // Add visual effect for low health
                    spriteRenderer.material.SetFloat("_FlashAmount", Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f);
                }
            }
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            health = Mathf.Max(0, health);

            // Update the game manager if it exists
            if (gameManager != null)
            {
                gameManager.tavernHealth = health;
            }

            // Check for game over condition
            if (health <= 0 && gameManager != null)
            {
                // Game over condition handled in GameManager
            }
        }

        public void Heal(float amount)
        {
            health += amount;
            health = Mathf.Min(health, maxHealth);

            if (gameManager != null)
            {
                gameManager.tavernHealth = health;
            }
        }

        public float GetHealthPercentage()
        {
            return health / maxHealth;
        }

        // Called by the GameManager to sync health
        public void SyncHealth(float currentHealth, float maxHp)
        {
            health = currentHealth;
            maxHealth = maxHp;
        }
    }
}