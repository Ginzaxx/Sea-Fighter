using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D Rb2D;
    [SerializeField] private Transform playerTF;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Variables")]
    [SerializeField] private float BoatInput;
    [SerializeField] private bool IsUsingBaseInputs;

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

    void Start()
    {
        Rb2D = GetComponent<Rigidbody2D>();
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (IsUsingBaseInputs) return;

        BoatInput = InputManager.Instance.MoveInput.x;

        if (GameOverManager.Instance.IsGameFinished) BoatInput = 0;
        
        Rb2D.linearVelocity = new Vector2(BoatInput, Rb2D.linearVelocityY);
    }

    private void OnMoveOn()
    {
        if (!IsUsingBaseInputs) return;

        BoatInput = InputManager.Instance.MoveInput.x;

        if (playerTF.transform.position.x <= 2 && playerTF.transform.position.x >= -2)
        {
            playerTF.transform.position = new Vector3(BoatInput * 2, 0, 0);
        }
    }

    private void OnConfirm()
    {
        Debug.Log("Using Base Inputs");

        IsUsingBaseInputs = !IsUsingBaseInputs;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
            else
            {
                // Fallback jika komponen health tidak ditemukan
                GameOverManager.Instance.PlayerDied();
            }
        }
    }
}
