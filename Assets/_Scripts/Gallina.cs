using System.Collections.Generic;
using UnityEngine;

public enum StatoGallina
{
    Disponibile,
    Prenotata,
    Trasportata,
    Persa
}

public class Gallina : MonoBehaviour
{
    private static readonly HashSet<Gallina> attive =
        new HashSet<Gallina>();
    private static Sprite spriteGallina;
    private static Sprite spriteNido;

    private SpriteRenderer rendererGallina;
    private SpriteRenderer rendererNido;
    private Collider2D colliderGallina;
    private EnemyAI ladraPrenotata;
    private Vector3 posizioneCasa;
    private bool registrataNelManager;

    public static IEnumerable<Gallina> Attive => attive;
    public StatoGallina Stato { get; private set; } = StatoGallina.Disponibile;
    public bool Disponibile => Stato == StatoGallina.Disponibile;
    public bool Trasportata => Stato == StatoGallina.Trasportata;
    public EnemyAI LadraPrenotata => ladraPrenotata;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraRegistro()
    {
        attive.Clear();
    }

    void Awake()
    {
        posizioneCasa = transform.position;
        rendererGallina = GetComponent<SpriteRenderer>();
        if (rendererGallina == null)
        {
            rendererGallina = gameObject.AddComponent<SpriteRenderer>();
        }
        rendererGallina.sprite = OttieniSpriteGallina();
        rendererGallina.color = Color.white;

        colliderGallina = GetComponent<Collider2D>();
        if (colliderGallina == null)
        {
            BoxCollider2D nuovoCollider = gameObject.AddComponent<BoxCollider2D>();
            nuovoCollider.size = new Vector2(0.72f, 0.72f);
            colliderGallina = nuovoCollider;
        }
        colliderGallina.isTrigger = true;
        CreaNido();
    }

    void OnEnable()
    {
        if (Stato == StatoGallina.Persa) return;
        if (rendererGallina != null) rendererGallina.enabled = true;
        if (colliderGallina != null) colliderGallina.enabled = true;
        attive.Add(this);
    }

    void Start()
    {
        if (!registrataNelManager && GameManager.instance != null)
        {
            registrataNelManager = true;
            GameManager.instance.RegistraGallina();
        }
    }

    public bool ProvaPrenotare(EnemyAI ladra)
    {
        if (!isActiveAndEnabled || ladra == null ||
            Stato != StatoGallina.Disponibile)
        {
            return false;
        }
        Stato = StatoGallina.Prenotata;
        ladraPrenotata = ladra;
        return true;
    }

    public bool PrenotataDa(EnemyAI ladra)
    {
        return isActiveAndEnabled &&
               ladra != null &&
               ladraPrenotata == ladra &&
               (Stato == StatoGallina.Prenotata ||
                Stato == StatoGallina.Trasportata);
    }

    public bool ProvaPrelevare(EnemyAI ladra)
    {
        if (!PrenotataDa(ladra) || Stato != StatoGallina.Prenotata)
        {
            return false;
        }

        Stato = StatoGallina.Trasportata;
        if (rendererGallina != null) rendererGallina.enabled = false;
        if (colliderGallina != null) colliderGallina.enabled = false;
        return true;
    }

    public void Rilascia(EnemyAI ladra)
    {
        if (ladra == null || ladraPrenotata != ladra ||
            Stato == StatoGallina.Persa)
        {
            return;
        }

        Stato = StatoGallina.Disponibile;
        ladraPrenotata = null;
        transform.position = posizioneCasa;
        if (rendererGallina != null) rendererGallina.enabled = true;
        if (colliderGallina != null) colliderGallina.enabled = true;
        if (isActiveAndEnabled) attive.Add(this);
    }

    public bool ConfermaPerdita(EnemyAI ladra)
    {
        if (!PrenotataDa(ladra) || Stato != StatoGallina.Trasportata)
        {
            return false;
        }

        Stato = StatoGallina.Persa;
        ladraPrenotata = null;
        attive.Remove(this);
        if (registrataNelManager && GameManager.instance != null)
        {
            GameManager.instance.GallinaMorta();
        }
        Destroy(gameObject);
        return true;
    }

    private void CreaNido()
    {
        GameObject nido = new GameObject("NidoPixel");
        nido.layer = gameObject.layer;
        nido.transform.SetParent(transform, false);
        nido.transform.localPosition = new Vector3(0f, -0.34f, 0f);
        nido.transform.localScale = new Vector3(1.08f, 0.72f, 1f);
        rendererNido = nido.AddComponent<SpriteRenderer>();
        rendererNido.sprite = OttieniSpriteNido();
        rendererNido.sortingLayerID = rendererGallina.sortingLayerID;
        rendererNido.sortingOrder = rendererGallina.sortingOrder - 1;
    }

