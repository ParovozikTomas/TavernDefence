using UnityEngine;

namespace TavernDefense
{
    public class HeroController : MonoBehaviour
    {
        [Header("Hero Data")]
        public Hero heroData;
        public SpriteRenderer spriteRenderer;
        public Animator animator;
        public GameObject healthBar;

        [Header("Visual Settings")]
        public Color mageColor = Color.blue;
        public Color warriorColor = Color.gray;
        public Color archerColor = Color.green;

        private GameManager gameManager;
        private bool isBeingDragged = false;

        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            InitializeHero();
        }

        void Update()
        {
            UpdatePosition();
            UpdateVisuals();
        }

        private void InitializeHero()
        {
            if (heroData == null) return;

            // Set the visual appearance based on hero class
            if (spriteRenderer != null)
            {
                switch (heroData.heroClass)
                {
                    case HeroClass.Mage:
                        spriteRenderer.color = mageColor;
                        break;
                    case HeroClass.Warrior:
                        spriteRenderer.color = warriorColor;
                        break;
                    case HeroClass.Archer:
                        spriteRenderer.color = archerColor;
                        break;
                }
            }

            // Update health bar if it exists
            if (healthBar != null)
            {
                UpdateHealthBar();
            }
        }

        private void UpdatePosition()
        {
            if (gameManager != null && heroData != null)
            {
                // Find the current hero data in the game manager
                var currentHero = gameManager.heroes.Find(h => h.id == heroData.id);
                if (currentHero != null)
                {
                    // Update our local reference
                    heroData = currentHero;
                    
                    // Update position smoothly
                    transform.position = new Vector3(heroData.position.x / 100f * Screen.width, 
                                                   heroData.position.y / 100f * Screen.height, 0);
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
                // Set animation parameters based on hero state
                animator.SetBool("IsDead", heroData.isDead);
                animator.SetBool("IsAttacking", heroData.attackCooldown <= 0);
            }

            // Visual feedback when being dragged
            if (spriteRenderer != null)
            {
                if (isBeingDragged)
                {
                    spriteRenderer.transform.localScale = Vector3.one * 1.2f; // Slightly larger when dragged
                }
                else
                {
                    spriteRenderer.transform.localScale = Vector3.one;
                }
            }
        }

        private void UpdateHealthBar()
        {
            // Update health bar fill amount based on current health
            if (healthBar.GetComponent<Slider>() != null)
            {
                var slider = healthBar.GetComponent<Slider>();
                slider.value = heroData.health / heroData.maxHealth;
            }
        }

        // Called when player starts dragging this hero
        public void OnStartDrag()
        {
            if (gameManager != null && !heroData.isDead)
            {
                isBeingDragged = true;
                gameManager.StartDraggingHero(heroData.id);
            }
        }

        // Called when player stops dragging this hero
        public void OnEndDrag()
        {
            if (gameManager != null)
            {
                isBeingDragged = false;
                gameManager.FinishDraggingHero();
            }
        }

        // Called when player drags this hero to a new position
        public void OnDrag(Vector2 newPosition)
        {
            if (gameManager != null && isBeingDragged)
            {
                // Convert screen position to game coordinates (0-100%)
                Vector2 gamePosition = new Vector2(
                    (newPosition.x / Screen.width) * 100f,
                    (newPosition.y / Screen.height) * 100f
                );
                
                gameManager.UpdateHeroPosition(heroData.id, gamePosition);
            }
        }

        // Method to use hero's skill
        public void UseSkill()
        {
            if (gameManager != null && heroData != null && !heroData.isDead)
            {
                var activeSkill = new ActiveSkill
                {
                    id = heroData.skill.id,
                    name = heroData.skill.name,
                    description = heroData.skill.description,
                    icon = heroData.skill.icon,
                    cooldown = heroData.skill.cooldown,
                    areaOfEffect = heroData.skill.areaOfEffect,
                    damage = heroData.skill.damage,
                    healing = heroData.skill.healing,
                    duration = heroData.skill.duration,
                    maxTargets = heroData.skill.maxTargets,
                    buff = heroData.skill.buff,
                    casterId = heroData.id,
                    position = heroData.position,
                    timestamp = Time.time
                };
                
                gameManager.UseSkill(activeSkill);
            }
        }

        // Visual feedback methods
        public void ShowDamageTaken()
        {
            if (spriteRenderer != null)
            {
                // Flash red to indicate damage taken
                StartCoroutine(FlashRed());
            }
        }

        private System.Collections.IEnumerator FlashRed()
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            InitializeHero(); // Restore original color
        }

        // Property accessors
        public bool IsBeingDragged => isBeingDragged;
        public Hero HeroData => heroData;
    }
}