using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 8f;
    public float durataBoost = 5f;

    [Header("Fluidità")]
    [Min(0f)] public float accelerazione = 45f;
    [Min(0f)] public float decelerazione = 60f;

    public Vector2 DirezioneMovimento { get; private set; }
    public Vector2 VelocitaAttuale { get; private set; }
    public float MoltiplicatoreVelocitaCorrente => moltiplicatoreVelocita;
    public bool StaCamminando => VelocitaAttuale.sqrMagnitude > 0.01f;

    private Rigidbody2D corpo;
    private float moltiplicatoreVelocita = 1f;
    private Coroutine boostRoutine;

    void Awake()
    {
        corpo = GetComponent<Rigidbody2D>();
        corpo.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        DirezioneMovimento = Vector2.ClampMagnitude(new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ), 1f);
    }

    void FixedUpdate()
    {
        Vector2 velocitaDesiderata =
            DirezioneMovimento * speed * moltiplicatoreVelocita;

        float rapiditaCambio = DirezioneMovimento.sqrMagnitude > 0.001f
            ? accelerazione
            : decelerazione;

        if (Vector2.Dot(VelocitaAttuale, velocitaDesiderata) < 0f)
        {
            rapiditaCambio *= 1.35f;
        }

        VelocitaAttuale = Vector2.MoveTowards(
            VelocitaAttuale,
            velocitaDesiderata,
            rapiditaCambio * Time.fixedDeltaTime
        );

        corpo.MovePosition(
            corpo.position +
            VelocitaAttuale * Time.fixedDeltaTime
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
