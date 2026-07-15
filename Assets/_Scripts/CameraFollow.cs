using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Range(0.1f, 0.9f)]
    public float zonaMortaOrizzontale = 0.5f;

    [Range(0.1f, 0.9f)]
    public float zonaMortaVerticale = 0.5f;

    public float tempoFluidita = 0.25f;

    [Header("Vibrazione colpi potenti")]
    [SerializeField] private bool vibrazioneAbilitata = true;

    private Transform giocatore;
    private Camera cameraPrincipale;
    private Vector3 velocitaCamera;
    private float tempoVibrazione;
    private float durataVibrazione;
    private float intensitaVibrazione;
    private Vector2 offsetVibrazione;
    private Vector3 posizionePrimaDellaVibrazione;
    private bool offsetApplicatoAlRendering;

    private static readonly Vector2[] SequenzaVibrazione =
    {
        new Vector2(0.88f, 0.28f),
        new Vector2(-0.42f, 0.91f),
        new Vector2(-0.94f, -0.19f),
        new Vector2(0.31f, -0.95f),
        new Vector2(0.73f, 0.61f),
        new Vector2(-0.81f, 0.49f),
        new Vector2(-0.26f, -0.86f),
        new Vector2(0.92f, -0.37f)
    };

    public bool VibrazioneAbilitata => vibrazioneAbilitata;
    public bool VibrazioneInCorso => tempoVibrazione > 0f;
    public Vector2 OffsetVibrazioneCorrente => offsetVibrazione;
    public int VibrazioniAccettate { get; private set; }

    void Start()
    {
        cameraPrincipale = GetComponent<Camera>();
        vibrazioneAbilitata =
            GameBalanceConfig.Corrente.FeedbackCombattimento
                .vibrazioneCameraAttiva;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            giocatore = player.transform;

            transform.position = new Vector3(
                giocatore.position.x,
                giocatore.position.y,
                transform.position.z
            );
        }
    }

    void LateUpdate()
    {
        RipristinaPosizioneLogica();

        if (giocatore != null && cameraPrincipale != null)
        {
            AggiornaInseguimento();
        }

        AggiornaVibrazione();
    }

    private void AggiornaInseguimento()
    {
        float mezzaLarghezza =
            cameraPrincipale.orthographicSize * cameraPrincipale.aspect;
        float mezzaAltezza = cameraPrincipale.orthographicSize;

        // Con valore 0.5: il contadino può muoversi
        // nel 50% centrale della schermata senza spostare la camera.
        float mezzaZonaMortaX = mezzaLarghezza * zonaMortaOrizzontale;
        float mezzaZonaMortaY = mezzaAltezza * zonaMortaVerticale;

        Vector3 destinazione = transform.position;

        if (giocatore.position.x < transform.position.x - mezzaZonaMortaX)
        {
            destinazione.x = giocatore.position.x + mezzaZonaMortaX;
        }
        else if (giocatore.position.x > transform.position.x + mezzaZonaMortaX)
        {
            destinazione.x = giocatore.position.x - mezzaZonaMortaX;
        }

        if (giocatore.position.y < transform.position.y - mezzaZonaMortaY)
        {
            destinazione.y = giocatore.position.y + mezzaZonaMortaY;
        }
        else if (giocatore.position.y > transform.position.y + mezzaZonaMortaY)
        {
            destinazione.y = giocatore.position.y - mezzaZonaMortaY;
        }

        Vector3 posizioneFluida = Vector3.SmoothDamp(
            transform.position,
            destinazione,
            ref velocitaCamera,
            tempoFluidita
        );

        transform.position = new Vector3(
            posizioneFluida.x,
            posizioneFluida.y,
            transform.position.z
        );
    }

    public void ImpostaVibrazioneAbilitata(bool abilitata)
    {
        vibrazioneAbilitata = abilitata;
        if (!abilitata)
        {
            tempoVibrazione = 0f;
            offsetVibrazione = Vector2.zero;
            RipristinaPosizioneLogica();
        }
    }

    public bool RichiediVibrazione(float intensita, float durata)
    {
        if (!vibrazioneAbilitata || intensita <= 0f || durata <= 0f)
        {
            return false;
        }

        intensitaVibrazione = Mathf.Max(intensitaVibrazione, intensita);
        durataVibrazione = Mathf.Max(durataVibrazione, durata);
        tempoVibrazione = Mathf.Max(tempoVibrazione, durata);
        VibrazioniAccettate++;
        return true;
    }

    private void AggiornaVibrazione()
    {
        if (!vibrazioneAbilitata || tempoVibrazione <= 0f)
        {
            tempoVibrazione = 0f;
            offsetVibrazione = Vector2.zero;
            intensitaVibrazione = 0f;
            durataVibrazione = 0f;
            return;
        }

        tempoVibrazione = Mathf.Max(
            0f,
            tempoVibrazione - Time.unscaledDeltaTime
        );
        float rapporto = tempoVibrazione /
            Mathf.Max(0.01f, durataVibrazione);
        float decadimento = rapporto * rapporto;
        float trascorso = durataVibrazione - tempoVibrazione;
        int indice = Mathf.FloorToInt(trascorso / 0.012f) %
                     SequenzaVibrazione.Length;
        offsetVibrazione =
            SequenzaVibrazione[indice] *
            intensitaVibrazione *
            decadimento;

        if (tempoVibrazione <= 0f)
        {
            offsetVibrazione = Vector2.zero;
            intensitaVibrazione = 0f;
            durataVibrazione = 0f;
        }
    }

    void OnPreCull()
    {
        if (offsetApplicatoAlRendering ||
            offsetVibrazione.sqrMagnitude <= 0.0000001f)
        {
            return;
        }

        posizionePrimaDellaVibrazione = transform.position;
        transform.position = posizionePrimaDellaVibrazione +
            (Vector3)offsetVibrazione;
        offsetApplicatoAlRendering = true;
    }

    void OnPostRender()
    {
        RipristinaPosizioneLogica();
    }

    private void RipristinaPosizioneLogica()
    {
        if (!offsetApplicatoAlRendering) return;
        transform.position = posizionePrimaDellaVibrazione;
        offsetApplicatoAlRendering = false;
    }

    void OnDisable()
    {
        RipristinaPosizioneLogica();
        offsetVibrazione = Vector2.zero;
        tempoVibrazione = 0f;
    }
}
