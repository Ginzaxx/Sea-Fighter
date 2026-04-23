using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D Rb2D;
    [SerializeField] private Vector2 Velocity;
    [SerializeField] private bool isDead;

    void Start()
    {
        isDead = false;

        Rb2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Velocity = InputManager.Instance.MoveInput;

        if (GameOverManager.Instance.IsGameFinished) Velocity = Vector2.zero;
        
        Rb2D.linearVelocity = new Vector2(Velocity.x, Rb2D.linearVelocityY);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            isDead = true;

            GameOverManager.Instance.PlayerDied();
        }
    }
}
