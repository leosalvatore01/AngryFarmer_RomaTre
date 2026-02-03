using UnityEngine;
using System.Collections;

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
    
    // Lista delle ondate configurabile da Inspector
    public Wave[] ondate; 
    public float tempoTraOndate = 5f;

    private int currentWaveIndex = 0;

    void Start()
    {
        StartCoroutine(GestoreOndate());
    }

    IEnumerator GestoreOndate()
    {
        // Cicla attraverso tutte le ondate che abbiamo impostato
        while (currentWaveIndex < ondate.Length)
        {
            Wave ondataCorrente = ondate[currentWaveIndex];
            Debug.Log("Inizia: " + ondataCorrente.nomeOndata);

            // 1. Spawna i nemici di questa ondata
            for (int i = 0; i < ondataCorrente.numeroNemici; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(ondataCorrente.intervalloTraNemici);
            }

            // 2. Aspetta che il giocatore uccida tutti i nemici prima di passare alla prossima
            // (Controlla ogni secondo se ci sono ancora nemici vivi)
            while (GameObject.FindGameObjectWithTag("Nemico") != null)
            {
                yield return new WaitForSeconds(1f);
            }

            Debug.Log("Ondata completata!");
            
            // 3. Pausa meritata prima della prossima ondata
            yield return new WaitForSeconds(tempoTraOndate);

            currentWaveIndex++;
        }

        // Se esce dal ciclo, le ondate sono finite -> VITTORIA
        if (GameManager.instance != null)
        {
            GameManager.instance.Vittoria();
        }
    }

    void SpawnEnemy()
    {
        if (foxPrefab == null) return;
        Vector2 spawnPos = Random.insideUnitCircle.normalized * spawnDistance;
        Instantiate(foxPrefab, spawnPos, Quaternion.identity);
    }
}
