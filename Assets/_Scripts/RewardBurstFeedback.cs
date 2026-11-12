using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Solo presentazione: le monete reali sono gia' accreditate alla morte.</summary>
public sealed class RewardBurstFeedback : MonoBehaviour
{
    private const int NumeroMonete = 3;
    private static Sprite spritePolvere;
    private readonly SpriteRenderer[] monete = new SpriteRenderer[NumeroMonete];
    private readonly SpriteRenderer[] polvere = new SpriteRenderer[3];
    private Transform destinazione;
    private Vector2 origine;
    private Vector2 ultimaDestinazione;
    private int scena;
    private float tempo;
    private float scalaMoneta;

    public float Progresso => Mathf.Clamp01(tempo / 0.58f);

    void Awake()
    {
        Sprite spriteMoneta = FarmPixelUI.OttieniIcona(FarmPixelIcon.Moneta);
        scalaMoneta = 0.27f / Mathf.Max(0.01f, spriteMoneta.bounds.size.x);
        Sprite dust = OttieniPolvere();
        for (int i = 0; i < NumeroMonete; i++)
        {
            monete[i] = CreaRenderer("MonetaCosmetica_" + i, spriteMoneta);
            polvere[i] = CreaRenderer("Polvere_" + i, dust);
        }
    }

    private SpriteRenderer CreaRenderer(string nome, Sprite sprite)
    {
        GameObject oggetto = new GameObject(nome, typeof(SpriteRenderer));
        oggetto.transform.SetParent(transform, false);
        SpriteRenderer renderer = oggetto.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.enabled = false;
        return renderer;
    }

    public void Attiva(Vector2 posizione, Transform giocatore, int quantita, int layer, int ordine)
    {
        origine = posizione;
        destinazione = giocatore;
        ultimaDestinazione = giocatore != null ? (Vector2)giocatore.position : posizione;
        scena = SceneManager.GetActiveScene().handle;
        tempo = 0f;
        transform.position = posizione;
        bool leggera = MobilePerformance.ModalitaLeggera;
        int numero = Mathf.Clamp(quantita, 0, leggera ? 1 : NumeroMonete);
        for (int i = 0; i < NumeroMonete; i++)
        {
            monete[i].enabled = i < numero;
            monete[i].sortingLayerID = layer;
            monete[i].sortingOrder = ordine + 8;
            monete[i].color = Color.white;
            monete[i].transform.localPosition = Vector3.zero;
            monete[i].transform.localScale = Vector3.one * scalaMoneta;
            polvere[i].enabled = !leggera || i == 1;
            polvere[i].sortingLayerID = layer;
            polvere[i].sortingOrder = ordine - 2;
        }
        gameObject.SetActive(true);
        AggiornaAspetto();
    }

    void Update()
    {
        CombatFeedbackController feedback = CombatFeedbackController.Instance;
        GameManager gestore = GameManager.instance;
        if (SceneManager.GetActiveScene().handle != scena ||
            (feedback != null && !feedback.VfxAbilitati) ||
            (gestore != null && (gestore.isGameOver || gestore.StatoCorrente != StatoPartita.Onda)))
        {
            gameObject.SetActive(false);
            return;
        }
        // La stessa scala temporale del gameplay mantiene fermi gli effetti in pausa.
        tempo += Time.deltaTime;
        if (destinazione != null) ultimaDestinazione = destinazione.position;
        AggiornaAspetto();
        if (tempo >= 0.58f) gameObject.SetActive(false);
    }

    private void AggiornaAspetto()
    {
        for (int i = 0; i < NumeroMonete; i++)
        {
            float ritardo = i * 0.035f;
            float volo = Mathf.Clamp01((tempo - ritardo) / 0.48f);
            float attrazione = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((volo - 0.23f) / 0.77f));
            Vector2 ventaglio = new Vector2((i - 1) * 0.3f, 0.72f + i * 0.08f);
            Vector2 salto = origine + ventaglio * Mathf.Sin(volo * Mathf.PI);
            monete[i].transform.position = Vector2.Lerp(salto, ultimaDestinazione, attrazione);
            float rotazione = 0.4f + 0.6f * Mathf.Abs(Mathf.Cos(volo * Mathf.PI * 3f + i));
            monete[i].transform.localScale = new Vector3(scalaMoneta * rotazione, scalaMoneta, 1f);
            Color colore = Color.white;
            colore.a = 1f - Mathf.SmoothStep(0.8f, 1f, volo);
            monete[i].color = colore;

            float nube = Mathf.Clamp01(tempo / 0.25f);
            polvere[i].transform.localPosition = new Vector3((i - 1) * (0.12f + nube * 0.22f), -0.25f + nube * 0.09f, 0f);
            polvere[i].transform.localScale = new Vector3(0.8f + nube * 1.1f, 0.7f + nube * 0.55f, 1f);
            polvere[i].color = new Color(0.78f, 0.7f, 0.48f, (1f - nube) * 0.42f);
        }
    }

    private static Sprite OttieniPolvere()
    {
        if (spritePolvere != null) return spritePolvere;
        const int w = 16, h = 8;
        var pixel = new Color32[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float nx = (x - 7.5f) / 7.5f;
                float ny = (y - 3.5f) / 3.5f;
                if (nx * nx + ny * ny < 0.9f) pixel[y * w + x] = new Color32(255, 255, 255, 255);
            }
        Texture2D texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        texture.name = "PolvereRicompensaRuntime";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels32(pixel);
        texture.Apply(false, true);
        spritePolvere = Sprite.Create(texture, new Rect(0, 0, w, h), Vector2.one * 0.5f, 48f);
        spritePolvere.hideFlags = HideFlags.HideAndDontSave;
        return spritePolvere;
    }

    public static void LiberaRisorse()
    {
        if (spritePolvere == null) return;
        Destroy(spritePolvere.texture);
        Destroy(spritePolvere);
        spritePolvere = null;
    }
}
