using UnityEngine;

public class WaterScroller : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float resetPositionX = 5f;  // Titik di mana sprite akan reset
    [SerializeField] private float startPositionX = -5f; // Titik awal setelah reset

    void Update()
    {
        // Gerakkan objek ke arah kanan
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Cek jika sudah melewati batas reset
        if (transform.position.x >= resetPositionX)
        {
            Vector3 newPos = transform.position;
            newPos.x = startPositionX;
            transform.position = newPos;
        }
    }
}
