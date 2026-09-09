using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolableGameplayObject
{
    void PreparaUscitaDalPool();
    void PreparaRientroNelPool();
}

public readonly struct GameplayPoolSnapshot
{
    public int PoolAttivi { get; }
    public int OggettiInUso { get; }
    public int OggettiDisponibili { get; }
    public int OggettiCreati { get; }
    public int Riutilizzi { get; }
    public int Rilasci { get; }
    public int DistruzioniPerLimite { get; }

    public GameplayPoolSnapshot(
        int poolAttivi,
        int oggettiInUso,
        int oggettiDisponibili,
        int oggettiCreati,
        int riutilizzi,
        int rilasci,
        int distruzioniPerLimite
    )
    {
        PoolAttivi = poolAttivi;
        OggettiInUso = oggettiInUso;
        OggettiDisponibili = oggettiDisponibili;
        OggettiCreati = oggettiCreati;
        Riutilizzi = riutilizzi;
        Rilasci = rilasci;
        DistruzioniPerLimite = distruzioniPerLimite;
    }
}

internal sealed class GameplayPoolMarker : MonoBehaviour
{
    public string chiave;
    public Vector3 scalaOriginale;
    public bool inUso;

    void OnDestroy()
    {
        GameplayObjectPool.NotificaDistruzione(this);
    }
}

/// <summary>
/// Pool centrale per le entita ad alta frequenza. Supporta sia prefab sia
/// oggetti procedurali e conserva metriche utili per verificare le ondate alte.
/// </summary>
public static class GameplayObjectPool
{
    private sealed class PoolEntry
    {
        public string chiave;
        public GameObject prefab;
        public Func<GameObject> fabbrica;
        public int limite;
        public int inUso;
        public readonly Queue<GameObject> disponibili =
            new Queue<GameObject>();
    }

    private static readonly Dictionary<string, PoolEntry> pool =
        new Dictionary<string, PoolEntry>();
    private static Transform radice;
    private static int creati;
    private static int riutilizzi;
    private static int rilasci;
    private static int distruzioniPerLimite;

