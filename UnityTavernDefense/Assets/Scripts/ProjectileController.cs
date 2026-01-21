using UnityEngine;

namespace TavernDefense
{
    public class ProjectileController : MonoBehaviour
    {
        [Header("Projectile Data")]
        public Projectile projectileData;
        public SpriteRenderer spriteRenderer;
        public float moveSpeed = 3f;

        private GameManager gameManager;
        private Transform targetTransform; // The target enemy transform
        private string targetId;

        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            InitializeProjectile();
        }

        void Update()
        {
            MoveTowardsTarget();
        }

        private void InitializeProjectile()
        {
            if (projectileData == null) return;

            // Set the visual appearance based on the projectile's visual property
            if (spriteRenderer != null)
            {
                // In a real Unity project, we might set the sprite based on the visual property
                // For now, we'll just log it
                Debug.Log($"Initializing projectile with visual: {projectileData.visual}");
            }

            // Find the target enemy
            if (gameManager != null)
            {
                var targetEnemy = gameManager.enemies.Find(e => e.id == projectileData.targetId);
                if (targetEnemy != null)
                {
                    targetId = targetEnemy.id;
                    // In a real scenario, we'd find the corresponding EnemyController
                    // For now, we'll just store the target ID
                }
            }
        }

        private void MoveTowardsTarget()
        {
            if (gameManager == null) return;

            // Find the target enemy in the game manager
            var targetEnemy = gameManager.enemies.Find(e => e.id == projectileData.targetId);
            
            if (targetEnemy != null)
            {
                // Calculate direction to target
                Vector2 targetPosition = new Vector2(
                    (targetEnemy.position.x / 100f) * Screen.width,
                    (targetEnemy.position.y / 100f) * Screen.height
                );

                // Move towards target
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                transform.position += (Vector3)(direction * projectileData.speed * Time.deltaTime);

                // Check if we've reached the target (within a small threshold)
                float distance = Vector2.Distance(transform.position, targetPosition);
                if (distance < 10f) // Close enough to consider a hit
                {
                    HitTarget();
                }
            }
            else
            {
                // Target is gone, destroy projectile
                DestroyProjectile();
            }
        }

        private void HitTarget()
        {
            if (gameManager != null)
            {
                // Apply damage to the target through the game manager
                // The actual damage application is handled in the GameManager
                Debug.Log($"Projectile hit target {projectileData.targetId} for {projectileData.damage} damage");
                
                // Remove this projectile from the game manager's list
                gameManager.projectiles.RemoveAll(p => p.id == projectileData.id);
                
                // Destroy this game object
                Destroy(gameObject);
            }
        }

        private void DestroyProjectile()
        {
            if (gameManager != null)
            {
                // Remove this projectile from the game manager's list
                gameManager.projectiles.RemoveAll(p => p.id == projectileData.id);
            }
            
            // Destroy this game object
            Destroy(gameObject);
        }

        // Method to initialize projectile with data
        public void SetProjectileData(Projectile projData)
        {
            projectileData = projData;
            
            // Set initial position based on the projectile data
            transform.position = new Vector3(
                (projectileData.position.x / 100f) * Screen.width,
                (projectileData.position.y / 100f) * Screen.height,
                0
            );
        }
    }
}