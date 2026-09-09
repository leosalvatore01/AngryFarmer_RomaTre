using TMPro;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public sealed class EliteFoxController : MonoBehaviour
{
    private const float ScalaEtichetta = 0.16f;

    private EnemyAI nemico;
    private float prossimoImpulso;
    private TextMeshPro etichetta;

    public int NumeroOndata { get; private set; }
    public int Capitolo { get; private set; }
    public int RicompensaMonete => nemico != null
        ? nemico.MonetePerEliminazione
        : 0;

    public static EliteFoxController Configura(
        EnemyAI nemico,
        int numeroOndata
    )
    {
        if (nemico == null) return null;

        EliteFoxController elite =
            nemico.GetComponent<EliteFoxController>();
        if (elite == null)
        {
            elite = nemico.gameObject.AddComponent<EliteFoxController>();
        }
        elite.Inizializza(numeroOndata);
        return elite;
    }

    void Awake()
    {
        nemico = GetComponent<EnemyAI>();
    }

    private void Inizializza(int numeroOndata)
    {
        NumeroOndata = Mathf.Max(1, numeroOndata);
        WaveBalanceSettings ondate = GameBalanceConfig.Corrente.Ondate;
        SpecialEncounterBalanceSettings impostazioni =
            ondate.IncontriSpeciali;
        Capitolo = SpecialEncounterDirector.CalcolaNumeroCapitolo(
            NumeroOndata,
            ondate.capitoli != null ? ondate.capitoli.ondePerCapitolo : 5
        );

        nemico.InizializzaVita(
            SpecialEncounterDirector.CalcolaVitaElite(
                nemico.VitaMassima,
                impostazioni
            )
        );
        nemico.ConfiguraRicompensaSpeciale(
            SpecialEncounterDirector.CalcolaMoneteElite(
                Capitolo,
                impostazioni
            ),
            impostazioni.dropGarantitiElite
        );
        if (etichetta != null) etichetta.gameObject.SetActive(true);
        transform.localScale *= impostazioni.moltiplicatoreScalaElite;
        gameObject.name = "Volpe_ELITE_" + nemico.NomeTipo;

        CreaEtichetta();
        prossimoImpulso = Time.time;
    }

    void Update()
    {
        if (nemico == null || nemico.IsDead)
        {
            if (etichetta != null) etichetta.gameObject.SetActive(false);
            return;
        }

        if (Time.time >= prossimoImpulso)
        {
            prossimoImpulso = Time.time + 0.9f;
            FoxAbilityVfx.CreaAnello(
                transform.position,
                new Color32(255, 197, 59, 220),
                0.38f,
                1.05f,
                0.72f,
                transform
            );
        }

        if (etichetta != null)
        {
            float impulso = 0.92f + Mathf.Sin(Time.time * 5f) * 0.08f;
            etichetta.transform.localScale =
                Vector3.one * ScalaEtichetta * impulso;
        }
    }

    private void CreaEtichetta()
    {
        if (etichetta != null) return;

        GameObject oggetto = new GameObject("EtichettaElite");
        oggetto.transform.SetParent(transform, false);
        oggetto.transform.localPosition = new Vector3(0f, 0.92f, 0f);
        oggetto.transform.localScale = Vector3.one * ScalaEtichetta;
        etichetta = oggetto.AddComponent<TextMeshPro>();
        etichetta.text = "ELITE";
        etichetta.fontSize = 2.35f;
        etichetta.fontStyle = FontStyles.Bold;
        etichetta.alignment = TextAlignmentOptions.Center;
        etichetta.textWrappingMode = TextWrappingModes.NoWrap;
        SpriteRenderer riferimento = nemico != null
            ? nemico.RendererVisibile
            : null;
        if (riferimento != null)
        {
            etichetta.renderer.sortingLayerID = riferimento.sortingLayerID;
            etichetta.renderer.sortingOrder = riferimento.sortingOrder + 45;
        }
        FarmPixelUI.ApplicaTestoMondo(
            etichetta,
            new Color32(255, 207, 76, 255)
        );
    }
}
