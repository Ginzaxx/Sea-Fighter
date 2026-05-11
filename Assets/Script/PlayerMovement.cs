using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D Rb2D;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Movement Settings")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float moveInput;
    private Vector2 lastInput = Vector2.zero;
    private float targetX = 0f;

    private void OnSub()
    {
        InputManager.Instance.OnMove += OnMove;
        InputManager.Instance.OnConfirm += OnConfirm;
    }

    private void OnUnsub()
    {
        InputManager.Instance.OnMove -= OnMove;
        InputManager.Instance.OnConfirm -= OnConfirm;
    }

    void OnDestroy()
    {
        OnUnsub();
    }

    void Start()
    {
        OnSub();

        if (Rb2D == null)
            Rb2D = GetComponent<Rigidbody2D>();
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        targetX = transform.position.x;
    }

    void Update()
    {
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameFinished) 
        {
            Rb2D.linearVelocity = new Vector2(0, Rb2D.linearVelocityY);
            return;
        }
    }

    private void FixedUpdate()
    {
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameFinished) return;

        MovePlayer();
    }

    private void OnMove()
    {
        moveInput = InputManager.Instance.MoveInput.x;

        if (!InputManager.Instance.UseFixedInputs) return;

        targetX = Mathf.Clamp(targetX + (laneDistance * moveInput), -laneDistance, laneDistance);
    }

    private void OnConfirm()
    {
        InputManager.Instance.UseFixedInputs = !InputManager.Instance.UseFixedInputs;
    }

    private void MovePlayer()
    {
        moveInput = InputManager.Instance.MoveInput.x;

        if (!InputManager.Instance.UseFixedInputs)
        {
            Rb2D.linearVelocity = new Vector2(moveInput, Rb2D.linearVelocityY);
            return;
        }

        float currentX = transform.position.x;
        float nextX = Mathf.Lerp(currentX, targetX, Time.fixedDeltaTime * moveSpeed);
        
        Rb2D.MovePosition(new Vector2(nextX, transform.position.y));
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
                GameOverManager.Instance.PlayerDied();
            }
        }
    }
}
