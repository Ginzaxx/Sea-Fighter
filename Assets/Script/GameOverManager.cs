using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Rules")]
    [SerializeField] private float survivalGoalTime = 60f;
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    
    [Header("Start UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Game Over Navigation")]
    [SerializeField] private RectTransform mainMenuBtnRect;
    [SerializeField] private RectTransform playAgainBtnRect;
    [SerializeField] private Image mainMenuBtnImage;
    [SerializeField] private Image playAgainBtnImage;
    [SerializeField] private int selectedGameOverIndex = 1; // 0: Main Menu, 1: Play Again
    [SerializeField] private float selectionInput;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 1.0f;
    [SerializeField] private float loadDelay = 0.5f;
    [SerializeField] private Color pressedColor = new(0.5f, 0.5f, 0.5f, 1f);

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
    }

    void OnEnable()
    {
        InputManager.Instance.OnMove += OnMove;
        InputManager.Instance.OnConfirm += OnConfirm;
    }

    void OnDisable()
    {
        InputManager.Instance.OnMove -= OnMove;
        InputManager.Instance.OnConfirm -= OnConfirm;
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

    private void OnMove()
    {
        if (!isGameFinished) return;

        selectionInput = InputManager.Instance.MoveInput.x;
        
        if (selectionInput > 0) selectedGameOverIndex = 1;
        if (selectionInput < 0) selectedGameOverIndex = 0;
    }

    private void OnConfirm()
    {
        if (!isGameFinished) return;

        if (selectedGameOverIndex == 0) StartCoroutine(LoadSceneRoutine(mainMenuSceneName, mainMenuBtnImage));
        else StartCoroutine(LoadSceneRoutine(SceneManager.GetActiveScene().name, playAgainBtnImage));
    }

    private void HandleGameOverInput()
    {
        if (!isGameFinished) return;

        // Visual Feedback Scaling
        if (mainMenuBtnRect != null)
            mainMenuBtnRect.localScale = Vector3.Lerp(mainMenuBtnRect.localScale, Vector3.one * (selectedGameOverIndex == 0 ? selectedScale : normalScale), Time.deltaTime * 10f);
        if (playAgainBtnRect != null)
            playAgainBtnRect.localScale = Vector3.Lerp(playAgainBtnRect.localScale, Vector3.one * (selectedGameOverIndex == 1 ? selectedScale : normalScale), Time.deltaTime * 10f);
    }

    private IEnumerator LoadSceneRoutine(string sceneName, Image buttonImage)
    {
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
