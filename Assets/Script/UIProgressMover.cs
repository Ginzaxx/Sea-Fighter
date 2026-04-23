using UnityEngine;

public class UIProgressMover : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private RectTransform uiElement;
    [SerializeField] private float minY = -420f;
    [SerializeField] private float maxY = 450f;

    private void Start()
    {
        // Jika uiElement tidak diisi di Inspector, coba ambil dari object ini
        if (uiElement == null) uiElement = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Pastikan GameOverManager ada
        if (GameOverManager.Instance == null) return;

        // Ambil data waktu dari GameOverManager
        float current = GameOverManager.Instance.CurrentTimer;
        float total = GameOverManager.Instance.SurvivalGoalTime;

        // Hitung progress (0 sampai 1)
        // Semakin besar currentTimer (semakin mendekati menang), progress semakin mendekati 1
        float progress = Mathf.Clamp01(current / total);

        // Hitung posisi Y berdasarkan progress
        // Lerp akan menghasilkan minY jika progress=0, dan maxY jika progress=1
        float targetY = Mathf.Lerp(minY, maxY, progress);

        // Update posisi anchoredPosition.y
        if (uiElement != null)
        {
            Vector2 pos = uiElement.anchoredPosition;
            pos.y = targetY;
            uiElement.anchoredPosition = pos;
        }
    }
}