    public static GameplayPoolSnapshot Diagnostica
    {
        get
        {
            int inUso = 0;
            int disponibili = 0;
            foreach (PoolEntry entry in pool.Values)
            {
                inUso += Mathf.Max(0, entry.inUso);
                disponibili += entry.disponibili.Count;
            }
            return new GameplayPoolSnapshot(
                pool.Count,
                inUso,
                disponibili,
                creati,
                riutilizzi,
                rilasci,
                distruzioniPerLimite
            );
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraRuntime()
    {
        pool.Clear();
        radice = null;
        creati = 0;
        riutilizzi = 0;
        rilasci = 0;
        distruzioniPerLimite = 0;
    }

    public static GameObject Spawn(
        GameObject prefab,
        Vector3 posizione,
        Quaternion rotazione,
        int limite,
        string variante = null
    )
    {
        if (prefab == null) return null;
        string chiave = "prefab:" + prefab.GetInstanceID() + ":" +
            (string.IsNullOrEmpty(variante) ? "base" : variante);
        PoolEntry entry = OttieniOCrea(
            chiave,
            prefab,
            null,
            limite
        );
        return Estrai(entry, posizione, rotazione);
    }

    public static GameObject SpawnProcedurale(
        string chiave,
        Func<GameObject> fabbrica,
        Vector3 posizione,
        Quaternion rotazione,
        int limite
    )
    {
        if (string.IsNullOrWhiteSpace(chiave) || fabbrica == null) return null;
        PoolEntry entry = OttieniOCrea(
            "runtime:" + chiave,
            null,
            fabbrica,
            limite
        );
        return Estrai(entry, posizione, rotazione);
    }

    public static GameObject SpawnDalloStessoPool(
        GameObject sorgente,
        Vector3 posizione,
        Quaternion rotazione
    )
    {
        if (sorgente == null) return null;
        GameplayPoolMarker marker = sorgente.GetComponent<GameplayPoolMarker>();
        if (marker == null || string.IsNullOrEmpty(marker.chiave) ||
            !pool.TryGetValue(marker.chiave, out PoolEntry entry))
        {
            return UnityEngine.Object.Instantiate(
                sorgente,
                posizione,
                rotazione
            );
        }
        return Estrai(entry, posizione, rotazione);
    }

    public static bool Rilascia(GameObject oggetto)
    {
        if (oggetto == null) return false;
        GameplayPoolMarker marker = oggetto.GetComponent<GameplayPoolMarker>();
        if (marker == null || !marker.inUso ||
            !pool.TryGetValue(marker.chiave, out PoolEntry entry))
        {
            return false;
        }

        NotificaPoolable(oggetto, false);
        marker.inUso = false;
        entry.inUso = Mathf.Max(0, entry.inUso - 1);
        rilasci++;

        oggetto.SetActive(false);
        Transform contenitore = OttieniRadice();
        oggetto.transform.SetParent(contenitore, false);
        oggetto.transform.localPosition = Vector3.zero;
        oggetto.transform.localRotation = Quaternion.identity;
        oggetto.transform.localScale = marker.scalaOriginale;

        if (entry.disponibili.Count < entry.limite)
        {
            entry.disponibili.Enqueue(oggetto);
        }
        else
        {
            distruzioniPerLimite++;
            Distruggi(oggetto);
        }
        return true;
    }

    public static void RilasciaODistruggi(GameObject oggetto)
    {
        if (oggetto == null || Rilascia(oggetto)) return;
        Distruggi(oggetto);
    }

    public static void RilasciaTuttiAttivi<T>() where T : Component
    {
        T[] oggetti = UnityEngine.Object.FindObjectsByType<T>(
            FindObjectsSortMode.None
        );
        for (int i = 0; i < oggetti.Length; i++)
        {
            if (oggetti[i] != null)
                RilasciaODistruggi(oggetti[i].gameObject);
        }
    }

#if UNITY_EDITOR
    public static void AzzeraPerTest()
    {
        if (radice != null)
        {
            UnityEngine.Object.DestroyImmediate(radice.gameObject);
        }
        AzzeraRuntime();
    }
#endif

    private static PoolEntry OttieniOCrea(
        string chiave,
        GameObject prefab,
        Func<GameObject> fabbrica,
        int limite
    )
    {
        if (!pool.TryGetValue(chiave, out PoolEntry entry))
        {
            entry = new PoolEntry
            {
                chiave = chiave,
                prefab = prefab,
                fabbrica = fabbrica,
                limite = Mathf.Clamp(limite, 1, 512)
            };
            pool.Add(chiave, entry);
        }
        else
        {
            entry.limite = Mathf.Max(
                entry.limite,
                Mathf.Clamp(limite, 1, 512)
            );
        }
        return entry;
    }

    private static GameObject Estrai(
        PoolEntry entry,
        Vector3 posizione,
        Quaternion rotazione
    )
    {
        GameObject oggetto = null;
        while (entry.disponibili.Count > 0 && oggetto == null)
        {
            oggetto = entry.disponibili.Dequeue();
        }

        bool riutilizzato = oggetto != null;
        if (!riutilizzato)
        {
            oggetto = entry.prefab != null
                ? UnityEngine.Object.Instantiate(
                    entry.prefab,
                    posizione,
                    rotazione
                )
                : entry.fabbrica();
            if (oggetto == null) return null;

            GameplayPoolMarker nuovoMarker =
                oggetto.AddComponent<GameplayPoolMarker>();
            nuovoMarker.chiave = entry.chiave;
            nuovoMarker.scalaOriginale = entry.prefab != null
                ? entry.prefab.transform.localScale
                : oggetto.transform.localScale;
            creati++;
        }

        GameplayPoolMarker marker = oggetto.GetComponent<GameplayPoolMarker>();
        marker.inUso = true;
        entry.inUso++;

        oggetto.transform.SetParent(null, false);
        oggetto.transform.position = posizione;
        oggetto.transform.rotation = rotazione;
        oggetto.transform.localScale = marker.scalaOriginale;
        NotificaPoolable(oggetto, true);
        if (!oggetto.activeSelf) oggetto.SetActive(true);
        if (riutilizzato) riutilizzi++;
        return oggetto;
    }

    private static void NotificaPoolable(GameObject oggetto, bool uscita)
    {
        MonoBehaviour[] componenti = oggetto.GetComponents<MonoBehaviour>();
        for (int i = 0; i < componenti.Length; i++)
        {
            if (!(componenti[i] is IPoolableGameplayObject poolable))
                continue;
            if (uscita) poolable.PreparaUscitaDalPool();
            else poolable.PreparaRientroNelPool();
        }
    }

    private static Transform OttieniRadice()
    {
        if (radice != null) return radice;
        GameObject oggetto = new GameObject("GameplayObjectPool");
        radice = oggetto.transform;
        if (Application.isPlaying)
            UnityEngine.Object.DontDestroyOnLoad(oggetto);
        return radice;
    }

    internal static void NotificaDistruzione(GameplayPoolMarker marker)
    {
        if (marker == null || !marker.inUso) return;
        if (pool.TryGetValue(marker.chiave, out PoolEntry entry))
        {
            entry.inUso = Mathf.Max(0, entry.inUso - 1);
        }
        marker.inUso = false;
    }

    private static void Distruggi(GameObject oggetto)
    {
        if (oggetto == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEngine.Object.DestroyImmediate(oggetto);
            return;
        }
#endif
        UnityEngine.Object.Destroy(oggetto);
    }
}
