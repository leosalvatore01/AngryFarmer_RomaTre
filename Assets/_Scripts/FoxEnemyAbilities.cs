using UnityEngine;

/// <summary>
/// Effetti runtime semplici e leggibili per le abilita delle volpi speciali.
/// Non usa il flusso casuale globale: posizione, ritmo e lato restano
/// deterministici rispetto allo spawn che li ha generati.
/// </summary>
internal static class FoxAbilityVfx
{
    private static Sprite pixel;

    internal static Sprite Pixel
    {
        get
        {
            if (pixel != null) return pixel;
            Texture2D texture = Texture2D.whiteTexture;
            pixel = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                Mathf.Max(1, texture.width),
                0,
                SpriteMeshType.FullRect
            );
            pixel.name = "PixelAbilitaVolpi";
            pixel.hideFlags = HideFlags.HideAndDontSave;
            return pixel;
        }
    }

    public static void CreaAnello(
        Vector2 posizione,
        Color colore,
        float raggioIniziale,
        float raggioFinale,
        float durata,
        Transform segui
    )
    {
        GameObject oggetto = new GameObject("TelegraphAbilitaVolpe");
        oggetto.transform.position = posizione;
        FoxAbilityRingVfx effetto = oggetto.AddComponent<FoxAbilityRingVfx>();
        effetto.Inizializza(
            segui,
            colore,
            raggioIniziale,
            raggioFinale,
            durata,
            16
        );
    }

    public static void CreaLinea(
        Transform origine,
        Vector2 destinazione,
        Color colore,
        float durata
    )
    {
        if (origine == null) return;
        GameObject oggetto = new GameObject("MiraSputafango");
        oggetto.transform.position = origine.position;
        FoxAbilityLineVfx effetto = oggetto.AddComponent<FoxAbilityLineVfx>();
        effetto.Inizializza(origine, destinazione, colore, durata);
    }

    public static void CreaTracciaScavo(Vector2 posizione)
    {
        GameObject oggetto = new GameObject("TracciaScavatrice");
        oggetto.transform.position = posizione;
        FoxAbilityRingVfx effetto = oggetto.AddComponent<FoxAbilityRingVfx>();
        effetto.Inizializza(
            null,
            new Color32(157, 96, 40, 210),
            0.08f,
            0.48f,
            0.48f,
            10
        );
    }
}

internal sealed class FoxAbilityRingVfx : MonoBehaviour
{
    private SpriteRenderer[] punti;
    private Transform bersaglio;
    private Color colore;
    private float raggioIniziale;
    private float raggioFinale;
    private float durata;
    private float tempo;

    public void Inizializza(
        Transform nuovoBersaglio,
        Color nuovoColore,
        float nuovoRaggioIniziale,
        float nuovoRaggioFinale,
        float nuovaDurata,
        int numeroPunti
    )
    {
        bersaglio = nuovoBersaglio;
        colore = nuovoColore;
        raggioIniziale = Mathf.Max(0.01f, nuovoRaggioIniziale);
        raggioFinale = Mathf.Max(raggioIniziale, nuovoRaggioFinale);
        durata = Mathf.Max(0.05f, nuovaDurata);
        punti = new SpriteRenderer[Mathf.Clamp(numeroPunti, 6, 24)];

        for (int i = 0; i < punti.Length; i++)
        {
            GameObject punto = new GameObject("Punto_" + i);
            punto.transform.SetParent(transform, false);
            SpriteRenderer renderer = punto.AddComponent<SpriteRenderer>();
            renderer.sprite = FoxAbilityVfx.Pixel;
            renderer.sortingOrder = 32;
            renderer.color = colore;
            punti[i] = renderer;
        }
        AggiornaAspetto(0f);
    }

    void Update()
    {
        if (bersaglio != null) transform.position = bersaglio.position;
        tempo += Time.deltaTime;
        float t = Mathf.Clamp01(tempo / durata);
        AggiornaAspetto(t);
        if (tempo >= durata) Destroy(gameObject);
    }

