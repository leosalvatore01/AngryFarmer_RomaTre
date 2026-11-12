using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class FarmBackgroundDecorator : MonoBehaviour
{
    [Header("Mondo a chunk")]
    [SerializeField] private int seedMondo = 73021;
    [SerializeField, Min(4f)] private float dimensioneChunk = 7f;
    [SerializeField, Range(2, 4)] private int raggioChunk = 3;
    [SerializeField, Range(1, 5)] private int dettagliPerChunk = 3;

    [Header("Aspetto")]
    [SerializeField] private Color tintaPrato =
        Color.white;
    [SerializeField] private int ordineTerreno = -30;

    private readonly Dictionary<Vector2Int, GameObject> chunkAttivi =
        new Dictionary<Vector2Int, GameObject>();
    private readonly List<UnityEngine.Object> risorseGenerate =
        new List<UnityEngine.Object>();
    private readonly Dictionary<Vector2Int, Sprite> sentieriChunk =
        new Dictionary<Vector2Int, Sprite>();

    private const int PixelSentieroPerUnita = 32;

    private Camera cameraPrincipale;
    private SpriteRenderer rendererPrato;
    private Transform contenitore;
    private Vector2Int chunkCentrale = new Vector2Int(int.MinValue, int.MinValue);
    private float periodoTextureX = 1f;
    private float periodoTextureY = 1f;
    private float posizioneZIniziale;

    private Sprite[] macchieTerra;
    private Sprite ciuffoErba;
    private Sprite fiorellini;
    private Sprite sassolini;

    void Awake()
    {
        rendererPrato = GetComponent<SpriteRenderer>();
        posizioneZIniziale = transform.position.z;

        CreaLibreriaSprite();
        rendererPrato.sprite = CreaPratoPixel();

        rendererPrato.sortingOrder = ordineTerreno;
        rendererPrato.color = tintaPrato;

        if (rendererPrato.sprite != null)
        {
            periodoTextureX = Mathf.Max(
                0.1f,
                rendererPrato.sprite.bounds.size.x *
                Mathf.Abs(transform.lossyScale.x)
            );
            periodoTextureY = Mathf.Max(
                0.1f,
                rendererPrato.sprite.bounds.size.y *
                Mathf.Abs(transform.lossyScale.y)
            );
        }

        GameObject radice = new GameObject("DettagliFattoriaDinamici");
        contenitore = radice.transform;
    }

    void Start()
    {
        cameraPrincipale = Camera.main;
        AggiornaMondo(true);
    }

    void LateUpdate()
    {
        if (cameraPrincipale == null)
        {
            cameraPrincipale = Camera.main;
        }

        if (cameraPrincipale == null) return;

        RicentraPratoSenzaScorrimento();
        AggiornaMondo(false);
    }

    private void RicentraPratoSenzaScorrimento()
    {
        Vector3 posizioneCamera = cameraPrincipale.transform.position;
        float x = Mathf.Round(posizioneCamera.x / periodoTextureX) * periodoTextureX;
        float y = Mathf.Round(posizioneCamera.y / periodoTextureY) * periodoTextureY;
        transform.position = new Vector3(x, y, posizioneZIniziale);
    }

    private void AggiornaMondo(bool forza)
    {
        if (cameraPrincipale == null) return;

        Vector3 posizione = cameraPrincipale.transform.position;
        Vector2Int nuovoCentro = new Vector2Int(
            Mathf.FloorToInt(posizione.x / dimensioneChunk),
            Mathf.FloorToInt(posizione.y / dimensioneChunk)
        );

        if (!forza && nuovoCentro == chunkCentrale) return;
        chunkCentrale = nuovoCentro;

        HashSet<Vector2Int> necessari = new HashSet<Vector2Int>();
        for (int y = -raggioChunk; y <= raggioChunk; y++)
        {
            for (int x = -raggioChunk; x <= raggioChunk; x++)
            {
                Vector2Int coordinata = nuovoCentro + new Vector2Int(x, y);
                necessari.Add(coordinata);

                if (!chunkAttivi.ContainsKey(coordinata))
                {
                    chunkAttivi[coordinata] = CreaChunk(coordinata);
                }
            }
        }

        List<Vector2Int> daRimuovere = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, GameObject> coppia in chunkAttivi)
        {
            if (!necessari.Contains(coppia.Key))
            {
                if (coppia.Value != null) Destroy(coppia.Value);
                if (sentieriChunk.TryGetValue(coppia.Key, out Sprite sentiero))
                {
                    DistruggiSentiero(sentiero);
                    sentieriChunk.Remove(coppia.Key);
                }
                daRimuovere.Add(coppia.Key);
            }
        }

        foreach (Vector2Int coordinata in daRimuovere)
        {
            chunkAttivi.Remove(coordinata);
        }
    }

    private GameObject CreaChunk(Vector2Int coordinata)
    {
        GameObject chunk = new GameObject(
            "ChunkFattoria_" + coordinata.x + "_" + coordinata.y
        );
        chunk.transform.SetParent(contenitore, false);
        chunk.transform.position = new Vector3(
            (coordinata.x + 0.5f) * dimensioneChunk,
            (coordinata.y + 0.5f) * dimensioneChunk,
            0f
        );

        System.Random casuale = new System.Random(HashChunk(coordinata));
        CreaSentiero(chunk.transform, coordinata);
        int quantita = Mathf.Max(2, dettagliPerChunk + 2 + casuale.Next(-1, 2));

        for (int i = 0; i < quantita; i++)
        {
            float margine = dimensioneChunk * 0.43f;
            Vector2 posizione = new Vector2(
                Intervallo(casuale, -margine, margine),
                Intervallo(casuale, -margine, margine)
            );
            Vector2 mondo = (Vector2)chunk.transform.position + posizione;
            // I sentieri restano sgombri; i dettagli formano piccoli gruppi sui prati.
            if (DistanzaSentiero(mondo) < 0.15f) continue;
            CreaDettaglio(chunk.transform, casuale, posizione, i);
            if (casuale.Next(100) < 58)
            {
                Vector2 vicino = posizione + new Vector2(
                    Intervallo(casuale, -0.55f, 0.55f),
                    Intervallo(casuale, -0.22f, 0.22f));
                if (DistanzaSentiero((Vector2)chunk.transform.position + vicino) > 0.15f)
                    CreaDettaglio(chunk.transform, casuale, vicino, i + 100);
            }
        }

        return chunk;
    }

    private void CreaDettaglio(
        Transform parent,
        System.Random casuale,
        Vector2 posizione,
        int indice
    )
    {
        int scelta = casuale.Next(1000);
        Sprite sprite;
        int ordine;
        float scalaMin;
        float scalaMax;
        Color colore = Color.white;

        if (scelta < 100)
        {
            sprite = macchieTerra[casuale.Next(macchieTerra.Length)];
            ordine = ordineTerreno + 1;
            scalaMin = 0.95f;
            scalaMax = 1.55f;
            colore = new Color(1f, 1f, 1f, 0.65f);
        }
        else if (scelta < 620)
        {
            sprite = ciuffoErba;
            ordine = ordineTerreno + 3;
            scalaMin = 0.8f;
            scalaMax = 1.3f;
        }
        else if (scelta < 870)
        {
            sprite = fiorellini;
            ordine = ordineTerreno + 4;
            scalaMin = 0.8f;
            scalaMax = 1.1f;
        }
        else
        {
            sprite = sassolini;
            ordine = ordineTerreno + 2;
            scalaMin = 0.7f;
            scalaMax = 1.2f;
        }

        GameObject dettaglio = new GameObject("Dettaglio_" + indice);
        dettaglio.transform.SetParent(parent, false);
        dettaglio.transform.localPosition = posizione;

        float scala = Intervallo(casuale, scalaMin, scalaMax);
        float segnoX = casuale.Next(2) == 0 ? -1f : 1f;
        dettaglio.transform.localScale = new Vector3(scala * segnoX, scala, 1f);

        if (sprite == macchieTerra[0] ||
            sprite == macchieTerra[1] ||
            sprite == macchieTerra[2])
        {
            dettaglio.transform.localRotation = Quaternion.Euler(
                0f,
                0f,
                casuale.Next(4) * 90f
            );
        }

        SpriteRenderer renderer = dettaglio.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = colore;
        renderer.sortingLayerID = rendererPrato.sortingLayerID;
        renderer.sortingOrder = ordine;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
    }

    private void CreaLibreriaSprite()
    {
        macchieTerra = new[]
        {
            CreaMacchiaTerra(141, 58, 36),
            CreaMacchiaTerra(271, 64, 40),
            CreaMacchiaTerra(619, 52, 34)
        };
        ciuffoErba = CreaCiuffoErba();
        fiorellini = CreaFiorellini();
        sassolini = CreaSassolini();
    }

    private Sprite CreaPratoPixel()
    {
        const int lato = 384;
        Color32[] pixel = new Color32[lato * lato];
        // Rumore periodico: i bordi coincidono quando lo sprite viene ripetuto.
        // La variazione resta tenue, dietro alle sagome e ai segnali di attacco.
        float seme = (seedMondo & 255) * 0.17f;
        for (int y = 0; y < lato; y++)
        {
            for (int x = 0; x < lato; x++)
            {
                float u = x / (float)lato;
                float v = y / (float)lato;
                float a = Mathf.PerlinNoise(seme + u * 4f, seme + v * 4f);
                float b = Mathf.PerlinNoise(seme + (u - 1f) * 4f, seme + v * 4f);
                float c = Mathf.PerlinNoise(seme + u * 4f, seme + (v - 1f) * 4f);
                float d = Mathf.PerlinNoise(seme + (u - 1f) * 4f, seme + (v - 1f) * 4f);
                float macchia = Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
                int tono = Mathf.RoundToInt((macchia - 0.5f) * 18f);
                pixel[y * lato + x] = new Color32(
                    (byte)(97 + tono), (byte)(132 + tono), (byte)(70 + tono / 2), 255);
            }
        }

        System.Random casuale = new System.Random(seedMondo ^ 0x51F15EED);
        for (int i = 0; i < 560; i++)
        {
            int x = casuale.Next(5, lato - 6);
            int y = casuale.Next(5, lato - 7);
            Color32 baseFilo = pixel[y * lato + x];
            Color32 scuro = new Color32((byte)(baseFilo.r - 11), (byte)(baseFilo.g - 12), (byte)(baseFilo.b - 6), 255);
            Color32 luce = new Color32((byte)(baseFilo.r + 16), (byte)(baseFilo.g + 12), (byte)(baseFilo.b + 4), 255);
            int direzione = casuale.Next(2) == 0 ? -1 : 1;
            Imposta(pixel, lato, lato, x, y, scuro);
            Imposta(pixel, lato, lato, x + direzione, y + 1, scuro);
            Imposta(pixel, lato, lato, x + direzione, y + 2, luce);
            if (casuale.Next(3) == 0)
                Imposta(pixel, lato, lato, x - direzione, y + 1, luce);
        }

        return CreaSprite("PratoPixelFattoria", pixel, lato, lato, 64f);
    }

    private float AsseSentieroOrizzontale(float x)
    {
        float fase = (seedMondo % 97) * 0.031f;
        return 1.7f + x * 0.16f + Mathf.Sin(x * 0.19f + fase) * 0.85f;
    }

    private float AsseSentieroVerticale(float y)
    {
        return 5.8f + y * 0.08f + Mathf.Sin(y * 0.15f) * 1.1f;
    }

    private static float DistanzaPeriodica(float coordinata, float periodo)
    {
        return Mathf.Abs(Mathf.Repeat(coordinata + periodo * 0.5f, periodo) - periodo * 0.5f);
    }

    private float DistanzaSentiero(Vector2 mondo)
    {
        return Mathf.Min(
            DistanzaPeriodica(mondo.y - AsseSentieroOrizzontale(mondo.x), 26f) - 0.62f,
            DistanzaPeriodica(mondo.x - AsseSentieroVerticale(mondo.y), 33f) - 0.43f);
    }

    private void CreaSentiero(Transform parent, Vector2Int coordinata)
    {
        if (DistanzaSentiero(parent.position) > dimensioneChunk * 0.85f) return;
        int lato = Mathf.RoundToInt(dimensioneChunk * PixelSentieroPerUnita);
        float origineX = coordinata.x * dimensioneChunk;
        float origineY = coordinata.y * dimensioneChunk;
        var assiX = new float[lato];
        var assiY = new float[lato];
        // Trigonometria per riga/colonna, non per pixel, anche durante lo streaming.
        for (int i = 0; i < lato; i++)
        {
            assiX[i] = AsseSentieroOrizzontale(origineX + (i + 0.5f) / PixelSentieroPerUnita);
            assiY[i] = AsseSentieroVerticale(origineY + (i + 0.5f) / PixelSentieroPerUnita);
        }
        Color32[] pixel = TelaTrasparente(lato, lato);
        bool presente = false;
        for (int y = 0; y < lato; y++)
        {
            float mondoY = origineY + (y + 0.5f) / PixelSentieroPerUnita;
            for (int x = 0; x < lato; x++)
            {
                float mondoX = origineX + (x + 0.5f) / PixelSentieroPerUnita;
                int px = coordinata.x * lato + x;
                int py = coordinata.y * lato + y;
                // Coordinate globali: colori e bordi continuano tra chunk adiacenti.
                float bordo = RumorePixel(Mathf.FloorToInt(px / 4f), Mathf.FloorToInt(py / 4f), seedMondo) * 0.16f;
                float orizzontale = DistanzaPeriodica(mondoY - assiX[x], 26f);
                float verticale = DistanzaPeriodica(mondoX - assiY[y], 33f);
                float distanza = Mathf.Min(orizzontale - 0.62f, verticale - 0.43f) + bordo;
                if (distanza > 0.17f) continue;
                presente = true;
                float interno = Mathf.Clamp01(-distanza / 0.26f);
                int grana = Mathf.RoundToInt(RumorePixel(px / 2, py / 2, seedMondo + 13) * 11f);
                Color colore = Color.Lerp(new Color32(119, 126, 70, 255), new Color32(171, 145, 96, 255), interno);
                // Due solchi tenui danno al sentiero il carattere di una strada poderale.
                bool solco = Mathf.Abs(orizzontale - 0.24f) < 0.035f && verticale > 0.5f;
                int ombra = solco ? 9 : 0;
                Color32 c = colore;
                c.r = (byte)(c.r + grana - ombra);
                c.g = (byte)(c.g + grana - ombra);
                c.b = (byte)(c.b + grana / 2 - ombra / 2);
                c.a = (byte)Mathf.RoundToInt(Mathf.Clamp01((0.17f - distanza) / 0.15f) * 255f);
                pixel[y * lato + x] = c;
            }
        }
        if (!presente) return;
        Sprite sprite = CreaSprite("Sentiero_" + coordinata.x + "_" + coordinata.y,
            pixel, lato, lato, PixelSentieroPerUnita, false);
        sentieriChunk.Add(coordinata, sprite);
        GameObject oggetto = new GameObject("SentieroDecorativo");
        oggetto.transform.SetParent(parent, false);
        SpriteRenderer renderer = oggetto.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerID = rendererPrato.sortingLayerID;
        renderer.sortingOrder = ordineTerreno + 1;
    }

    private static void DistruggiSentiero(Sprite sprite)
    {
        if (sprite == null) return;
        Destroy(sprite.texture);
        Destroy(sprite);
    }

    private Sprite CreaMacchiaTerra(int seed, int larghezza, int altezza)
    {
        Color32[] pixel = TelaTrasparente(larghezza, altezza);
        System.Random casuale = new System.Random(seed);
        float centroX = (larghezza - 1) * 0.5f;
        float centroY = (altezza - 1) * 0.5f;

        for (int y = 0; y < altezza; y++)
        {
            for (int x = 0; x < larghezza; x++)
            {
                float nx = (x - centroX) / (larghezza * 0.47f);
                float ny = (y - centroY) / (altezza * 0.43f);
                float rumore = RumorePixel(x, y, seed) * 0.17f;
                float distanza = nx * nx + ny * ny;

                if (distanza > 1f + rumore) continue;

                int variante = casuale.Next(100);
                Color32 colore;
                if (variante < 12)
                    colore = new Color32(111, 107, 61, 85);
                else if (variante < 38)
                    colore = new Color32(133, 122, 69, 95);
                else if (variante < 63)
                    colore = new Color32(147, 131, 76, 105);
                else
                    colore = new Color32(140, 126, 71, 95);

                if (distanza > 0.78f + rumore)
                {
                    colore.a = (byte)Mathf.RoundToInt(colore.a * 0.48f);
                }

                Imposta(pixel, larghezza, altezza, x, y, colore);
            }
        }

        return CreaSprite("MacchiaTerra_" + seed, pixel, larghezza, altezza);
    }

    private Sprite CreaCiuffoErba()
    {
        const int w = 24;
        const int h = 20;
        Color32[] pixel = TelaTrasparente(w, h);
        Color32 scuro = new Color32(64, 100, 50, 255);
        Color32 medio = new Color32(91, 129, 58, 255);
        Color32 luce = new Color32(149, 164, 83, 255);

        Rettangolo(pixel, w, h, 5, 2, 18, 3, new Color32(49, 76, 39, 55));
        Linea(pixel, w, h, 11, 3, 8, 16, scuro, 2);
        Linea(pixel, w, h, 12, 3, 14, 18, medio, 2);
        Linea(pixel, w, h, 10, 3, 4, 12, medio, 2);
        Linea(pixel, w, h, 13, 3, 20, 12, scuro, 2);
        Linea(pixel, w, h, 12, 4, 11, 15, luce, 1);
        Linea(pixel, w, h, 8, 3, 2, 8, luce, 1);
        Linea(pixel, w, h, 14, 3, 16, 11, luce, 1);
        Linea(pixel, w, h, 10, 3, 6, 11, luce, 1);
        return CreaSprite("CiuffoErba", pixel, w, h);
    }

    private Sprite CreaFiorellini()
    {
        const int w = 26;
        const int h = 22;
        Color32[] pixel = TelaTrasparente(w, h);
        Color32 verde = new Color32(70, 108, 51, 255);
        Color32 verdeLuce = new Color32(113, 145, 66, 255);

        Linea(pixel, w, h, 7, 3, 7, 15, verde, 2);
        Linea(pixel, w, h, 13, 3, 14, 18, verdeLuce, 2);
        Linea(pixel, w, h, 19, 3, 18, 13, verde, 2);
        DisegnaFiore(pixel, w, h, 7, 16, new Color32(244, 217, 165, 255));
        DisegnaFiore(pixel, w, h, 14, 18, new Color32(227, 106, 72, 255));
        DisegnaFiore(pixel, w, h, 18, 14, new Color32(244, 217, 165, 255));
        return CreaSprite("FiorelliniCampo", pixel, w, h);
    }

    private Sprite CreaSassolini()
    {
        const int w = 30;
        const int h = 18;
        Color32[] pixel = TelaTrasparente(w, h);
        DisegnaSasso(pixel, w, h, 3, 3, 11, 9);
        DisegnaSasso(pixel, w, h, 12, 5, 21, 13);
        DisegnaSasso(pixel, w, h, 21, 3, 27, 8);
        return CreaSprite("Sassolini", pixel, w, h);
    }

    private Sprite CreaSprite(
        string nome,
        Color32[] pixel,
        int larghezza,
        int altezza,
        float pixelPerUnita = 48f,
        bool conservaRisorsa = true
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

        if (conservaRisorsa)
        {
            risorseGenerate.Add(sprite);
            risorseGenerate.Add(texture);
        }
        return sprite;
    }

    private int HashChunk(Vector2Int coordinata)
    {
        unchecked
        {
            int hash = seedMondo;
            hash = hash * 397 ^ coordinata.x;
            hash = hash * 397 ^ coordinata.y;
            hash ^= hash >> 16;
            return hash;
        }
    }

    private static float RumorePixel(int x, int y, int seed)
    {
        unchecked
        {
            int valore = x * 374761393 + y * 668265263 + seed * 69069;
            valore = (valore ^ (valore >> 13)) * 1274126177;
            return ((valore ^ (valore >> 16)) & 1023) / 1023f - 0.5f;
        }
    }

    private static float Intervallo(
        System.Random casuale,
        float minimo,
        float massimo
    )
    {
        return minimo + (float)casuale.NextDouble() * (massimo - minimo);
    }

    private static Color32[] TelaTrasparente(int larghezza, int altezza)
    {
        Color32[] pixel = new Color32[larghezza * altezza];
        Color32 trasparente = new Color32(0, 0, 0, 0);
        for (int i = 0; i < pixel.Length; i++) pixel[i] = trasparente;
        return pixel;
    }

    private static void DisegnaFiore(
        Color32[] pixel,
        int w,
        int h,
        int cx,
        int cy,
        Color32 petalo
    )
    {
        Color32 centro = new Color32(224, 161, 40, 255);
        Imposta(pixel, w, h, cx - 1, cy, petalo);
        Imposta(pixel, w, h, cx + 1, cy, petalo);
        Imposta(pixel, w, h, cx, cy - 1, petalo);
        Imposta(pixel, w, h, cx, cy + 1, petalo);
        Imposta(pixel, w, h, cx, cy, centro);
    }

    private static void DisegnaSasso(
        Color32[] pixel,
        int w,
        int h,
        int xMin,
        int yMin,
        int xMax,
        int yMax
    )
    {
        float cx = (xMin + xMax) * 0.5f;
        float cy = (yMin + yMax) * 0.5f;
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                float nx = (x - cx) / Mathf.Max(1f, (xMax - xMin) * 0.5f);
                float ny = (y - cy) / Mathf.Max(1f, (yMax - yMin) * 0.5f);
                float distanza = nx * nx + ny * ny;
                if (distanza > 1f) continue;
                Color32 colore = distanza > 0.78f
                    ? new Color32(78, 87, 65, 230)
                    : y > cy && x < cx + 2
                        ? new Color32(162, 160, 128, 255)
                        : new Color32(124, 131, 105, 255);
                Imposta(pixel, w, h, x, y, colore);
            }
        }
    }

    private static void Rettangolo(
        Color32[] pixel,
        int w,
        int h,
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
                Imposta(pixel, w, h, x, y, colore);
            }
        }
    }

    private static void Linea(
        Color32[] pixel,
        int w,
        int h,
        int x0,
        int y0,
        int x1,
        int y1,
        Color32 colore,
        int spessore
    )
    {
        int dx = Math.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int errore = dx + dy;

        while (true)
        {
            for (int oy = 0; oy < spessore; oy++)
            {
                for (int ox = 0; ox < spessore; ox++)
                {
                    Imposta(pixel, w, h, x0 + ox, y0 + oy, colore);
                }
            }

            if (x0 == x1 && y0 == y1) break;
            int doppioErrore = 2 * errore;
            if (doppioErrore >= dy)
            {
                errore += dy;
                x0 += sx;
            }
            if (doppioErrore <= dx)
            {
                errore += dx;
                y0 += sy;
            }
        }
    }

    private static void Imposta(
        Color32[] pixel,
        int w,
        int h,
        int x,
        int y,
        Color32 colore
    )
    {
        if (x < 0 || y < 0 || x >= w || y >= h) return;
        pixel[y * w + x] = colore;
    }

    void OnDestroy()
    {
        foreach (Sprite sentiero in sentieriChunk.Values)
            DistruggiSentiero(sentiero);
        sentieriChunk.Clear();
        if (contenitore != null)
        {
            Destroy(contenitore.gameObject);
        }

        foreach (UnityEngine.Object risorsa in risorseGenerate)
        {
            if (risorsa != null) Destroy(risorsa);
        }
        risorseGenerate.Clear();
    }
}
