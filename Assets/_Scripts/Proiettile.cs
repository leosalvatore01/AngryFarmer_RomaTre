using System.Collections.Generic;
using UnityEngine;

public class Proiettile : MonoBehaviour
{
    [Min(1)] public int danno = 1;
    [Min(0f)] public float rotazioneVisivaMinima = 100f;
    [Min(0f)] public float rotazioneVisivaMassima = 165f;

    private readonly HashSet<int> bersagliColpiti = new HashSet<int>();
    private int penetrazioniRimaste;
    private Transform grafica;
    private float velocitaRotazioneVisiva;
    private float durataVita = 3f;

    private bool consumato;

    void Awake()
    {
        PlayerBalanceSettings bilanciamento =
            GameBalanceConfig.Corrente.Giocatore;
        danno = Mathf.Max(1, bilanciamento.dannoProiettile);
        penetrazioniRimaste = Mathf.Max(
            0,
            bilanciamento.penetrazioneProiettile
        );
        durataVita = Mathf.Max(0.05f, bilanciamento.durataProiettile);

        ConfiguraGraficaRotante();

        float minimo = Mathf.Min(rotazioneVisivaMinima, rotazioneVisivaMassima);
        float massimo = Mathf.Max(rotazioneVisivaMinima, rotazioneVisivaMassima);
        velocitaRotazioneVisiva = Random.Range(minimo, massimo) *
                                  (Random.value < 0.5f ? -1f : 1f);
    }

    public void Inizializza(int nuovoDanno, int penetrazioni)
    {
        danno = Mathf.Max(1, nuovoDanno);
        penetrazioniRimaste = Mathf.Max(0, penetrazioni);
    }

    void Start()
    {
        Destroy(gameObject, durataVita);
    }

    void Update()
    {
        if (grafica != null)
        {
            grafica.Rotate(
                0f,
                0f,
                velocitaRotazioneVisiva * Time.deltaTime,
                Space.Self
            );
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (consumato) return;

        IDanneggiabile bersaglio = other.GetComponentInParent<IDanneggiabile>();
        if (bersaglio == null) return;

        Component componenteBersaglio = bersaglio as Component;
        int idBersaglio = componenteBersaglio != null
            ? componenteBersaglio.gameObject.GetInstanceID()
            : other.GetInstanceID();

        if (!bersagliColpiti.Add(idBersaglio)) return;

        EsitoDanno esito = bersaglio.ProvaSubireDanno(danno);
        if (!esito.Applicato) return;

        bool puoPenetrare =
            esito.Ucciso &&
            esito.ConsentePenetrazioneAllaMorte &&
            penetrazioniRimaste > 0;

        if (puoPenetrare)
        {
            penetrazioniRimaste--;
            return;
        }

        consumato = true;
        Destroy(gameObject);
    }

    void ConfiguraGraficaRotante()
    {
        SpriteRenderer rendererOriginale = GetComponent<SpriteRenderer>();
        if (rendererOriginale == null)
        {
            SpriteRenderer rendererFiglio =
                GetComponentInChildren<SpriteRenderer>();
            grafica = rendererFiglio != null
                ? rendererFiglio.transform
                : null;
            return;
        }

        GameObject oggettoGrafico = new GameObject("GraficaProiettile");
        oggettoGrafico.layer = gameObject.layer;
        grafica = oggettoGrafico.transform;
        grafica.SetParent(transform, false);

        SpriteRenderer renderer = oggettoGrafico.AddComponent<SpriteRenderer>();
        renderer.sprite = rendererOriginale.sprite;
        renderer.color = rendererOriginale.color;
        renderer.flipX = rendererOriginale.flipX;
        renderer.flipY = rendererOriginale.flipY;
        renderer.drawMode = rendererOriginale.drawMode;
        renderer.size = rendererOriginale.size;
        renderer.maskInteraction = rendererOriginale.maskInteraction;
        renderer.spriteSortPoint = rendererOriginale.spriteSortPoint;
        renderer.sortingLayerID = rendererOriginale.sortingLayerID;
        renderer.sortingOrder = rendererOriginale.sortingOrder;
        renderer.sharedMaterials = rendererOriginale.sharedMaterials;
        renderer.enabled = rendererOriginale.enabled;

        rendererOriginale.enabled = false;
    }
}
