using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class DamageNumberFeedback : MonoBehaviour
{
    private const int LimitePool = 28;
    private readonly Queue<DamageNumberPopup> pool =
        new Queue<DamageNumberPopup>();

    public static DamageNumberFeedback Instance { get; private set; }
    public int NumeriMostrati { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatici()
    {
        Instance = null;
    }

    public static void Mostra(
        Vector2 posizione,
        int danno,
        bool critico = false
    )
    {
        MostraInterno(posizione, danno, critico, false);
    }

    public static void MostraGiocatore(Vector2 posizione, int danno)
    {
        MostraInterno(posizione, danno, false, true);
    }

    private static void MostraInterno(
        Vector2 posizione,
        int danno,
        bool critico,
        bool giocatore
    )
    {
        if (danno <= 0) return;
        GameOptionsController opzioni = GameOptionsController.Instance;
        if (opzioni != null && !opzioni.NumeriDannoAttivi) return;

        DamageNumberFeedback controller = CreaOTrova();
        controller.Emetti(posizione, danno, critico, giocatore);
    }

    private static DamageNumberFeedback CreaOTrova()
    {
        if (Instance != null) return Instance;
        DamageNumberFeedback esistente =
            FindFirstObjectByType<DamageNumberFeedback>();
        if (esistente != null)
        {
            Instance = esistente;
            return esistente;
        }
        GameObject oggetto = new GameObject("NumeriDanno");
        return oggetto.AddComponent<DamageNumberFeedback>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Emetti(
        Vector2 posizione,
        int danno,
        bool critico,
        bool giocatore
    )
    {
        DamageNumberPopup popup = null;
        while (pool.Count > 0 && popup == null)
        {
            popup = pool.Dequeue();
        }
        if (popup == null)
        {
            GameObject oggetto = new GameObject("NumeroDannoPixel");
            oggetto.transform.SetParent(transform, false);
            popup = oggetto.AddComponent<DamageNumberPopup>();
            popup.Configura(this);
        }

        popup.gameObject.SetActive(true);
        popup.Attiva(posizione, danno, critico, giocatore);
        NumeriMostrati++;
    }

    internal void Rilascia(DamageNumberPopup popup)
    {
        if (popup == null) return;
        popup.gameObject.SetActive(false);
        if (pool.Count < LimitePool)
        {
            pool.Enqueue(popup);
        }
        else
        {
            Destroy(popup.gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

internal sealed class DamageNumberPopup : MonoBehaviour
{
    private DamageNumberFeedback proprietario;
    private TextMeshPro testo;
    private Vector2 velocita;
    private float tempo;
    private float durata;
    private Color coloreBase;

    public void Configura(DamageNumberFeedback nuovoProprietario)
    {
        proprietario = nuovoProprietario;
        testo = gameObject.AddComponent<TextMeshPro>();
        testo.alignment = TextAlignmentOptions.Center;
        testo.fontStyle = FontStyles.Bold;
        testo.fontSize = 4.2f;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.rectTransform.sizeDelta = new Vector2(3.5f, 1.2f);
        FarmPixelUI.ApplicaTestoMondo(testo, Color.white);
        MeshRenderer renderer = testo.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.sortingOrder = 320;
    }

    public void Attiva(
        Vector2 posizione,
        int danno,
        bool critico,
        bool giocatore
    )
    {
        transform.position = posizione + new Vector2(
            Random.Range(-0.12f, 0.12f),
            giocatore ? 0.72f : 0.5f
        );
        transform.localScale = Vector3.one * (critico ? 0.19f : 0.16f);
        tempo = 0f;
        durata = critico ? 0.82f : 0.68f;
        velocita = new Vector2(
            Random.Range(-0.16f, 0.16f),
            critico ? 1.15f : 0.88f
        );

        if (giocatore)
        {
            testo.text = "-" + danno;
            coloreBase = new Color(1f, 0.32f, 0.24f, 1f);
        }
        else if (critico)
        {
            testo.text = danno + "!";
            coloreBase = new Color(1f, 0.88f, 0.24f, 1f);
        }
        else
        {
            testo.text = danno.ToString();
            coloreBase = new Color(1f, 0.95f, 0.72f, 1f);
        }
        testo.color = coloreBase;
    }

    void Update()
    {
        if (GameManager.instance != null &&
            GameManager.instance.PausaManualeAttiva)
        {
            return;
        }

        float delta = Time.deltaTime;
        tempo += delta;
        transform.position += (Vector3)(velocita * delta);
        velocita *= Mathf.Exp(-2.8f * delta);
        float progresso = Mathf.Clamp01(tempo / Mathf.Max(0.01f, durata));
        Color colore = coloreBase;
        colore.a = 1f - Mathf.SmoothStep(0f, 1f, progresso);
        testo.color = colore;

        if (tempo >= durata)
        {
            proprietario.Rilascia(this);
        }
    }
}
