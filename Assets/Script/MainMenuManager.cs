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

    [Header("Feedback Settings")]
    [SerializeField] private float selectedScale = 3.2f;
    [SerializeField] private float normalScale = 3.0f;
    [SerializeField] private float scaleSpeed = 10f;
    [SerializeField] private float loadDelay = 0.5f;
    // [SerializeField] private float selectionInput;
    [SerializeField] private Color pressedColor = new(0.5f, 0.5f, 0.5f, 1f);

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Main Game";

    private int selectedIndex = 0; // 0: Play, 1: Exit
    private bool isTransitioning = false;

    void OnEnable()
    {
        InputManager.Instance.OnMoveOn += OnMoveOn;
        InputManager.Instance.OnConfirm += OnConfirm;
    }

    void OnDisable()
    {
        InputManager.Instance.OnMoveOn -= OnMoveOn;
        InputManager.Instance.OnConfirm -= OnConfirm;
    }

    void Update()
    {
        // selectionInput = InputManager.Instance.MoveInput.y;

        if (isTransitioning) return;

        ApplyVisualFeedback();
    }

    private void OnConfirm()
    {
        if (selectedIndex == 0) StartCoroutine(PlayGameRoutine());
        else StartCoroutine(ExitGameRoutine());
    }

    private void OnMoveOn()
    {
        selectedIndex = (selectedIndex == 0) ? 1 : 0;
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
}
