using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TavernDefense
{
    public class GameUIController : MonoBehaviour
    {
        // UI Elements
        [Header("Game Info")]
        public TextMeshProUGUI dayText;
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI tavernHealthText;
        public Slider tavernHealthSlider;
        public TextMeshProUGUI totalKillsText;
        public Button speedUpButton;
        public Button speedDownButton;
        public TextMeshProUGUI speedText;

        [Header("Game Manager Reference")]
        public GameManager gameManager;

        void Start()
        {
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
                
            if (gameManager != null)
            {
                gameManager.OnWaveComplete += OnWaveComplete;
                gameManager.OnGameOver += OnGameOver;
            }
            
            UpdateUI();
        }

        void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (gameManager != null)
            {
                // Update basic game info
                if (dayText != null)
                    dayText.text = $"Day {gameManager.day}";

                if (goldText != null)
                    goldText.text = $"{gameManager.gold} Gold";

                if (tavernHealthText != null)
                    tavernHealthText.text = $"Tavern: {Mathf.RoundToInt(gameManager.tavernHealth)}/{Mathf.RoundToInt(gameManager.maxTavernHealth)}";

                if (tavernHealthSlider != null)
                {
                    tavernHealthSlider.minValue = 0;
                    tavernHealthSlider.maxValue = gameManager.maxTavernHealth;
                    tavernHealthSlider.value = gameManager.tavernHealth;
                }

                if (totalKillsText != null)
                    totalKillsText.text = $"Total Kills: {gameManager.totalKills + gameManager.killsInWave}";

                if (speedText != null)
                    speedText.text = $"Speed: {gameManager.gameSpeed}x";
            }
        }

        public void SpeedUp()
        {
            if (gameManager != null)
            {
                gameManager.gameSpeed = Mathf.Min(4f, gameManager.gameSpeed + 0.5f);
            }
        }

        public void SpeedDown()
        {
            if (gameManager != null)
            {
                gameManager.gameSpeed = Mathf.Max(0.5f, gameManager.gameSpeed - 0.5f);
            }
        }

        private void OnWaveComplete(int goldEarned, System.Collections.Generic.List<Hero> heroes, int killsInWave)
        {
            if (gameManager != null)
            {
                gameManager.gold += goldEarned;
                gameManager.totalKills += killsInWave;
                gameManager.day++;
            }
        }

        private void OnGameOver(System.Collections.Generic.List<Hero> finalHeroes)
        {
            Debug.Log("Game Over!");
            // Handle game over logic here
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnWaveComplete -= OnWaveComplete;
                gameManager.OnGameOver -= OnGameOver;
            }
        }
    }
}