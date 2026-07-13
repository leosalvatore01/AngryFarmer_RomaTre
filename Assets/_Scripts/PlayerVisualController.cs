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

    private SpriteRenderer spriteRenderer;
    private Transform grafica;
    private PlayerMovement movimento;
    private Sprite[] frameCamminata;

    private int direzioneCorrente = 0;
    private float timerAnimazione;
    private float faseMovimento;
    private float offsetVerticale;

    void Awake()
    {
        spriteRenderer = CreaRendererVisivo(GetComponent<SpriteRenderer>());
        movimento = GetComponent<PlayerMovement>();

        frameCamminata = Resources.LoadAll<Sprite>("FarmerWalk-v1");

        Array.Sort(frameCamminata, (a, b) =>
            NumeroFrame(a.name).CompareTo(NumeroFrame(b.name))
        );

        if (frameCamminata.Length < 16)
        {
            Debug.LogError("FarmerWalk-v1 deve contenere 16 frame.");
        }
    }

    void Update()
    {
        if (frameCamminata.Length < 16 || movimento == null)
        {
            AggiornaMovimentoVisivo(false);
            return;
        }

        Vector2 direzione = movimento.DirezioneMovimento;
        bool staCamminando = direzione.sqrMagnitude > 0.01f;

        if (staCamminando)
        {
            direzioneCorrente = OttieniDirezione(direzione);
            timerAnimazione += Time.deltaTime;
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

        AggiornaMovimentoVisivo(staCamminando);
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

    void AggiornaMovimentoVisivo(bool staCamminando)
    {
        if (grafica == null) return;

        float frequenza = staCamminando ? frequenzaCamminata : frequenzaIdle;
        float ampiezza = staCamminando ? ampiezzaCamminata : ampiezzaIdle;

        faseMovimento = Mathf.Repeat(
            faseMovimento + Time.deltaTime * frequenza * Mathf.PI * 2f,
            Mathf.PI * 2f
        );

        float onda = staCamminando
            ? (1f - Mathf.Cos(faseMovimento)) * 0.5f
            : Mathf.Sin(faseMovimento);

        float offsetDesiderato = onda * ampiezza;
        float transizione = 1f - Mathf.Exp(-velocitaTransizione * Time.deltaTime);
        offsetVerticale = Mathf.Lerp(offsetVerticale, offsetDesiderato, transizione);

        grafica.localPosition = Vector3.up * offsetVerticale;
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