    private void AggiornaAspetto(float t)
    {
        if (punti == null) return;
        float curva = t * t * (3f - 2f * t);
        float raggio = Mathf.Lerp(raggioIniziale, raggioFinale, curva);
        float alpha = Mathf.Clamp01(
            0.18f + Mathf.Sin(Mathf.PI * t) * 0.82f
        );
        float scala = Mathf.Lerp(0.1f, 0.055f, curva);

        for (int i = 0; i < punti.Length; i++)
        {
            float angolo = (i / (float)punti.Length) * Mathf.PI * 2f;
            SpriteRenderer renderer = punti[i];
            renderer.transform.localPosition = new Vector3(
                Mathf.Cos(angolo) * raggio,
                Mathf.Sin(angolo) * raggio * 0.64f,
                0f
            );
            renderer.transform.localScale = Vector3.one * scala;
            Color coloreCorrente = colore;
            coloreCorrente.a *= alpha;
            renderer.color = coloreCorrente;
        }
    }
}

internal sealed class FoxAbilityLineVfx : MonoBehaviour
{
    private const int NumeroPunti = 13;
    private SpriteRenderer[] punti;
    private Transform origine;
    private Vector2 destinazione;
    private Color colore;
    private float durata;
    private float tempo;

    public void Inizializza(
        Transform nuovaOrigine,
        Vector2 nuovaDestinazione,
        Color nuovoColore,
        float nuovaDurata
    )
    {
        origine = nuovaOrigine;
        destinazione = nuovaDestinazione;
        colore = nuovoColore;
        durata = Mathf.Max(0.05f, nuovaDurata);
        punti = new SpriteRenderer[NumeroPunti];
        for (int i = 0; i < punti.Length; i++)
        {
            GameObject punto = new GameObject("PuntoMira_" + i);
            punto.transform.SetParent(transform, false);
            SpriteRenderer renderer = punto.AddComponent<SpriteRenderer>();
            renderer.sprite = FoxAbilityVfx.Pixel;
            renderer.sortingOrder = 31;
            renderer.transform.localScale = Vector3.one * 0.065f;
            punti[i] = renderer;
        }
        AggiornaAspetto(0f);
    }

    void Update()
    {
        if (origine == null)
        {
            Destroy(gameObject);
            return;
        }
        tempo += Time.deltaTime;
        float t = Mathf.Clamp01(tempo / durata);
        AggiornaAspetto(t);
        if (tempo >= durata) Destroy(gameObject);
    }

    private void AggiornaAspetto(float t)
    {
        Vector2 partenza = origine != null
            ? (Vector2)origine.position
            : (Vector2)transform.position;
        transform.position = Vector3.zero;
        for (int i = 0; i < punti.Length; i++)
        {
            float frazione = (i + 1f) / (punti.Length + 1f);
            punti[i].transform.position = Vector2.Lerp(
                partenza,
                destinazione,
                frazione
            );
            float impulso = 0.5f + 0.5f * Mathf.Sin(
                (t * 4f - frazione) * Mathf.PI * 2f
            );
            Color corrente = colore;
            corrente.a *= Mathf.Lerp(0.28f, 1f, impulso) *
                           Mathf.Lerp(1f, 0.3f, t);
            punti[i].color = corrente;
        }
    }
}

/// <summary>
/// Proiettile ostile creato dalla Sputafango. Ha una traiettoria leggibile,
/// non insegue il giocatore e applica un rallentamento breve all'impatto.
/// </summary>
internal sealed class FoxMudProjectile : MonoBehaviour
{
    private static Sprite spriteFango;
    private Rigidbody2D corpo;
    private Vector2 direzione;
    private float velocita;
    private int danno;
    private float moltiplicatoreRallentamento = 0.62f;
    private float durataRallentamento = 1.55f;
    private float durata = 4f;
    private float prossimaTraccia;
    private bool consumato;

