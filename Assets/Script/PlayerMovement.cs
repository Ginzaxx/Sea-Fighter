using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D Rb2D;
    [SerializeField] private Vector2 Velocity;
    [SerializeField] private PlayerHealth playerHealth;

    void Start()
    {
        Rb2D = GetComponent<Rigidbody2D>();
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (GameOverManager.Instance.IsGameFinished) return;

        Velocity = InputManager.Instance.MoveInput;
        
        Rb2D.linearVelocity = new Vector2(Velocity.x, Rb2D.linearVelocityY);
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
