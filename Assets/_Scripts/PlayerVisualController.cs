using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisualController : MonoBehaviour
{
    private const float QuotaAppoggio = -0.45f;

    [Header("Corsa")]
    [Min(0.1f)] public float distanzaTraPassi = 0.82f;
    [Min(0f)] public float ampiezzaCamminata = 0.018f;
    [Range(0f, 0.1f)] public float compressioneAppoggio = 0.05f;

    [Header("Feedback sparo")]
    [Min(0.01f)] public float durataFeedbackSparo = 0.1f;
    [Min(0f)] public float distanzaRinculo = 0.08f;
    [Range(0f, 0.1f)] public float compressioneSparo = 0.035f;
    [Min(0.01f)] public float durataFrameLampo = 0.032f;
    [Min(0f)] public float distanzaLampo = 0.34f;

    private SpriteRenderer spriteRenderer;
    private Transform grafica;
    private PlayerMovement movimento;
    private PlayerShooting sparo;
    private Sprite[] frameCamminata;
    private SpriteRenderer rendererLampo;
    private static Sprite[] spriteLampo;

    private int direzioneCorrente = 0;
    private Vector2 ultimaPosizione;
    private float avanzamentoCorsa;
    private float tempoFeedbackSparo;
    private Vector2 direzioneRinculo = Vector2.down;
    private float tempoLampo;
    private bool lampoPerforante;
    private float intensitaFeedbackSparo = 1f;

    public int LampiEmessi { get; private set; }
    public bool LampoVisibile =>
        rendererLampo != null && rendererLampo.enabled;

    void Awake()
    {
        spriteRenderer = CreaRendererVisivo(GetComponent<SpriteRenderer>());
        movimento = GetComponent<PlayerMovement>();
        sparo = GetComponent<PlayerShooting>();
        ultimaPosizione = transform.position;
        CreaRendererLampo();

        frameCamminata = Resources.LoadAll<Sprite>("FarmerWalk-v1");

        Array.Sort(frameCamminata, (a, b) =>
            NumeroFrame(a.name).CompareTo(NumeroFrame(b.name))
        );

        if (frameCamminata.Length < 16)
        {
            Debug.LogError("FarmerWalk-v1 deve contenere 16 frame.");
        }

        OmbraDinamica2D.Crea(
            transform,
            spriteRenderer,
            new Vector2(0f, QuotaAppoggio),
            new Vector2(0.66f, 0.22f)
        );
    }

    void LateUpdate()
    {
        AggiornaLampoSparo();

        if (frameCamminata.Length < 16 || movimento == null)
        {
            AggiornaMovimentoVisivo(0f, 0f);
            return;
        }

        Vector2 posizione = transform.position;
        float distanzaPercorsa = Vector2.Distance(ultimaPosizione, posizione);
        ultimaPosizione = posizione;

        if (sparo != null && sparo.ProvaOttieniDirezioneMira(out Vector2 direzioneMira))
        {
            direzioneCorrente = OttieniDirezione(direzioneMira);
        }
        else if (movimento.StaCamminando)
        {
            direzioneCorrente = OttieniDirezione(movimento.VelocitaAttuale);
        }

        if (!movimento.StaCamminando || movimento.SchivataAttiva)
        {
            avanzamentoCorsa = 0f;
            spriteRenderer.sprite = frameCamminata[direzioneCorrente * 2];
            AggiornaMovimentoVisivo(0f, 0f);
            return;
        }

        if (Time.deltaTime > 0f)
        {
            avanzamentoCorsa = Mathf.Repeat(
                avanzamentoCorsa + distanzaPercorsa / Mathf.Max(0.1f, distanzaTraPassi),
                2f
            );
        }

        // Ogni cambio di posa inizia con l'appoggio, poi il piede si solleva.
        int passo = (int)avanzamentoCorsa;
        float fase = avanzamentoCorsa - passo;
        float appoggio = 1f - Mathf.Clamp01(fase / 0.35f);
        float sollevamento = Mathf.Sin(Mathf.InverseLerp(0.45f, 1f, fase) * Mathf.PI);
        spriteRenderer.sprite = frameCamminata[direzioneCorrente * 2 + passo];
        AggiornaMovimentoVisivo(sollevamento * ampiezzaCamminata, appoggio * compressioneAppoggio);
    }

    public void RiproduciFeedbackSparo(
        Vector2 direzione,
        bool potente = false,
        bool perforante = false
    )
    {
        if (direzione.sqrMagnitude <= 0.0001f) return;

        direzioneRinculo = direzione.normalized;
        direzioneCorrente = OttieniDirezione(direzioneRinculo);
        tempoFeedbackSparo = Mathf.Max(0.01f, durataFeedbackSparo);
        intensitaFeedbackSparo = potente ? 1.45f : perforante ? 1.25f : 1.1f;

        bool flashConsentito =
            GameOptionsController.Instance == null ||
            GameOptionsController.Instance.FlashAttivi;
        if (rendererLampo != null && flashConsentito)
        {
            float angolo = Mathf.Atan2(
                direzioneRinculo.y,
                direzioneRinculo.x
            ) * Mathf.Rad2Deg;
            rendererLampo.transform.localPosition =
                (Vector3)(direzioneRinculo * distanzaLampo);
            rendererLampo.transform.localRotation =
                Quaternion.Euler(0f, 0f, angolo);
            rendererLampo.transform.localScale = potente
                ? new Vector3(1.18f, 1.12f, 1f)
                : Vector3.one;
            rendererLampo.color = perforante
                ? new Color(1f, 0.82f, 0.25f, 1f)
                : Color.white;
            rendererLampo.sprite = OttieniSpriteLampo()[0];
            rendererLampo.enabled = true;
            tempoLampo = Mathf.Max(0.01f, durataFrameLampo) * 2f;
            lampoPerforante = perforante;
            LampiEmessi++;
        }
    }

    private void CreaRendererLampo()
    {
        if (grafica == null || spriteRenderer == null) return;

        GameObject oggetto = new GameObject("LampoSparo");
        oggetto.layer = gameObject.layer;
        oggetto.transform.SetParent(grafica, false);

        rendererLampo = oggetto.AddComponent<SpriteRenderer>();
        rendererLampo.sprite = OttieniSpriteLampo()[0];
        rendererLampo.sortingLayerID = spriteRenderer.sortingLayerID;
        rendererLampo.sortingOrder = spriteRenderer.sortingOrder + 3;
        rendererLampo.enabled = false;
    }

    private void AggiornaLampoSparo()
    {
        if (rendererLampo == null || !rendererLampo.enabled) return;
        if (GameOptionsController.Instance != null && !GameOptionsController.Instance.FlashAttivi)
        {
            rendererLampo.enabled = false;
            tempoLampo = 0f;
            return;
        }
        if (GameManager.instance != null &&
            GameManager.instance.PausaManualeAttiva)
        {
            return;
        }

        float durataFrame = Mathf.Max(0.01f, durataFrameLampo);
        tempoLampo = Mathf.Max(
            0f,
            tempoLampo - Time.unscaledDeltaTime
        );

        if (tempoLampo <= 0f)
        {
            rendererLampo.enabled = false;
            return;
        }

        rendererLampo.sprite = OttieniSpriteLampo()[
            tempoLampo > durataFrame ? 0 : 1
        ];
        rendererLampo.sortingLayerID = spriteRenderer.sortingLayerID;
        rendererLampo.sortingOrder = spriteRenderer.sortingOrder + 3;
        rendererLampo.color = lampoPerforante
            ? new Color(1f, 0.82f, 0.25f, 1f)
            : Color.white;
    }

    private static Sprite[] OttieniSpriteLampo()
    {
        if (spriteLampo != null) return spriteLampo;

        spriteLampo = new[]
        {
            CreaSpriteLampo(false),
            CreaSpriteLampo(true)
        };
        return spriteLampo;
    }

    private static Sprite CreaSpriteLampo(bool secondoFrame)
    {
        const int larghezza = 12;
        const int altezza = 9;
        Color32[] pixel = new Color32[larghezza * altezza];
        Color32 scuro = new Color32(104, 48, 17, 255);
        Color32 arancio = new Color32(244, 126, 29, 255);
        Color32 giallo = new Color32(255, 211, 67, 255);
        Color32 crema = new Color32(255, 244, 186, 255);

        int centroY = altezza / 2;
        if (!secondoFrame)
        {
            DisegnaRettangolo(pixel, larghezza, 0, centroY - 1, 8, 3, scuro);
            DisegnaRettangolo(pixel, larghezza, 1, centroY, 9, 1, crema);
            DisegnaRettangolo(pixel, larghezza, 2, centroY - 1, 6, 3, giallo);
            DisegnaRettangolo(pixel, larghezza, 4, centroY - 3, 2, 7, scuro);
            DisegnaRettangolo(pixel, larghezza, 5, centroY - 2, 2, 5, arancio);
            DisegnaRettangolo(pixel, larghezza, 8, centroY - 1, 3, 3, scuro);
            DisegnaRettangolo(pixel, larghezza, 8, centroY, 2, 1, crema);
        }
        else
        {
            DisegnaRettangolo(pixel, larghezza, 0, centroY - 1, 7, 3, scuro);
            DisegnaRettangolo(pixel, larghezza, 1, centroY, 7, 1, crema);
            DisegnaRettangolo(pixel, larghezza, 2, centroY - 1, 4, 3, giallo);
            DisegnaRettangolo(pixel, larghezza, 5, centroY - 2, 2, 5, scuro);
            DisegnaRettangolo(pixel, larghezza, 6, centroY - 1, 2, 3, arancio);
            DisegnaRettangolo(pixel, larghezza, 8, centroY, 2, 1, scuro);
        }

        Texture2D texture = new Texture2D(
            larghezza,
            altezza,
            TextureFormat.RGBA32,
            false
        );
        texture.name = secondoFrame
            ? "TextureLampoSparo_2"
            : "TextureLampoSparo_1";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels32(pixel);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, larghezza, altezza),
            new Vector2(0f, 0.5f),
            32f,
            0,
            SpriteMeshType.FullRect
        );
        sprite.name = secondoFrame ? "LampoSparo_2" : "LampoSparo_1";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static void DisegnaRettangolo(
        Color32[] pixel,
        int larghezza,
        int x,
        int y,
        int dimensioneX,
        int dimensioneY,
        Color32 colore
    )
    {
        int altezza = pixel.Length / larghezza;
        for (int py = y; py < y + dimensioneY; py++)
        {
            for (int px = x; px < x + dimensioneX; px++)
            {
                if (px < 0 || py < 0 || px >= larghezza || py >= altezza)
                {
                    continue;
                }
                pixel[py * larghezza + px] = colore;
            }
        }
    }

    SpriteRenderer CreaRendererVisivo(SpriteRenderer rendererOriginale)
    {
        GameObject oggettoGrafico = new GameObject("Grafica");
        oggettoGrafico.layer = gameObject.layer;

        grafica = oggettoGrafico.transform;
        grafica.SetParent(transform, false);

        SpriteRenderer nuovoRenderer = oggettoGrafico.AddComponent<SpriteRenderer>();
        nuovoRenderer.sprite = rendererOriginale.sprite;
        nuovoRenderer.color = rendererOriginale.color;
        nuovoRenderer.flipX = rendererOriginale.flipX;
        nuovoRenderer.flipY = rendererOriginale.flipY;
        nuovoRenderer.drawMode = rendererOriginale.drawMode;
        nuovoRenderer.size = rendererOriginale.size;
        nuovoRenderer.maskInteraction = rendererOriginale.maskInteraction;
        nuovoRenderer.spriteSortPoint = rendererOriginale.spriteSortPoint;
        nuovoRenderer.sortingLayerID = rendererOriginale.sortingLayerID;
        nuovoRenderer.sortingOrder = rendererOriginale.sortingOrder;
        nuovoRenderer.sharedMaterials = rendererOriginale.sharedMaterials;
        nuovoRenderer.enabled = rendererOriginale.enabled;

        rendererOriginale.enabled = false;
        return nuovoRenderer;
    }

    private void AggiornaMovimentoVisivo(float sollevamento, float compressione)
    {
        if (grafica == null) return;

        float forzaSparo = 0f;
        if (tempoFeedbackSparo > 0f)
        {
            float durata = Mathf.Max(0.01f, durataFeedbackSparo);
            forzaSparo = Mathf.Clamp01(tempoFeedbackSparo / durata);
            forzaSparo = forzaSparo * forzaSparo * intensitaFeedbackSparo;
            tempoFeedbackSparo = Mathf.Max(
                0f,
                tempoFeedbackSparo - Time.deltaTime
            );
        }

        Vector2 offsetRinculo =
            -direzioneRinculo * distanzaRinculo * forzaSparo;
        compressione += compressioneSparo * forzaSparo;
        grafica.localPosition = new Vector3(
            offsetRinculo.x,
            QuotaAppoggio * compressione + sollevamento + offsetRinculo.y,
            0f
        );

        grafica.localScale = new Vector3(
            1f + compressione * 0.55f,
            1f - compressione,
            1f
        );
    }

    int OttieniDirezione(Vector2 direzione)
    {
        float angolo = Mathf.Atan2(direzione.y, direzione.x) * Mathf.Rad2Deg;

        if (angolo >= -112.5f && angolo < -67.5f) return 0;  // Sud
        if (angolo >= -67.5f && angolo < -22.5f) return 1;   // Sud-est
        if (angolo >= -22.5f && angolo < 22.5f) return 2;    // Est
        if (angolo >= 22.5f && angolo < 67.5f) return 3;     // Nord-est
        if (angolo >= 67.5f && angolo < 112.5f) return 4;    // Nord
        if (angolo >= 112.5f && angolo < 157.5f) return 5;   // Nord-ovest
        if (angolo >= 157.5f || angolo < -157.5f) return 6;  // Ovest

        return 7; // Sud-ovest
    }

    int NumeroFrame(string nome)
    {
        string[] parti = nome.Split('_');

        if (parti.Length > 1 &&
            int.TryParse(parti[parti.Length - 1], out int numero))
        {
            return numero;
        }

        return 0;
    }
}

