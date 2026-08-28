using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Elements")]
    [SerializeField] private RectTransform playButtonRect;
    [SerializeField] private RectTransform exitButtonRect;
    [SerializeField] private Image playButtonImage;
    [SerializeField] private Image exitButtonImage;
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private float selectionInput;

    [Header("Feedback Settings")]
    [Tooltip("Multiplier applied to the button's original scale when selected or clicked (e.g. 1.15 = 15% larger)")]
    [SerializeField] private float selectedScaleMultiplier = 1.15f;
    [SerializeField] private float scaleSpeed = 10f;
    [SerializeField] private float loadDelay = 0.5f;
    [SerializeField] private Color pressedColor = new(0.5f, 0.5f, 0.5f, 1f);

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Main Game";
    
    private int selectedIndex = -1; // -1: None, 0: Play, 1: Exit
    private bool isTransitioning = false;

    private Vector3 playInitialScale = Vector3.one;
    private Vector3 exitInitialScale = Vector3.one;

    private void Awake()
    {
        // Save the exact original scales set in the Inspector/Scene
        if (playButtonRect != null) playInitialScale = playButtonRect.localScale;
        if (exitButtonRect != null) exitInitialScale = exitButtonRect.localScale;
    }

    private void Start()
    {
        if (playButton == null && playButtonRect != null) playButton = playButtonRect.GetComponent<Button>();
        if (exitButton == null && exitButtonRect != null) exitButton = exitButtonRect.GetComponent<Button>();

        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMainMenuBGM();
        }
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

    void Update()
    {
        if (isTransitioning) return;

        ApplyVisualFeedback();
    }

    private void OnConfirm()
    {
        if (selectedIndex == -1) return;

        if (selectedIndex == 0)
            StartCoroutine(PlayGameRoutine());
        else
            StartCoroutine(ExitGameRoutine());
    }

    private void OnMove()
    {
        selectionInput = InputManager.Instance.MoveInput.y;

        if (selectionInput > 0) selectedIndex = 0;
        if (selectionInput < 0) selectedIndex = 1;

        Debug.Log("Selected: " + (selectedIndex == 0 ? "Play" : "Exit"));
    }

    private void ApplyVisualFeedback()
    {
        if (playButtonRect == null || exitButtonRect == null) return;

        // Calculate target scale relative to the initial scene scale
        Vector3 playTargetScale = (selectedIndex == 0) ? (playInitialScale * selectedScaleMultiplier) : playInitialScale;
        Vector3 exitTargetScale = (selectedIndex == 1) ? (exitInitialScale * selectedScaleMultiplier) : exitInitialScale;

        playButtonRect.localScale = Vector3.Lerp(playButtonRect.localScale, playTargetScale, Time.deltaTime * scaleSpeed);
        exitButtonRect.localScale = Vector3.Lerp(exitButtonRect.localScale, exitTargetScale, Time.deltaTime * scaleSpeed);
    }

    private IEnumerator PlayGameRoutine()
    {
        isTransitioning = true;
        if (playButtonImage != null) playButtonImage.color = pressedColor;

        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator ExitGameRoutine()
    {
        isTransitioning = true;
        if (exitButtonImage != null) exitButtonImage.color = pressedColor;

        yield return new WaitForSeconds(loadDelay);
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    public void OnPlayButtonClicked()
    {
        if (isTransitioning) return;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayTouchSFX();
        selectedIndex = 0;
        ApplyVisualFeedback();
        StartCoroutine(PlayGameRoutine());
    }

    public void OnExitButtonClicked()
    {
        if (isTransitioning) return;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayTouchSFX();
        selectedIndex = 1;
        ApplyVisualFeedback();
        StartCoroutine(ExitGameRoutine());
    }

    public void SelectPlay()
    {
        if (!isTransitioning) selectedIndex = 0;
    }

    public void SelectExit()
    {
        if (!isTransitioning) selectedIndex = 1;
    }
}
