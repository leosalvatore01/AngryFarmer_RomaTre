using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    // Mantenuto come int per non rompere i prefab Dente (0) e Coda (1).
    public int type;

    private static readonly List<PowerUp> dropAttivi = new List<PowerUp>();
    private Vector3 scalaBase;
    private SpriteRenderer rendererDrop;

    public TipoDropTemporaneo Tipo => CatalogoDropTemporanei.Normalizza(type);
    public float DurataDespawn { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraRegistro()
    {
        dropAttivi.Clear();
    }

    void OnEnable()
    {
        if (!dropAttivi.Contains(this)) dropAttivi.Add(this);
    }

    void OnDisable()
    {
        dropAttivi.Remove(this);
    }

    void Start()
    {
        scalaBase = transform.localScale;
        DurataDespawn = Mathf.Max(
            0.1f,
            GameBalanceConfig.Corrente.Volpe.durataDropSullaMappa
        );
        AggiornaAspetto();
        Destroy(gameObject, DurataDespawn);
    }

    void Update()
    {
        transform.Rotate(0f, 0f, 120f * Time.deltaTime);
        float pulsazione = 1f + Mathf.Sin(Time.time * 5f) * 0.12f;
        transform.localScale = scalaBase * pulsazione;
    }

    public void Configura(TipoDropTemporaneo nuovoTipo)
    {
        type = (int)nuovoTipo;
        gameObject.name = "Drop_" + nuovoTipo;
        AggiornaAspetto();
    }

    private void AggiornaAspetto()
    {
        if (rendererDrop == null)
            rendererDrop = GetComponentInChildren<SpriteRenderer>();
        if (rendererDrop == null) return;

        DefinizioneDropTemporaneo definizione =
            CatalogoDropTemporanei.Ottieni(Tipo);
        rendererDrop.sprite = FarmPixelUI.OttieniIcona(definizione.Icona);
        rendererDrop.color = Color.white;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerTemporaryEffects effetti =
            PlayerTemporaryEffects.AggiungiOTrova(other.gameObject);
        if (effetti == null || !effetti.Applica(Tipo)) return;

        DefinizioneDropTemporaneo definizione =
            CatalogoDropTemporanei.Ottieni(Tipo);
        RunTelemetryController.Corrente?.RegistraDropTemporaneo(
            definizione.Nome,
            definizione.Effetto
        );
        CombatFeedbackController.CreaOTrova()?.RegistraRicompensa(
            transform.position,
            definizione.Colore,
            definizione.Istantaneo
        );
        Destroy(gameObject);
    }

    public static int AttraiDropVerso(
        Vector2 centro,
        float raggio,
        float velocita,
        float deltaTime
    )
    {
        int attratti = 0;
        float raggioQuadrato = raggio * raggio;
        for (int i = dropAttivi.Count - 1; i >= 0; i--)
        {
            PowerUp drop = dropAttivi[i];
            if (drop == null)
            {
                dropAttivi.RemoveAt(i);
                continue;
            }

            Vector2 posizione = drop.transform.position;
            if ((posizione - centro).sqrMagnitude > raggioQuadrato) continue;
            drop.transform.position = Vector2.MoveTowards(
                posizione,
                centro,
                velocita * Mathf.Max(0f, deltaTime)
            );
            attratti++;
        }
        return attratti;
    }
}
