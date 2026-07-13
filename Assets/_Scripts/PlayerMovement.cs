using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 8f;
    public float durataBoost = 5f;

    public Vector2 DirezioneMovimento { get; private set; }

    private Rigidbody2D corpo;
    private float moltiplicatoreVelocita = 1f;
    private Coroutine boostRoutine;

    void Awake()
    {
        corpo = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        DirezioneMovimento = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        corpo.MovePosition(
            corpo.position +
            DirezioneMovimento * speed * moltiplicatoreVelocita * Time.fixedDeltaTime
        );
    }

    public void AttivaBoostVelocita()
    {
        if (boostRoutine != null)
        {
            StopCoroutine(boostRoutine);
        }

        boostRoutine = StartCoroutine(BoostVelocita());
    }

    IEnumerator BoostVelocita()
    {
        moltiplicatoreVelocita = 2f;
        yield return new WaitForSeconds(durataBoost);
        moltiplicatoreVelocita = 1f;
        boostRoutine = null;
    }
}
