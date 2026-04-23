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
        
        Rb2D.linearVelocity = new Vector2(Velocity.x, Rb2D.linearVelocityY);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isDead = true;

            GameOverManager.Instance.PlayerDied();
        }
    }
}
