using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[System.Serializable]
public class Wave
{
    public string nomeOndata;
    [Min(0)] public int numeroNemici;
    [Min(0.05f)] public float intervalloTraNemici = 1f;

    [Header("Bonus")]
    [Min(0)] public int numeroMaialiniBonus;
    [Min(1)] public int vitaMaialinoBonus = 1;
    [Min(0)] public int moneteMaialinoBonus = 3;
}

public class EnemySpawner : MonoBehaviour
{
    public GameObject foxPrefab;
    public GameObject pigPrefab;
    public float spawnDistance = 10f;
    public float distanzaSpawnMaialino = 5.2f;

    [Header("Vita nemici")]
    [Min(1)] public int vitaPrimaOndata = 2;
    [Min(0)] public int vitaAggiuntivaPerOndata = 1;

    public Wave[] ondate;

    private int currentWaveIndex = 0;
    private int tokenOndata;
    private TMP_Text testoOndata;
    private TMP_Text messaggioOndata;
    private GameObject contenitoreMessaggio;
    private Transform giocatore;

    void Start()
    {
        testoOndata = GameManager.TrovaTestoInterfaccia("OndataText");
        messaggioOndata =
            GameManager.TrovaTestoInterfaccia("MessaggioOndataText");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            giocatore = player.transform;
        }

