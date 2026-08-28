using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);
    private static readonly WaitForSeconds _waitForSeconds1 = new(1f);
    private static readonly WaitForSeconds _waitForSeconds5 = new(5f);

    [Header("Game Rules")]
    [SerializeField] private float survivalGoalTime = 60f;
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Start UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private TextMeshProUGUI startText;
    [SerializeField] private string startTextHere;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private string gameOverTextHere;

    [Header("Game Over Navigation")]
    [SerializeField] private RectTransform mainMenuBtnRect;
    [SerializeField] private RectTransform continueBtnRect;
    [SerializeField] private Image mainMenuBtnImage;
    [SerializeField] private Image continueBtnImage;
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button continueBtn;
    [SerializeField] private TextMeshProUGUI continueBtnText;
    [SerializeField] private TextMeshProUGUI mainMenuBtnText;
    [Tooltip("Multiplier applied to button's initial scale when selected or clicked")]
    [SerializeField] private float selectedScaleMultiplier = 1.15f;
    [SerializeField] private Color pressedColor = new(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private float loadDelay = 0.5f;

    private Vector3 mainMenuInitialScale = Vector3.one;
    private Vector3 continueInitialScale = Vector3.one;

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
    private bool isWin = false;

    private int selectedGameOverIndex = -1; // -1: None, 0: Main Menu, 1: Continue

    public static GameOverManager Instance { get; private set; }
    public bool IsGameFinished => isGameFinished;
    public bool IsGameStarted  => isGameStarted;
    public float SurvivalGoalTime => survivalGoalTime;
    public float CurrentTimer => currentTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainMenuBtnRect != null) mainMenuInitialScale = mainMenuBtnRect.localScale;
        if (continueBtnRect != null) continueInitialScale = continueBtnRect.localScale;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        SetGameOverButtonLabels(false);
    }

    private void Start()
    {
        if (mainMenuBtn == null && mainMenuBtnRect != null) mainMenuBtn = mainMenuBtnRect.GetComponent<Button>();
        if (continueBtn == null && continueBtnRect != null) continueBtn = continueBtnRect.GetComponent<Button>();
        if (mainMenuBtnText == null && mainMenuBtn != null) mainMenuBtnText = mainMenuBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (continueBtnText == null && continueBtn != null) continueBtnText = continueBtn.GetComponentInChildren<TextMeshProUGUI>();

        if (mainMenuBtn != null) mainMenuBtn.onClick.AddListener(OnMainMenuButtonClicked);
        if (continueBtn != null) continueBtn.onClick.AddListener(OnContinueButtonClicked);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayInGameBGM();
        }

        StartCoroutine(StartGameCountdown());
    }

    private IEnumerator StartGameCountdown()
    {
        isGameStarted = false;
        if (startPanel != null) startPanel.SetActive(true);

        if (startText != null)
        {
            startText.text = startTextHere;
            yield return _waitForSeconds5;
            startText.text = "3";
            yield return _waitForSeconds1;
            startText.text = "2";
            yield return _waitForSeconds1;
            startText.text = "1";
            yield return _waitForSeconds1;
            startText.text = "START!";
            yield return _waitForSeconds0_5;
        }

        if (startPanel != null) startPanel.SetActive(false);
        isGameStarted = true;
    }

    void Update()
    {
        if (isTransitioning) return;

        if (!isGameStarted || isGameFinished)
        {
            if (isGameFinished)
            {
                ApplyVisualFeedback();
                HandleGameOverInput();
            }
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

    private void ApplyVisualFeedback()
    {
        Vector3 mainMenuTargetScale = (selectedGameOverIndex == 0) ? (mainMenuInitialScale * selectedScaleMultiplier) : mainMenuInitialScale;
        Vector3 continueTargetScale = (selectedGameOverIndex == 1) ? (continueInitialScale * selectedScaleMultiplier) : continueInitialScale;

        if (mainMenuBtnRect != null)
            mainMenuBtnRect.localScale = Vector3.Lerp(mainMenuBtnRect.localScale, mainMenuTargetScale, Time.deltaTime * 10f);
        if (continueBtnRect != null)
            continueBtnRect.localScale = Vector3.Lerp(continueBtnRect.localScale, continueTargetScale, Time.deltaTime * 10f);
    }

    private void HandleGameOverInput()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null) return;

        if (UnityEngine.InputSystem.Keyboard.current.aKey.wasPressedThisFrame)
        {
            selectedGameOverIndex = 0;
        }
        if (UnityEngine.InputSystem.Keyboard.current.dKey.wasPressedThisFrame)
        {
            selectedGameOverIndex = 1;
        }

        if (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ActivateSelectedButton();
        }
    }

    private void OnMove()
    {
        if (selectedGameOverIndex == -1) selectedGameOverIndex = 1;
        else selectedGameOverIndex = (selectedGameOverIndex == 0) ? 1 : 0;
        Debug.Log("Selected: " + (selectedGameOverIndex == 0 ? "Main Menu" : "Continue"));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, Image buttonImage)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty! Please assign a next scene in the inspector.");
            yield break;
        }

        isTransitioning = true;

        if (buttonImage != null) buttonImage.color = pressedColor;

        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(sceneName);
    }

    public void PlayerDied()
    {
        if (isGameFinished) return;
        isGameFinished = true;
        isWin = false;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayDeadSFX();
        }

        ShowGameOverUI("Ship Crashed!");
    }

    private void Win()
    {
        isGameFinished = true;
        isWin = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWinSFX();
        }

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

        ShowGameOverUI(gameOverTextHere);
    }

    private void ShowGameOverUI(string message)
    {
        DisableDynamicTouchControllers();
        SetGameOverButtonLabels(isWin);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverText != null) gameOverText.text = message;
        
        StopAllAnimations();
        
        Debug.Log("Game Finished: " + message);
    }

    private void SetGameOverButtonLabels(bool win)
    {
        if (mainMenuBtnText != null) mainMenuBtnText.text = "Back to Main Menu";
        if (continueBtnText != null) continueBtnText.text = win ? "Continue" : "Retry";
    }

    private void DisableDynamicTouchControllers()
    {
        DynamicTouchController[] touchControllers = Object.FindObjectsByType<DynamicTouchController>(FindObjectsSortMode.None);
        foreach (DynamicTouchController touchController in touchControllers)
        {
            touchController.enabled = false;
        }
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

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove += OnMove;
            InputManager.Instance.OnConfirm += OnConfirm;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove -= OnMove;
            InputManager.Instance.OnConfirm -= OnConfirm;
        }
    }

    private void OnConfirm()
    {
        if (!isGameFinished || isTransitioning || selectedGameOverIndex == -1) return;

        ActivateSelectedButton();
    }

    private void ActivateSelectedButton()
    {
        if (selectedGameOverIndex == 0)
        {
            StartCoroutine(LoadSceneRoutine(mainMenuSceneName, mainMenuBtnImage));
        }
        else if (isWin)
        {
            StartCoroutine(LoadSceneRoutine(nextSceneName, continueBtnImage));
        }
        else
        {
            StartCoroutine(LoadSceneRoutine(SceneManager.GetActiveScene().name, continueBtnImage));
        }
    }

    public void OnMainMenuButtonClicked()
    {
        if (isTransitioning) return;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayTouchSFX();
        selectedGameOverIndex = 0;
        StartCoroutine(LoadSceneRoutine(mainMenuSceneName, mainMenuBtnImage));
    }

    public void OnContinueButtonClicked()
    {
        if (isTransitioning) return;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayTouchSFX();
        selectedGameOverIndex = 1;
        ActivateSelectedButton();
    }

    public void OnRetryButtonClicked()
    {
        if (isTransitioning || isWin) return;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayTouchSFX();
        selectedGameOverIndex = 1;
        ActivateSelectedButton();
    }

    public void SelectMainMenu()
    {
        if (!isTransitioning) selectedGameOverIndex = 0;
    }

    public void SelectContinue()
    {
        if (!isTransitioning) selectedGameOverIndex = 1;
    }
}
