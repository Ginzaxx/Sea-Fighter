using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Diperlukan untuk TextMeshPro
using System.Collections;
using UnityEngine.Serialization;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Rules")]
    [SerializeField] private float survivalGoalTime = 60f;
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    
    [Header("Start UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Game Over Navigation")]
    [SerializeField] private RectTransform mainMenuBtnRect;
    [FormerlySerializedAs("playAgainBtnRect")]
    [SerializeField] private RectTransform continueBtnRect;
    [SerializeField] private Image mainMenuBtnImage;
    [FormerlySerializedAs("playAgainBtnImage")]
    [SerializeField] private Image continueBtnImage;
    [SerializeField] private TextMeshProUGUI continueBtnText;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 1.0f;
    [SerializeField] private Color pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private float loadDelay = 0.5f;

    [Header("Next Level Settings")]
    [SerializeField] private string nextSceneName;
#if UNITY_EDITOR
    [SerializeField] private UnityEditor.SceneAsset nextSceneAsset;

    private void OnValidate()
    {
        if (nextSceneAsset != null)
        {
            nextSceneName = nextSceneAsset.name;
        }
    }
#endif

    [Header("Score Settings")]
    [SerializeField] private TextMeshProUGUI scoreText;
    private float currentScore = 0f;

    [Header("Win Transition Settings")]
    [SerializeField] private Transform winSpriteTransform;
    [SerializeField] private float transitionDuration = 3f;
    [SerializeField] private float startY = 9f;
    [SerializeField] private float endY = 3f;

    [Header("Debug/Monitor")]
    [SerializeField] private float currentTimer = 0f;
    [SerializeField] private bool isGameFinished = false;
    [SerializeField] private bool isGameStarted = false;
    private bool isTransitioning = false;

    private int selectedGameOverIndex = 1; // 0: Main Menu, 1: Continue

    public static GameOverManager Instance { get; private set; }
    public bool IsGameFinished => isGameFinished;
    public bool IsGameStarted  => isGameStarted;
    public float SurvivalGoalTime => survivalGoalTime;
    public float CurrentTimer => currentTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (continueBtnText != null) continueBtnText.text = "Continue";
    }

    private void Start()
    {
        StartCoroutine(StartGameCountdown());
    }

    private IEnumerator StartGameCountdown()
    {
        isGameStarted = false;
        if (startPanel != null) startPanel.SetActive(true);

        if (countdownText != null)
        {
            countdownText.text = "3";
            yield return new WaitForSeconds(1f);
            countdownText.text = "2";
            yield return new WaitForSeconds(1f);
            countdownText.text = "1";
            yield return new WaitForSeconds(1f);
            countdownText.text = "START!";
            yield return new WaitForSeconds(0.5f);
        }

        if (startPanel != null) startPanel.SetActive(false);
        isGameStarted = true;
    }

    void Update()
    {
        if (isTransitioning) return;

        if (!isGameStarted || isGameFinished)
        {
            if (isGameFinished) HandleGameOverInput();
            return;
        }

        currentTimer += Time.deltaTime;
        
        // Update Score (1 poin tiap detik)
        currentScore += Time.deltaTime;
        if (scoreText != null)
        {
            scoreText.text = "" + Mathf.FloorToInt(currentScore).ToString();
        }

        if (currentTimer >= survivalGoalTime)
        {
            Win();
        }
    }

    private void HandleGameOverInput()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null) return;

        // Navigasi A (Kiri) dan D (Kanan)
        if (UnityEngine.InputSystem.Keyboard.current.aKey.wasPressedThisFrame)
        {
            selectedGameOverIndex = 0;
        }
        if (UnityEngine.InputSystem.Keyboard.current.dKey.wasPressedThisFrame)
        {
            selectedGameOverIndex = 1;
        }

        // Visual Feedback Scaling
        if (mainMenuBtnRect != null)
            mainMenuBtnRect.localScale = Vector3.Lerp(mainMenuBtnRect.localScale, Vector3.one * (selectedGameOverIndex == 0 ? selectedScale : normalScale), Time.deltaTime * 10f);
        if (continueBtnRect != null)
            continueBtnRect.localScale = Vector3.Lerp(continueBtnRect.localScale, Vector3.one * (selectedGameOverIndex == 1 ? selectedScale : normalScale), Time.deltaTime * 10f);

        // Konfirmasi
        if (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (selectedGameOverIndex == 0) StartCoroutine(LoadSceneRoutine(mainMenuSceneName, mainMenuBtnImage));
            else StartCoroutine(LoadSceneRoutine(nextSceneName, continueBtnImage));
        }
    }

    private IEnumerator LoadSceneRoutine(string sceneName, Image buttonImage)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty! Please assign a next scene in the inspector.");
            yield break;
        }

        isTransitioning = true;

        // Efek menggelap
        if (buttonImage != null) buttonImage.color = pressedColor;

        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(sceneName);
    }

    public void PlayerDied()
    {
        if (isGameFinished) return;
        isGameFinished = true;

        ShowGameOverUI("Kamu Kalah!");
    }

    private void Win()
    {
        isGameFinished = true;
        StartCoroutine(WinTransitionRoutine());
    }

    private IEnumerator WinTransitionRoutine()
    {
        float elapsed = 0f;
        
        if (winSpriteTransform != null)
        {
            Vector3 startPos = winSpriteTransform.position;
            startPos.y = startY;
            winSpriteTransform.position = startPos;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                
                Vector3 currentPos = winSpriteTransform.position;
                currentPos.y = Mathf.Lerp(startY, endY, t);
                winSpriteTransform.position = currentPos;
                
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(transitionDuration);
        }

        ShowGameOverUI("Kamu Menang!");
    }

    private void ShowGameOverUI(string message)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (resultText != null) resultText.text = message;
        
        StopAllAnimations();
        
        Debug.Log("Game Finished: " + message);
    }

    private void StopAllAnimations()
    {
        Animator[] allAnimators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (Animator anim in allAnimators)
        {
            anim.speed = 0;
        }
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0, survivalGoalTime - currentTimer);
    }
}
