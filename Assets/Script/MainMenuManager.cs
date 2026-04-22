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
    [SerializeField] private Color pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private float loadDelay = 0.5f;

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Main Game";

    private int selectedIndex = 0; // 0: Play, 1: Exit
    private bool isTransitioning = false;
    private Color originalPlayColor;
    private Color originalExitColor;

    void Start()
    {
        originalPlayColor = playButtonImage.color;
        originalExitColor = exitButtonImage.color;
    }

    void Update()
    {
        if (isTransitioning) return;

        HandleNavigation();
        HandleSelection();
        ApplyVisualFeedback();
    }

    private void HandleNavigation()
    {
        if (Keyboard.current == null) return;

        // Gunakan wasPressedThisFrame agar tidak scrolling terlalu cepat
        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            selectedIndex = (selectedIndex == 0) ? 1 : 0;
            Debug.Log("Selected: " + (selectedIndex == 0 ? "Play" : "Exit"));
        }
    }

    private void HandleSelection()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (selectedIndex == 0) StartCoroutine(PlayGameRoutine());
            else StartCoroutine(ExitGameRoutine());
        }
    }

    private void ApplyVisualFeedback()
    {
        // Smooth scaling untuk feedback visual
        float playTargetScale = (selectedIndex == 0) ? selectedScale : normalScale;
        float exitTargetScale = (selectedIndex == 1) ? selectedScale : normalScale;

        playButtonRect.localScale = Vector3.Lerp(playButtonRect.localScale, Vector3.one * playTargetScale, Time.deltaTime * scaleSpeed);
        exitButtonRect.localScale = Vector3.Lerp(exitButtonRect.localScale, Vector3.one * exitTargetScale, Time.deltaTime * scaleSpeed);
    }

    private IEnumerator PlayGameRoutine()
    {
        isTransitioning = true;
        
        // Efek menggelap
        playButtonImage.color = pressedColor;
        
        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator ExitGameRoutine()
    {
        isTransitioning = true;

        // Efek menggelap
        exitButtonImage.color = pressedColor;

        yield return new WaitForSeconds(loadDelay);
        Debug.Log("Exiting Game...");
        Application.Quit();
    }
}
