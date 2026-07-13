using UnityEngine;
using System.Collections;
using TMPro;

[System.Serializable]
public class Wave
{
    public string nomeOndata;
    public int numeroNemici;
    public float intervalloTraNemici;
}

public class EnemySpawner : MonoBehaviour
{
    public GameObject foxPrefab;
    public float spawnDistance = 10f;

    [Header("Vita nemici")]
    [Min(1)] public int vitaPrimaOndata = 2;
    [Min(0)] public int vitaAggiuntivaPerOndata = 1;

    public Wave[] ondate;
    public float tempoTraOndate = 3f;

    private int currentWaveIndex = 0;
    private TMP_Text testoOndata;
    private TMP_Text messaggioOndata;

    void Start()
    {
        testoOndata = GameObject.Find("OndataText")?.GetComponent<TMP_Text>();
        messaggioOndata = GameObject.Find("MessaggioOndataText")?.GetComponent<TMP_Text>();

        if (messaggioOndata != null)
        {
            messaggioOndata.gameObject.SetActive(false);
        }

        StartCoroutine(GestoreOndate());
    }

    IEnumerator GestoreOndate()
    {
        while (currentWaveIndex < ondate.Length)
        {
            if (GameManager.instance != null && GameManager.instance.isGameOver)
            {
                yield break;
            }

            AggiornaContatoreOndata();

            Wave ondataCorrente = ondate[currentWaveIndex];

            for (int i = 0; i < ondataCorrente.numeroNemici; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(ondataCorrente.intervalloTraNemici);
            }

            while (GameObject.FindGameObjectWithTag("Nemico") != null)
            {
                if (GameManager.instance != null && GameManager.instance.isGameOver)
                {
                    yield break;
                }

                yield return new WaitForSeconds(1f);
            }

            if (currentWaveIndex < ondate.Length - 1)
            {
                yield return StartCoroutine(ContoAllaRovescia());
            }

            currentWaveIndex++;
        }

        MostraMessaggio("VITTORIA!\nHai difeso tutte le uova!");

        if (GameManager.instance != null)
        {
            GameManager.instance.Vittoria();
        }
    }

    IEnumerator ContoAllaRovescia()
    {
        for (int secondi = Mathf.CeilToInt(tempoTraOndate); secondi > 0; secondi--)
        {
            MostraMessaggio(
                "ONDATA " + (currentWaveIndex + 1) + " COMPLETATA!\n" +
                "Prossima ondata tra: " + secondi
            );

            yield return new WaitForSeconds(1f);
        }

        NascondiMessaggio();
    }

    void AggiornaContatoreOndata()
    {
        if (testoOndata != null)
        {
            testoOndata.text = "ONDATA: " + (currentWaveIndex + 1) + "/" + ondate.Length;
        }
    }

    void MostraMessaggio(string testo)
    {
        if (messaggioOndata != null)
        {
            messaggioOndata.gameObject.SetActive(true);
            messaggioOndata.text = testo;
        }
    }

    void NascondiMessaggio()
    {
        if (messaggioOndata != null)
        {
            messaggioOndata.gameObject.SetActive(false);
        }
    }

    void SpawnEnemy()
    {
        if (foxPrefab == null) return;

        Vector2 spawnPos = Random.insideUnitCircle.normalized * spawnDistance;
        GameObject nuovaVolpe = Instantiate(foxPrefab, spawnPos, Quaternion.identity);

        EnemyAI nemico = nuovaVolpe.GetComponent<EnemyAI>();
        if (nemico == null)
        {
            Debug.LogError("Il prefab della volpe non contiene EnemyAI.", nuovaVolpe);
            Destroy(nuovaVolpe);
            return;
        }

        int vitaOndata = Mathf.Max(
            1,
            vitaPrimaOndata + currentWaveIndex * vitaAggiuntivaPerOndata
        );
        nemico.InizializzaVita(vitaOndata);
    }
}
