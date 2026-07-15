using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisualController : MonoBehaviour
{
    public float frameAlSecondo = 6f;

    [Header("Movimento visivo")]
    [Min(0f)] public float ampiezzaIdle = 0.02f;
    [Min(0f)] public float frequenzaIdle = 1.2f;
    [Min(0f)] public float ampiezzaCamminata = 0.08f;
    [Min(0f)] public float frequenzaCamminata = 3f;
    [Min(0f)] public float velocitaTransizione = 12f;

    [Header("Feedback sparo")]
    [Min(0.01f)] public float durataFeedbackSparo = 0.1f;
    [Min(0f)] public float distanzaRinculo = 0.08f;
    [Range(0f, 0.1f)] public float compressioneSparo = 0.035f;

    private SpriteRenderer spriteRenderer;
    private Transform grafica;
    private PlayerMovement movimento;
    private PlayerShooting sparo;
    private Sprite[] frameCamminata;

    private int direzioneCorrente = 0;
    private float timerAnimazione;
    private float faseIdle;
    private float faseCamminata;
    private float blendCamminata;
    private bool camminavaNelFramePrecedente;
    private float tempoFeedbackSparo;
    private Vector2 direzioneRinculo = Vector2.down;

    void Awake()
    {
        spriteRenderer = CreaRendererVisivo(GetComponent<SpriteRenderer>());
        movimento = GetComponent<PlayerMovement>();
        sparo = GetComponent<PlayerShooting>();

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
            new Vector2(0f, -0.42f),
            new Vector2(0.66f, 0.22f)
        );
    }

    void LateUpdate()
    {
        if (frameCamminata.Length < 16 || movimento == null)
        {
            AggiornaMovimentoVisivo(false, 1f);
            return;
        }

        Vector2 direzione = movimento.VelocitaAttuale;
        bool staCamminando = movimento.StaCamminando;

        Vector2 direzioneMira;
        if (sparo != null &&
            sparo.ProvaOttieniDirezioneMira(out direzioneMira))
        {
            direzioneCorrente = OttieniDirezione(direzioneMira);
        }
        else if (staCamminando)
        {
            direzioneCorrente = OttieniDirezione(direzione.normalized);
        }

        if (staCamminando)
        {
            float cadenza = Mathf.Clamp(
                direzione.magnitude / Mathf.Max(0.01f, movimento.speed),
                0.35f,
                2f
            );
            timerAnimazione += Time.deltaTime * cadenza;
        }
        else
        {
            timerAnimazione = 0f;
        }

        int passo = staCamminando
            ? Mathf.FloorToInt(timerAnimazione * frameAlSecondo) % 2
            : 0;

        int indiceSprite = direzioneCorrente * 2 + passo;
        spriteRenderer.sprite = frameCamminata[indiceSprite];

        float fattoreCadenza = Mathf.Clamp(
            direzione.magnitude / Mathf.Max(0.01f, movimento.speed),
            0.35f,
            2f
        );
        AggiornaMovimentoVisivo(staCamminando, fattoreCadenza);
    }

    public void RiproduciFeedbackSparo(Vector2 direzione)
    {
        if (direzione.sqrMagnitude <= 0.0001f) return;

        direzioneRinculo = direzione.normalized;
        direzioneCorrente = OttieniDirezione(direzioneRinculo);
        tempoFeedbackSparo = Mathf.Max(0.01f, durataFeedbackSparo);
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

    void AggiornaMovimentoVisivo(bool staCamminando, float fattoreCadenza)
    {
        if (grafica == null) return;

        faseIdle = Mathf.Repeat(
            faseIdle + Time.deltaTime * frequenzaIdle * Mathf.PI * 2f,
            Mathf.PI * 2f
        );

        if (staCamminando && !camminavaNelFramePrecedente)
        {
            faseCamminata = 0f;
        }

        if (staCamminando)
        {
            faseCamminata = Mathf.Repeat(
                faseCamminata +
                Time.deltaTime * frequenzaCamminata *
                Mathf.Clamp(fattoreCadenza, 0.5f, 2f) * Mathf.PI * 2f,
                Mathf.PI * 2f
            );
        }

        float transizione = 1f - Mathf.Exp(-velocitaTransizione * Time.deltaTime);
        blendCamminata = Mathf.Lerp(
            blendCamminata,
            staCamminando ? 1f : 0f,
            transizione
        );

        float offsetIdle = Mathf.Sin(faseIdle) * ampiezzaIdle;
        float offsetCamminata =
            (1f - Mathf.Cos(faseCamminata)) * 0.5f * ampiezzaCamminata;

        float offsetVerticale = Mathf.Lerp(
            offsetIdle,
            offsetCamminata,
            blendCamminata
        );

        float forzaSparo = 0f;
        if (tempoFeedbackSparo > 0f)
        {
            float durata = Mathf.Max(0.01f, durataFeedbackSparo);
            forzaSparo = Mathf.Clamp01(tempoFeedbackSparo / durata);
            forzaSparo *= forzaSparo;
            tempoFeedbackSparo = Mathf.Max(
                0f,
                tempoFeedbackSparo - Time.unscaledDeltaTime
            );
        }

        Vector2 offsetRinculo =
            -direzioneRinculo * distanzaRinculo * forzaSparo;
        grafica.localPosition = new Vector3(
            offsetRinculo.x,
            offsetVerticale + offsetRinculo.y,
            0f
        );

        float compressione = compressioneSparo * forzaSparo;
        grafica.localScale = new Vector3(
            1f + compressione * 0.55f,
            1f - compressione,
            1f
        );
        camminavaNelFramePrecedente = staCamminando;
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
