using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D Rb2D;
    [SerializeField] private PlayerHealth health;

    [Header("Movement Settings")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float moveInput;
    [SerializeField] private float targetX;

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

    private void OnDestroy()
    {
        OnUnsub();
    }

    private void Start()
    {
        OnSub();

        if (Rb2D == null) Rb2D = GetComponent<Rigidbody2D>();
        if (health == null) health = GetComponent<PlayerHealth>();
    }

    private void FixedUpdate()
    {
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameFinished) 
        {
            Rb2D.linearVelocity = new Vector2(0, 0);
            return;
        }

        MovePlayer();
    }

    private void OnMove()
    {
        moveInput = InputManager.Instance.MoveInput.x;

        targetX = transform.position.x + (laneDistance * moveInput);
        targetX = Mathf.Clamp(targetX, -laneDistance, laneDistance);
    }

    private void OnConfirm()
    {
        InputManager.Instance.ToggleFixedInputs();
    }

    private void MovePlayer()
    {
        moveInput = InputManager.Instance.MoveInput.x;

        if (!InputManager.Instance.UseFixedInputs)
        {
            Rb2D.linearVelocity = new Vector2(moveInput, 0);
            return;
        }

        float targetSpeed = Time.fixedDeltaTime * moveSpeed;
        float currentX = transform.position.x;
        float currentY = transform.position.y;
        float nextX = Mathf.Lerp(currentX, targetX, targetSpeed);
        
        Rb2D.MovePosition(new Vector2(nextX, currentY));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (health != null)
            {
                health.TakeDamage(1);
            }
            else
            {
                GameOverManager.Instance.PlayerDied();
            }
        }
    }
}