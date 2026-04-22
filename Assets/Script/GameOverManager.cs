using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Diperlukan untuk TextMeshPro
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Rules")]
    [SerializeField] private float survivalGoalTime = 60f;
    [SerializeField] private float reloadDelay = 5f;
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultText; 

    [Header("Debug/Monitor")]
    [SerializeField] private float currentTimer = 0f;
    [SerializeField] private bool isGameFinished = false;

    public static GameOverManager Instance { get; private set; }
    public bool IsGameFinished => isGameFinished; // Properti untuk diakses spawner

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (isGameFinished) return;

        currentTimer += Time.deltaTime;

        if (currentTimer >= survivalGoalTime)
        {
            Win();
        }
    }

    public void PlayerDied()
    {
        if (isGameFinished) return;
        isGameFinished = true;

        ShowGameOverUI("Kamu Kalah!");
        StartCoroutine(LoadSceneRoutine(mainMenuSceneName));
    }

    private void Win()
    {
        isGameFinished = true;
        
        ShowGameOverUI("Kamu Menang!");
        StartCoroutine(LoadSceneRoutine(mainMenuSceneName));
    }

    private void ShowGameOverUI(string message)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (resultText != null) resultText.text = message;
        
        Debug.Log("Game Finished: " + message);
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return new WaitForSeconds(reloadDelay);
        SceneManager.LoadScene(sceneName);
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0, survivalGoalTime - currentTimer);
    }
}
