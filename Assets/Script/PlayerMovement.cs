using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D Rb2D;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Movement Settings")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float moveInput;
    [SerializeField] private bool IsUsingBaseInputs;
    
    private float targetX = 0f;
    private float lastInput = 0;

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

    void Start()
    {
        Rb2D ??= GetComponent<Rigidbody2D>();
        playerHealth ??= GetComponent<PlayerHealth>();

        targetX = transform.position.x;
    }

    void Update()
    {
        if (IsUsingBaseInputs) return;
        if (InputManager.Instance == null) return;

        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameFinished) 
        {
            Rb2D.linearVelocityX = 0;
            return;
        }

        moveInput = InputManager.Instance.MoveInput.x;

        Rb2D.linearVelocityX = moveInput;
    }

    private void FixedUpdate()
    {
        float currentX = transform.position.x;
        float nextX = Mathf.Lerp(currentX, targetX, Time.fixedDeltaTime * moveSpeed);
        
        Rb2D.MovePosition(new Vector2(nextX, transform.position.y));
    }

    private void OnMove()
    {
        if (!IsUsingBaseInputs) return;
        if (InputManager.Instance == null) return;

        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameFinished) 
        {
            Rb2D.linearVelocityX = 0;
            return;
        }

        moveInput = InputManager.Instance.MoveInput.x;

        if (moveInput < -0.5f && lastInput >= -0.5f)
        {
            targetX = Mathf.Clamp(targetX - laneDistance, -laneDistance, laneDistance);
        }
        else if (moveInput > 0.5f && lastInput <= 0.5f)
        {
            targetX = Mathf.Clamp(targetX + laneDistance, -laneDistance, laneDistance);
        }

        lastInput = moveInput;
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
                GameOverManager.Instance.PlayerDied();
            }
        }
    }
}