    private static Sprite OttieniSpriteGallina()
    {
        if (spriteGallina != null) return spriteGallina;

        const int larghezza = 16;
        const int altezza = 16;
        Color32[] pixel = new Color32[larghezza * altezza];
        Color32 contorno = new Color32(67, 40, 29, 255);
        Color32 piume = new Color32(251, 233, 183, 255);
        Color32 luce = new Color32(255, 249, 222, 255);
        Color32 rosso = new Color32(211, 59, 45, 255);
        Color32 arancio = new Color32(238, 151, 40, 255);
        Color32 occhio = new Color32(24, 19, 17, 255);

        Rettangolo(pixel, larghezza, altezza, 3, 3, 11, 10, contorno);
        Rettangolo(pixel, larghezza, altezza, 2, 5, 12, 9, contorno);
        Rettangolo(pixel, larghezza, altezza, 4, 4, 10, 9, piume);
        Rettangolo(pixel, larghezza, altezza, 3, 6, 11, 8, piume);
        Rettangolo(pixel, larghezza, altezza, 9, 8, 12, 12, contorno);
        Rettangolo(pixel, larghezza, altezza, 9, 9, 11, 12, luce);
        Imposta(pixel, larghezza, altezza, 10, 11, occhio);
        Imposta(pixel, larghezza, altezza, 13, 9, arancio);
        Imposta(pixel, larghezza, altezza, 14, 9, arancio);
        Imposta(pixel, larghezza, altezza, 9, 13, rosso);
        Imposta(pixel, larghezza, altezza, 10, 14, rosso);
        Imposta(pixel, larghezza, altezza, 11, 13, rosso);
        Imposta(pixel, larghezza, altezza, 5, 2, arancio);
        Imposta(pixel, larghezza, altezza, 9, 2, arancio);
        Imposta(pixel, larghezza, altezza, 4, 1, arancio);
        Imposta(pixel, larghezza, altezza, 10, 1, arancio);

        spriteGallina = CreaSprite("GallinaPixel", pixel, larghezza, altezza, 16f);
        return spriteGallina;
    }

    private static Sprite OttieniSpriteNido()
    {
        if (spriteNido != null) return spriteNido;
        const int larghezza = 16;
        const int altezza = 8;
        Color32[] pixel = new Color32[larghezza * altezza];
        Color32 scuro = new Color32(95, 57, 29, 255);
        Color32 paglia = new Color32(207, 145, 54, 255);
        Color32 luce = new Color32(241, 191, 80, 255);
        Rettangolo(pixel, larghezza, altezza, 2, 2, 13, 5, scuro);
        Rettangolo(pixel, larghezza, altezza, 1, 3, 14, 4, paglia);
        Rettangolo(pixel, larghezza, altezza, 4, 4, 11, 5, luce);
        Imposta(pixel, larghezza, altezza, 3, 6, luce);
        Imposta(pixel, larghezza, altezza, 12, 6, luce);
        spriteNido = CreaSprite("NidoGallinaPixel", pixel, larghezza, altezza, 16f);
        return spriteNido;
    }

    private static void Rettangolo(
        Color32[] pixel,
        int larghezza,
        int altezza,
        int xMin,
        int yMin,
        int xMax,
        int yMax,
        Color32 colore
    )
    {
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                Imposta(pixel, larghezza, altezza, x, y, colore);
            }
        }
    }

    private static void Imposta(
        Color32[] pixel,
        int larghezza,
        int altezza,
        int x,
        int y,
        Color32 colore
    )
    {
        if (x < 0 || x >= larghezza || y < 0 || y >= altezza) return;
        pixel[y * larghezza + x] = colore;
    }

    private static Sprite CreaSprite(
        string nome,
        Color32[] pixel,
        int larghezza,
        int altezza,
        float pixelPerUnita
    )
    {
        Texture2D texture = new Texture2D(
            larghezza,
            altezza,
            TextureFormat.RGBA32,
            false
        );
        texture.name = nome + "_Texture";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels32(pixel);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, larghezza, altezza),
            new Vector2(0.5f, 0.5f),
            pixelPerUnita,
            0,
            SpriteMeshType.FullRect
        );
        sprite.name = nome;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    void OnDisable()
    {
        attive.Remove(this);
        if (Stato == StatoGallina.Persa) return;

        EnemyAI ladra = ladraPrenotata;
        Stato = StatoGallina.Disponibile;
        ladraPrenotata = null;
        transform.position = posizioneCasa;
        if (rendererGallina != null) rendererGallina.enabled = true;
        if (colliderGallina != null) colliderGallina.enabled = true;
        if (ladra != null) ladra.NotificaGallinaNonDisponibile(this);
    }
}
