using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float destroyYLimit = -10f;

    private EnemySpawner spawner;

    public void Init(EnemySpawner spawnerInstance, float moveSpeed)
    {
        spawner = spawnerInstance;
        speed = moveSpeed;
    }

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);

        if (transform.position.y < destroyYLimit)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        gameObject.SetActive(false);
    }
}
