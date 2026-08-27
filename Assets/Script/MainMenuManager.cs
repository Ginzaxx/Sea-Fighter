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
    [SerializeField] private float selectedScale = 3.2f;
    [SerializeField] private float normalScale = 3.0f;
    [SerializeField] private float scaleSpeed = 10f;
    [SerializeField] private float loadDelay = 0.5f;
    [SerializeField] private Color pressedColor = new(0.5f, 0.5f, 0.5f, 1f);

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Main Game";
    
    private int selectedIndex = 0; // 0: Play, 1: Exit
    private bool isTransitioning = false;

    private void Start()
    {
        if (playButton == null && playButtonRect != null) playButton = playButtonRect.GetComponent<Button>();
        if (exitButton == null && exitButtonRect != null) exitButton = exitButtonRect.GetComponent<Button>();

        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);
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
        // Smooth scaling for visual feedback
        float playTargetScale = (selectedIndex == 0) ? selectedScale : normalScale;
        float exitTargetScale = (selectedIndex == 1) ? selectedScale : normalScale;

        playButtonRect.localScale = Vector3.Lerp(playButtonRect.localScale, Vector3.one * playTargetScale, Time.deltaTime * scaleSpeed);
        exitButtonRect.localScale = Vector3.Lerp(exitButtonRect.localScale, Vector3.one * exitTargetScale, Time.deltaTime * scaleSpeed);
    }

    private IEnumerator PlayGameRoutine()
    {
        isTransitioning = true;
        playButtonImage.color = pressedColor;

        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator ExitGameRoutine()
    {
        isTransitioning = true;
        exitButtonImage.color = pressedColor;

        yield return new WaitForSeconds(loadDelay);
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    public void OnPlayButtonClicked()
    {
        if (isTransitioning) return;
        selectedIndex = 0;
        ApplyVisualFeedback();
        StartCoroutine(PlayGameRoutine());
    }

    public void OnExitButtonClicked()
    {
        if (isTransitioning) return;
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
