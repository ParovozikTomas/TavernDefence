using UnityEngine;

namespace TavernDefense
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Data")]
        public Enemy enemyData;
        public SpriteRenderer spriteRenderer;
        public Animator animator;
        public GameObject healthBar;

        [Header("Visual Settings")]
        public Color enemyColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Reddish color for enemies

        private GameManager gameManager;

        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            InitializeEnemy();
        }

        void Update()
        {
            UpdatePosition();
            UpdateVisuals();
        }

        private void InitializeEnemy()
        {
            if (enemyData == null) return;

            // Set the visual appearance for the enemy
            if (spriteRenderer != null)
            {
                spriteRenderer.color = enemyColor;
            }

            // Update health bar if it exists
            if (healthBar != null)
            {
                UpdateHealthBar();
            }
        }

        private void UpdatePosition()
        {
            if (gameManager != null && enemyData != null)
            {
                // Find the current enemy data in the game manager
                var currentEnemy = gameManager.enemies.Find(e => e.id == enemyData.id);
                if (currentEnemy != null)
                {
                    // Update our local reference
                    enemyData = currentEnemy;
                    
                    // Update position smoothly
                    transform.position = new Vector3(enemyData.position.x / 100f * Screen.width, 
                                                   enemyData.position.y / 100f * Screen.height, 0);
                }
            }
        }

        private void UpdateVisuals()
        {
            if (healthBar != null)
            {
                UpdateHealthBar();
            }

            if (animator != null)
            {
                // Set animation parameters based on enemy state
                animator.SetBool("IsDead", enemyData.health <= 0);
                animator.SetBool("IsAttacking", enemyData.damage > 0);
            }
        }

        private void UpdateHealthBar()
        {
            // Update health bar fill amount based on current health
            if (healthBar != null && healthBar.GetComponent<UnityEngine.UI.Slider>() != null)
            {
                var slider = healthBar.GetComponent<UnityEngine.UI.Slider>();
                slider.value = enemyData.health / enemyData.maxHealth;
            }
        }

        // Method called when enemy takes damage
        public void TakeDamage(float damage)
        {
            if (gameManager != null)
            {
                // The actual damage calculation happens in the GameManager
                // This method can be used for visual effects
                ShowDamageTaken();
            }
        }

        // Visual feedback methods
        public void ShowDamageTaken()
        {
            if (spriteRenderer != null)
            {
                // Flash white to indicate damage taken
                StartCoroutine(FlashWhite());
            }
        }

        private System.Collections.IEnumerator FlashWhite()
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }

        // Method called when enemy dies
        public void Die()
        {
            if (animator != null)
            {
                animator.SetBool("IsDead", true);
            }
            
            // Disable the collider to prevent further interactions
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            // Optionally destroy the object after a delay to show death animation
            Destroy(gameObject, 1.0f);
        }

        // Property accessors
        public Enemy EnemyData => enemyData;
    }
}