public static class OmbraDinamica2D
{
    private static Sprite spriteOmbra;

    public static SpriteRenderer Crea(
        Transform genitore,
        SpriteRenderer riferimento,
        Vector2 posizioneLocale,
        Vector2 dimensioni
    )
    {
        if (genitore == null || riferimento == null) return null;

        GameObject oggettoOmbra = new GameObject("Ombra");
        oggettoOmbra.layer = genitore.gameObject.layer;
        oggettoOmbra.transform.SetParent(genitore, false);
        oggettoOmbra.transform.localPosition = posizioneLocale;

        SpriteRenderer renderer = oggettoOmbra.AddComponent<SpriteRenderer>();
        renderer.sprite = OttieniSpriteOmbra();
        renderer.color = new Color(0.08f, 0.045f, 0.025f, 0.38f);
        renderer.sortingLayerID = riferimento.sortingLayerID;
        renderer.sortingOrder = riferimento.sortingOrder - 1;
        renderer.transform.localScale = new Vector3(
            dimensioni.x,
            dimensioni.y * 2f,
            1f
        );
        return renderer;
    }

    private static Sprite OttieniSpriteOmbra()
    {
        if (spriteOmbra != null) return spriteOmbra;

        const int larghezza = 64;
        const int altezza = 32;
        Texture2D texture = new Texture2D(
            larghezza,
            altezza,
            TextureFormat.RGBA32,
            false
        );
        texture.name = "TextureOmbraMorbida";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixel = new Color[larghezza * altezza];
        for (int y = 0; y < altezza; y++)
        {
            for (int x = 0; x < larghezza; x++)
            {
                float nx = (x + 0.5f) / larghezza * 2f - 1f;
                float ny = (y + 0.5f) / altezza * 2f - 1f;
                float distanza = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = Mathf.SmoothStep(1f, 0f, distanza);
                pixel[y * larghezza + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixel);
        texture.Apply(false, true);

        spriteOmbra = Sprite.Create(
            texture,
            new Rect(0f, 0f, larghezza, altezza),
            new Vector2(0.5f, 0.5f),
            larghezza,
            0,
            SpriteMeshType.FullRect
        );
        spriteOmbra.name = "SpriteOmbraMorbida";
        return spriteOmbra;
    }
}
