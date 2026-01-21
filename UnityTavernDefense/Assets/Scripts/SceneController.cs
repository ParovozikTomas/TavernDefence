using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TavernDefense
{
    public class SceneController : MonoBehaviour
    {
        [Header("Game Managers")]
        public GameManager gameManager;
        public HeroSetupManager heroSetupManager;
        
        [Header("UI Panels")]
        public GameObject gamePanel;
        public GameObject setupPanel;
        public GameObject gameOverPanel;

        void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Ensure managers exist
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
                
            if (heroSetupManager == null)
                heroSetupManager = FindObjectOfType<HeroSetupManager>();

            // Set up event listeners
            if (heroSetupManager != null)
            {
                heroSetupManager.OnSetupComplete += StartGameWithHeroes;
            }

            // Show setup panel initially
            ShowSetupPanel();
        }

        private void ShowSetupPanel()
        {
            if (setupPanel != null) setupPanel.SetActive(true);
            if (gamePanel != null) gamePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void ShowGamePanel()
        {
            if (setupPanel != null) setupPanel.SetActive(false);
            if (gamePanel != null) gamePanel.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void ShowGameOverPanel()
        {
            if (setupPanel != null) setupPanel.SetActive(false);
            if (gamePanel != null) gamePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        private void StartGameWithHeroes(System.Collections.Generic.List<Hero> heroes)
        {
            if (gameManager != null)
            {
                gameManager.heroes = heroes;
                gameManager.StartWave(); // Start the first wave
            }
            
            ShowGamePanel();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            if (heroSetupManager != null)
            {
                heroSetupManager.OnSetupComplete -= StartGameWithHeroes;
            }
        }
    }
}