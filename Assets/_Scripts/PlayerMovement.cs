using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;

    private Rigidbody2D corpo;
    private Vector2 direzione;

    void Awake()
    {
        corpo = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        direzione = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        corpo.MovePosition(
            corpo.position + direzione * speed * Time.fixedDeltaTime
        );
    }
}