    public static FoxMudProjectile Crea(
        Vector2 posizione,
        Vector2 nuovaDirezione,
        float nuovaVelocita,
        int nuovoDanno,
        float nuovoMoltiplicatoreRallentamento,
        float nuovaDurataRallentamento
    )
    {
        GameObject oggetto = new GameObject("ProiettileFangoVolpe");
        oggetto.transform.position = posizione;
        oggetto.transform.localScale = Vector3.one * 0.48f;

        SpriteRenderer renderer = oggetto.AddComponent<SpriteRenderer>();
        renderer.sprite = OttieniSpriteFango();
        renderer.sortingOrder = 18;

        CircleCollider2D collider = oggetto.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.42f;

        Rigidbody2D corpo = oggetto.AddComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Kinematic;
        corpo.gravityScale = 0f;
        corpo.interpolation = RigidbodyInterpolation2D.Interpolate;
        corpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        FoxMudProjectile proiettile = oggetto.AddComponent<FoxMudProjectile>();
        proiettile.corpo = corpo;
        proiettile.direzione = nuovaDirezione.sqrMagnitude > 0.001f
            ? nuovaDirezione.normalized
            : Vector2.right;
        proiettile.velocita = Mathf.Max(0.5f, nuovaVelocita);
        proiettile.danno = Mathf.Max(1, nuovoDanno);
        proiettile.moltiplicatoreRallentamento = Mathf.Clamp(
            nuovoMoltiplicatoreRallentamento,
            0.2f,
            0.95f
        );
        proiettile.durataRallentamento = Mathf.Max(
            0.1f,
            nuovaDurataRallentamento
        );
        return proiettile;
    }

    void FixedUpdate()
    {
        if (consumato || corpo == null) return;
        corpo.MovePosition(
            corpo.position + direzione * velocita * Time.fixedDeltaTime
        );
    }

    void Update()
    {
        if (consumato) return;
        durata -= Time.deltaTime;
        transform.Rotate(0f, 0f, 210f * Time.deltaTime);
        if (Time.time >= prossimaTraccia)
        {
            prossimaTraccia = Time.time + 0.1f;
            FoxAbilityVfx.CreaAnello(
                transform.position,
                new Color32(118, 76, 38, 150),
                0.03f,
                0.2f,
                0.22f,
                null
            );
        }
        if (durata <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (consumato) return;
        PlayerHealth salute = other.GetComponentInParent<PlayerHealth>();
        if (salute == null) return;

        consumato = true;
        salute.ProvaSubireDanno(danno);
        PlayerMovement movimento = salute.GetComponent<PlayerMovement>();
        if (movimento != null)
        {
            movimento.ApplicaRallentamentoTerreno(
                moltiplicatoreRallentamento,
                durataRallentamento
            );
        }
        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(126, 81, 39, 235),
            0.12f,
            0.9f,
            0.38f,
            null
        );
        Destroy(gameObject);
    }

    private static Sprite OttieniSpriteFango()
    {
        if (spriteFango != null) return spriteFango;
        const int lato = 11;
        Color32[] pixel = new Color32[lato * lato];
        Color32 bordo = new Color32(61, 39, 25, 255);
        Color32 fango = new Color32(132, 84, 42, 255);
        Color32 luce = new Color32(190, 132, 67, 255);
        for (int y = 1; y < lato - 1; y++)
        {
            for (int x = 1; x < lato - 1; x++)
            {
                float dx = x - 5f;
                float dy = y - 5f;
                float distanza = dx * dx + dy * dy;
                if (distanza <= 20f) pixel[y * lato + x] = bordo;
                if (distanza <= 14f) pixel[y * lato + x] = fango;
            }
        }
        pixel[7 * lato + 4] = luce;
        pixel[7 * lato + 5] = luce;
        pixel[6 * lato + 3] = luce;
        pixel[5 * lato + 9] = bordo;
        pixel[2 * lato + 2] = bordo;

        Texture2D texture = new Texture2D(
            lato,
            lato,
            TextureFormat.RGBA32,
            false
        );
        texture.name = "ProiettileFangoVolpe_Texture";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels32(pixel);
        texture.Apply(false, true);

        spriteFango = Sprite.Create(
            texture,
            new Rect(0f, 0f, lato, lato),
            new Vector2(0.5f, 0.5f),
            lato,
            0,
            SpriteMeshType.FullRect
        );
        spriteFango.name = "ProiettileFangoVolpe";
        spriteFango.hideFlags = HideFlags.HideAndDontSave;
        return spriteFango;
    }
}
