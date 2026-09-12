using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float speed;

    [SerializeField]
    private Renderer bgRenderer;

    void Update()
    {
        // Berhenti scroll jika game sudah selesai (menang atau kalah)
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameFinished)
            return;

        // X diatur ke 0 agar diam secara horizontal
        // Y menggunakan speed agar bergerak secara vertikal
        bgRenderer.material.mainTextureOffset += new Vector2(0, speed * Time.deltaTime);
    }
}