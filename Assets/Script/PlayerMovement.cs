using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D Rb2D;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Movement Settings")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private float moveSpeed = 15f;
    
    private float targetX = 0f;
    private Vector2 lastInput = Vector2.zero;

    void Start()
    {
        Rb2D = GetComponent<Rigidbody2D>();
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
        targetX = transform.position.x;
    }

    void Update()
    {
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameFinished) 
        {
            Rb2D.linearVelocity = new Vector2(0, Rb2D.linearVelocityY);
            return;
        }

        HandleInput();
    }

    private void FixedUpdate()
    {
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameFinished) return;
        
        MovePlayer();
    }

    private void HandleInput()
    {
        if (InputManager.Instance == null) return;

        Vector2 currentInput = InputManager.Instance.MoveInput;

        // Detect single press for Left
        if (currentInput.x < -0.5f && lastInput.x >= -0.5f)
        {
            targetX = Mathf.Clamp(targetX - laneDistance, -laneDistance, laneDistance);
        }
        // Detect single press for Right
        else if (currentInput.x > 0.5f && lastInput.x <= 0.5f)
        {
            targetX = Mathf.Clamp(targetX + laneDistance, -laneDistance, laneDistance);
        }

        lastInput = currentInput;
    }

    private void MovePlayer()
    {
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
