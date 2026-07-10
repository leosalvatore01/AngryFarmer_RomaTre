using UnityEngine;

public class WindSway : MonoBehaviour
{
    public float ampiezza = 3f;
    public float velocita = 1.5f;

    private Quaternion rotazioneIniziale;

    void Start()
    {
        rotazioneIniziale = transform.rotation;
    }

    void Update()
    {
        float oscillazione = Mathf.Sin(
            Time.time * velocita + transform.position.x
        ) * ampiezza;

        transform.rotation = rotazioneIniziale *
            Quaternion.Euler(0f, 0f, oscillazione);
    }
}