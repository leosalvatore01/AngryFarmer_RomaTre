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
    private Sprite paglia;
    private Sprite cassetta;
    private Sprite zucche;
    private Sprite staccionata;

    public Sprite SpriteFangoInterattivo =>
        macchieTerra != null && macchieTerra.Length > 0
            ? macchieTerra[0]
            : null;
    public Sprite SpritePagliaInterattiva => paglia;
    public Sprite SpriteCassaInterattiva => cassetta;
    public Sprite SpriteZuccheInterattive => zucche;

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
        int quantita = Mathf.Max(1, dettagliPerChunk + casuale.Next(-1, 2));

        for (int i = 0; i < quantita; i++)
        {
            float margine = dimensioneChunk * 0.43f;
            Vector2 posizione = new Vector2(
                Intervallo(casuale, -margine, margine),
                Intervallo(casuale, -margine, margine)
            );
            CreaDettaglio(chunk.transform, casuale, posizione, i);
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

        if (scelta < 250)
        {
            sprite = macchieTerra[casuale.Next(macchieTerra.Length)];
            ordine = ordineTerreno + 1;
            scalaMin = 1.1f;
            scalaMax = 1.75f;
            colore = new Color(1f, 1f, 1f, 0.9f);
        }
        else if (scelta < 530)
        {
            sprite = ciuffoErba;
            ordine = ordineTerreno + 3;
            scalaMin = 0.78f;
            scalaMax = 1.18f;
        }
        else if (scelta < 770)
        {
            sprite = fiorellini;
            ordine = ordineTerreno + 4;
            scalaMin = 0.88f;
            scalaMax = 1.25f;
        }
        else if (scelta < 940)
        {
            sprite = sassolini;
            ordine = ordineTerreno + 2;
            scalaMin = 0.7f;
            scalaMax = 1.2f;
        }
        else if (scelta < 990)
        {
            // Nel survival lo sfondo non deve simulare casse, balle o
            // zucche: restano soltanto dettagli naturali non interattivi.
            sprite = sassolini;
            ordine = ordineTerreno + 2;
            scalaMin = 0.7f;
            scalaMax = 1.2f;
        }
        else
        {
            sprite = staccionata;
            ordine = ordineTerreno + 5;
            scalaMin = 0.72f;
            scalaMax = 0.95f;
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
        paglia = CreaBallaPaglia();
        cassetta = CreaCassetta();
        zucche = CreaZucche();
        staccionata = CreaStaccionata();
    }

    private Sprite CreaPratoPixel()
    {
        const int lato = 192;
        Color32 basePrato = new Color32(105, 148, 61, 255);
        Color32[] pixel = new Color32[lato * lato];
        for (int i = 0; i < pixel.Length; i++) pixel[i] = basePrato;

        Color32[] coloriMacchie =
        {
            new Color32(101, 144, 59, 255),
            new Color32(110, 153, 64, 255),
            new Color32(106, 146, 57, 255),
            new Color32(112, 151, 62, 255)
        };
        System.Random casuale = new System.Random(seedMondo ^ 0x51F15EED);

        // Zone di verde ampie e morbide: spezzano la piastrellatura senza
        // trasformare il terreno in una texture piena di rumore minuto.
        for (int i = 0; i < 12; i++)
        {
            int centroX = casuale.Next(lato);
            int centroY = casuale.Next(lato);
            float raggioX = Intervallo(casuale, 15f, 35f);
            float raggioY = Intervallo(casuale, 9f, 22f);
            Color32 colore = coloriMacchie[casuale.Next(coloriMacchie.Length)];

            for (int y = 0; y < lato; y++)
            {
                float distanzaY = Mathf.Abs(y - centroY);
                distanzaY = Mathf.Min(distanzaY, lato - distanzaY);
                for (int x = 0; x < lato; x++)
                {
                    float distanzaX = Mathf.Abs(x - centroX);
                    distanzaX = Mathf.Min(distanzaX, lato - distanzaX);
                    float nx = distanzaX / raggioX;
                    float ny = distanzaY / raggioY;
                    float bordo = RumorePixel(x / 3, y / 3, i + seedMondo) * 0.16f;
                    if (nx * nx + ny * ny <= 1f + bordo)
                    {
                        Imposta(pixel, lato, lato, x, y, colore);
                    }
                }
            }
        }

        Color32 filoScuro = new Color32(69, 113, 47, 255);
        Color32 filoChiaro = new Color32(145, 174, 72, 255);
        for (int i = 0; i < 52; i++)
        {
            int x = casuale.Next(5, lato - 6);
            int y = casuale.Next(5, lato - 7);
            Color32 colore = casuale.Next(4) == 0 ? filoChiaro : filoScuro;
            Imposta(pixel, lato, lato, x, y, colore);
            Imposta(pixel, lato, lato, x + 1, y + 1, colore);
            if (casuale.Next(2) == 0)
            {
                Imposta(pixel, lato, lato, x - 1, y + 2, colore);
            }
            else
            {
                Imposta(pixel, lato, lato, x + 2, y + 2, colore);
            }
        }

        return CreaSprite("PratoPixelFattoria", pixel, lato, lato);
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
                    colore = new Color32(83, 54, 28, 112);
                else if (variante < 38)
                    colore = new Color32(112, 70, 34, 126);
                else if (variante < 63)
                    colore = new Color32(132, 83, 39, 130);
                else
                    colore = new Color32(121, 77, 36, 122);

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
        Color32 scuro = new Color32(52, 92, 35, 255);
        Color32 medio = new Color32(83, 126, 43, 255);
        Color32 luce = new Color32(151, 165, 57, 255);

        Linea(pixel, w, h, 11, 3, 8, 16, scuro, 2);
        Linea(pixel, w, h, 12, 3, 14, 18, medio, 2);
        Linea(pixel, w, h, 10, 3, 4, 12, medio, 2);
        Linea(pixel, w, h, 13, 3, 20, 12, scuro, 2);
        Linea(pixel, w, h, 12, 4, 11, 15, luce, 1);
        Linea(pixel, w, h, 8, 3, 2, 8, luce, 1);
        return CreaSprite("CiuffoErba", pixel, w, h);
    }

    private Sprite CreaFiorellini()
    {
        const int w = 26;
        const int h = 22;
        Color32[] pixel = TelaTrasparente(w, h);
        Color32 verde = new Color32(60, 105, 38, 255);
        Color32 verdeLuce = new Color32(105, 143, 46, 255);

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

    private Sprite CreaBallaPaglia()
    {
        const int w = 36;
        const int h = 28;
        Color32[] pixel = TelaTrasparente(w, h);
        Color32 contorno = new Color32(47, 27, 17, 255);
        Color32 scuro = new Color32(150, 91, 30, 255);
        Color32 medio = new Color32(214, 161, 59, 255);
        Color32 luce = new Color32(246, 198, 77, 255);

        Rettangolo(pixel, w, h, 4, 5, 31, 22, contorno);
        Rettangolo(pixel, w, h, 6, 7, 29, 20, medio);
        Rettangolo(pixel, w, h, 6, 7, 29, 9, scuro);
        Rettangolo(pixel, w, h, 9, 10, 11, 20, scuro);
        Rettangolo(pixel, w, h, 24, 10, 26, 20, scuro);
        for (int x = 7; x < 29; x += 5)
        {
            Linea(pixel, w, h, x, 12, x + 3, 17, luce, 1);
        }
        return CreaSprite("BallaPaglia", pixel, w, h);
    }

    private Sprite CreaCassetta()
    {
        const int w = 34;
        const int h = 28;
        Color32[] pixel = TelaTrasparente(w, h);
        Color32 contorno = new Color32(45, 24, 16, 255);
        Color32 scuro = new Color32(96, 48, 23, 255);
        Color32 medio = new Color32(151, 80, 35, 255);
        Color32 luce = new Color32(198, 122, 54, 255);

        Rettangolo(pixel, w, h, 3, 4, 30, 23, contorno);
        Rettangolo(pixel, w, h, 5, 6, 28, 21, medio);
        Rettangolo(pixel, w, h, 5, 9, 28, 11, scuro);
        Rettangolo(pixel, w, h, 5, 17, 28, 19, scuro);
        Linea(pixel, w, h, 6, 7, 27, 21, luce, 2);
        Linea(pixel, w, h, 27, 7, 6, 21, scuro, 2);
        return CreaSprite("CassettaLegno", pixel, w, h);
    }

    private Sprite CreaZucche()
    {
        const int w = 38;
        const int h = 24;
        Color32[] pixel = TelaTrasparente(w, h);
        DisegnaZucca(pixel, w, h, 2, 3, 13, 13);
        DisegnaZucca(pixel, w, h, 13, 4, 15, 15);
        DisegnaZucca(pixel, w, h, 27, 3, 9, 11);
        return CreaSprite("ZuccheCampo", pixel, w, h);
    }

    private Sprite CreaStaccionata()
    {
        const int w = 54;
        const int h = 30;
        Color32[] pixel = TelaTrasparente(w, h);
        Color32 contorno = new Color32(45, 24, 16, 255);
        Color32 scuro = new Color32(103, 53, 24, 255);
        Color32 medio = new Color32(151, 82, 37, 255);
        Color32 luce = new Color32(202, 132, 60, 255);

        Rettangolo(pixel, w, h, 3, 12, 50, 17, contorno);
        Rettangolo(pixel, w, h, 5, 13, 48, 15, medio);
        Rettangolo(pixel, w, h, 3, 5, 50, 10, contorno);
        Rettangolo(pixel, w, h, 5, 6, 48, 8, scuro);

        int[] pali = { 6, 26, 45 };
        foreach (int x in pali)
        {
            Rettangolo(pixel, w, h, x, 3, x + 6, 25, contorno);
            Rettangolo(pixel, w, h, x + 2, 5, x + 4, 23, medio);
            Rettangolo(pixel, w, h, x + 3, 18, x + 4, 22, luce);
            Imposta(pixel, w, h, x + 3, 26, luce);
        }
        return CreaSprite("Staccionata", pixel, w, h);
    }

    private Sprite CreaSprite(
        string nome,
        Color32[] pixel,
        int larghezza,
        int altezza
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
            32f,
            0,
            SpriteMeshType.FullRect
        );
        sprite.name = nome;
        sprite.hideFlags = HideFlags.HideAndDontSave;

        risorseGenerate.Add(sprite);
        risorseGenerate.Add(texture);
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
        Color32 contorno = new Color32(60, 53, 42, 220);
        Color32 medio = new Color32(126, 118, 91, 220);
        Color32 luce = new Color32(173, 159, 116, 220);
        Rettangolo(pixel, w, h, xMin + 1, yMin, xMax - 1, yMax, contorno);
        Rettangolo(pixel, w, h, xMin, yMin + 1, xMax, yMax - 1, contorno);
        Rettangolo(pixel, w, h, xMin + 2, yMin + 2, xMax - 2, yMax - 2, medio);
        Rettangolo(pixel, w, h, xMin + 2, yMax - 3, xMax - 4, yMax - 2, luce);
    }

    private static void DisegnaZucca(
        Color32[] pixel,
        int w,
        int h,
        int x,
        int y,
        int larghezza,
        int altezza
    )
    {
        Color32 contorno = new Color32(63, 34, 18, 255);
        Color32 arancioScuro = new Color32(177, 76, 25, 255);
        Color32 arancio = new Color32(224, 113, 32, 255);
        Color32 luce = new Color32(244, 153, 45, 255);
        Color32 gambo = new Color32(69, 104, 42, 255);

        Rettangolo(
            pixel,
            w,
            h,
            x + 1,
            y,
            x + larghezza - 2,
            y + altezza - 1,
            contorno
        );
        Rettangolo(
            pixel,
            w,
            h,
            x,
            y + 2,
            x + larghezza - 1,
            y + altezza - 3,
            contorno
        );
        Rettangolo(
            pixel,
            w,
            h,
            x + 2,
            y + 2,
            x + larghezza - 3,
            y + altezza - 3,
            arancio
        );

        int centro = x + larghezza / 2;
        Rettangolo(
            pixel,
            w,
            h,
            centro - 1,
            y + 2,
            centro,
            y + altezza - 3,
            arancioScuro
        );
        Imposta(pixel, w, h, centro + 2, y + altezza - 4, luce);
        Imposta(pixel, w, h, centro + 3, y + altezza - 4, luce);
        Rettangolo(
            pixel,
            w,
            h,
            centro,
            y + altezza,
            centro + 1,
            y + altezza + 2,
            gambo
        );
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