        ConfiguraPannelloMessaggio();
        NascondiMessaggio();
        StartCoroutine(GestoreOndate());
    }

    IEnumerator GestoreOndate()
    {
        if (ondate == null || ondate.Length == 0)
        {
            yield break;
        }

        while (currentWaveIndex < ondate.Length)
        {
            if (PartitaTerminata())
            {
                PulisciEntitaTraOndate();
                yield break;
            }

            int tokenCorrente = ++tokenOndata;
            Wave ondataCorrente = ondate[currentWaveIndex];
            AggiornaContatoreOndata();

            MostraMessaggio(
                "Ondata " + (currentWaveIndex + 1) + " / " + ondate.Length +
                "\n" + ondataCorrente.nomeOndata
            );
            yield return new WaitForSeconds(0.85f);
            NascondiMessaggio();

            if (ondataCorrente.numeroMaialiniBonus > 0)
            {
                StartCoroutine(
                    SpawnMaialiniGraduali(ondataCorrente, tokenCorrente)
                );
            }

            for (int i = 0; i < ondataCorrente.numeroNemici; i++)
            {
                if (PartitaTerminata())
                {
                    PulisciEntitaTraOndate();
                    yield break;
                }

                SpawnEnemy();
                if (i < ondataCorrente.numeroNemici - 1)
                {
                    yield return new WaitForSeconds(
                        ondataCorrente.intervalloTraNemici
                    );
                }
            }

            while (GameObject.FindGameObjectWithTag("Nemico") != null)
            {
                if (PartitaTerminata())
                {
                    PulisciEntitaTraOndate();
                    yield break;
                }

                yield return new WaitForSeconds(0.2f);
            }

            if (currentWaveIndex < ondate.Length - 1)
            {
                tokenOndata++;
                PulisciEntitaTraOndate();

                // Lascia completare le distruzioni prima di fermare il tempo.
                yield return null;

                if (PartitaTerminata()) yield break;
                PulisciEntitaTraOndate();

                if (GameManager.instance != null)
                {
                    GameManager.instance.IniziaIntervallo(
                        currentWaveIndex + 1,
                        ondate.Length
                    );

                    while (!PartitaTerminata() &&
                           GameManager.instance.StatoCorrente ==
                           StatoPartita.Intervallo)
                    {
                        yield return null;
                    }
                }
            }

            currentWaveIndex++;
        }

        tokenOndata++;
        PulisciEntitaTraOndate();
        NascondiMessaggio();

        if (GameManager.instance != null)
        {
            GameManager.instance.Vittoria();
        }
    }

    IEnumerator SpawnMaialiniGraduali(Wave ondata, int tokenCorrente)
    {
        int quantita = Mathf.Max(0, ondata.numeroMaialiniBonus);
        float durataSpawnVolpi = Mathf.Max(
            1f,
            Mathf.Max(0, ondata.numeroNemici - 1) *
            ondata.intervalloTraNemici
        );
        float tempoPrecedente = 0f;

        for (int i = 0; i < quantita; i++)
        {
            float frazione = (i + 1f) / (quantita + 1f);
            float tempoSpawn = durataSpawnVolpi * frazione;
            yield return new WaitForSeconds(tempoSpawn - tempoPrecedente);
            tempoPrecedente = tempoSpawn;

            if (tokenCorrente != tokenOndata || PartitaTerminata())
            {
                yield break;
            }

            SpawnMaialino(ondata);
        }
    }

    void AggiornaContatoreOndata()
    {
        if (testoOndata != null)
        {
            testoOndata.text =
                "Ondata   " + (currentWaveIndex + 1) + " / " + ondate.Length;
        }
    }

    void ConfiguraPannelloMessaggio()
    {
        if (messaggioOndata == null) return;

        RectTransform testoRect = messaggioOndata.rectTransform;
        Transform canvas = testoRect.parent;
        int indice = testoRect.GetSiblingIndex();

        contenitoreMessaggio = new GameObject(
            "PannelloMessaggioOndata",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Shadow)
        );
        contenitoreMessaggio.transform.SetParent(canvas, false);
        contenitoreMessaggio.transform.SetSiblingIndex(indice);

        RectTransform pannelloRect =
            contenitoreMessaggio.GetComponent<RectTransform>();
        pannelloRect.anchorMin = new Vector2(0.5f, 0.5f);
        pannelloRect.anchorMax = new Vector2(0.5f, 0.5f);
        pannelloRect.pivot = new Vector2(0.5f, 0.5f);
        pannelloRect.anchoredPosition = Vector2.zero;
        pannelloRect.sizeDelta = new Vector2(570f, 126f);

        Image sfondo = contenitoreMessaggio.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, false, false);

        Shadow ombra = contenitoreMessaggio.GetComponent<Shadow>();
        ombra.effectColor = new Color(0.08f, 0.035f, 0.018f, 0.88f);
        ombra.effectDistance = new Vector2(5f, -5f);
        ombra.useGraphicAlpha = true;

        GameObject interno = new GameObject(
            "CorniceMessaggio",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        interno.transform.SetParent(pannelloRect, false);
        RectTransform internoRect = interno.GetComponent<RectTransform>();
        internoRect.anchorMin = Vector2.zero;
        internoRect.anchorMax = Vector2.one;
        internoRect.offsetMin = new Vector2(13f, 13f);
        internoRect.offsetMax = new Vector2(-13f, -13f);
        FarmPixelUI.ApplicaPannello(
            interno.GetComponent<Image>(),
            true,
            false
        );

        testoRect.SetParent(internoRect, false);
        testoRect.anchorMin = Vector2.zero;
        testoRect.anchorMax = Vector2.one;
        testoRect.pivot = new Vector2(0.5f, 0.5f);
        testoRect.anchoredPosition = Vector2.zero;
        testoRect.offsetMin = new Vector2(76f, 8f);
        testoRect.offsetMax = new Vector2(-76f, -8f);

        FarmPixelUI.AggiungiIcona(
            internoRect,
            "IconaOndataSinistra",
            FarmPixelIcon.Ondata,
            new Vector2(-226f, 0f),
            new Vector2(42f, 42f)
        );
        FarmPixelUI.AggiungiIcona(
            internoRect,
            "IconaOndataDestra",
            FarmPixelIcon.Ondata,
            new Vector2(226f, 0f),
            new Vector2(42f, 42f)
        );

        messaggioOndata.fontSize = 29f;
        messaggioOndata.fontStyle = FontStyles.Bold;
        messaggioOndata.alignment = TextAlignmentOptions.Center;
        FarmPixelUI.ApplicaTesto(
            messaggioOndata,
            new Color(1f, 0.88f, 0.56f, 1f)
        );
        messaggioOndata.textWrappingMode = TextWrappingModes.Normal;
        messaggioOndata.raycastTarget = false;
        messaggioOndata.gameObject.SetActive(true);
    }

    void MostraMessaggio(string testo)
    {
        if (messaggioOndata == null) return;

        if (contenitoreMessaggio != null)
        {
            contenitoreMessaggio.SetActive(true);
        }
        else
        {
            messaggioOndata.gameObject.SetActive(true);
        }

        messaggioOndata.text = testo;
    }

    void NascondiMessaggio()
    {
        if (contenitoreMessaggio != null)
        {
            contenitoreMessaggio.SetActive(false);
        }
        else if (messaggioOndata != null)
        {
            messaggioOndata.gameObject.SetActive(false);
        }
    }

    void SpawnEnemy()
    {
        if (foxPrefab == null) return;

        Vector2 spawnPos = DirezioneCasuale() * spawnDistance;
        GameObject nuovaVolpe =
            Instantiate(foxPrefab, spawnPos, Quaternion.identity);

        EnemyAI nemico = nuovaVolpe.GetComponent<EnemyAI>();
        if (nemico == null)
        {
            Debug.LogError(
                "Il prefab della volpe non contiene EnemyAI.",
                nuovaVolpe
            );
            Destroy(nuovaVolpe);
            return;
        }

        int vitaOndata = Mathf.Max(
            1,
            vitaPrimaOndata + currentWaveIndex * vitaAggiuntivaPerOndata
        );
        nemico.InizializzaVita(vitaOndata);
    }

    void SpawnMaialino(Wave ondata)
    {
        if (pigPrefab == null) return;

        Vector2 centro = giocatore != null
            ? (Vector2)giocatore.position
            : Vector2.zero;
        Vector2 spawnPos =
            centro + DirezioneCasuale() * distanzaSpawnMaialino;

        GameObject nuovoMaialino =
            Instantiate(pigPrefab, spawnPos, Quaternion.identity);
        MaialinoBonus bonus = nuovoMaialino.GetComponent<MaialinoBonus>();

        if (bonus == null)
        {
            Debug.LogError(
                "Il prefab del maialino non contiene MaialinoBonus.",
                nuovoMaialino
            );
            Destroy(nuovoMaialino);
            return;
        }

        bonus.Inizializza(
            ondata.vitaMaialinoBonus,
            ondata.moneteMaialinoBonus
        );
    }

    bool PartitaTerminata()
    {
        return GameManager.instance != null && GameManager.instance.isGameOver;
    }

    static void PulisciEntitaTraOndate()
    {
        Proiettile[] proiettili = FindObjectsByType<Proiettile>(
            FindObjectsSortMode.None
        );
        foreach (Proiettile proiettile in proiettili)
        {
            if (proiettile != null)
            {
                Destroy(proiettile.gameObject);
            }
        }

        MaialinoBonus.RimuoviTuttiSenzaPremio();
    }

    static Vector2 DirezioneCasuale()
    {
        Vector2 direzione = Random.insideUnitCircle;
        return direzione.sqrMagnitude > 0.001f
            ? direzione.normalized
            : Vector2.right;
    }

    void OnDisable()
    {
        tokenOndata++;
    }
}
