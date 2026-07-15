using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum FarmPixelIcon
{
    Ondata,
    Cuore,
    Moneta,
    Uovo,
    Bottega,
    Movimento,
    Resistenza,
    SaluteMassima,
    Cura,
    Danno,
    Cadenza,
    Penetrazione,
    Volpe
}

/// <summary>
/// Piccola libreria di sprite procedurali per mantenere HUD e bottega
/// coerenti con la pixel art del gioco senza dipendere da texture esterne.
/// </summary>
public static class FarmPixelUI
{
    private const int DimensioneIcona = 20;

    private static readonly Color32 Trasparente = new Color32(0, 0, 0, 0);
    private static readonly Color32 Contorno = new Color32(33, 20, 15, 255);
    private static readonly Color32 ContornoMorbido = new Color32(66, 34, 20, 255);
    private static readonly Color32 LegnoScuro = new Color32(91, 45, 22, 255);
    private static readonly Color32 Legno = new Color32(139, 74, 37, 255);
    private static readonly Color32 LegnoChiaro = new Color32(190, 117, 52, 255);
    private static readonly Color32 Paglia = new Color32(214, 161, 59, 255);
    private static readonly Color32 Crema = new Color32(244, 217, 165, 255);

    private static Sprite pannelloLegno;
    private static Sprite pannelloIncassato;
    private static Sprite pulsanteLegno;
    private static readonly Dictionary<FarmPixelIcon, Sprite> Icone =
        new Dictionary<FarmPixelIcon, Sprite>();

    public static Sprite PannelloLegno
    {
        get
        {
            if (pannelloLegno == null)
            {
                pannelloLegno = CreaPannelloLegno();
            }
            return pannelloLegno;
        }
    }

    public static Sprite PannelloIncassato
    {
        get
        {
            if (pannelloIncassato == null)
            {
                pannelloIncassato = CreaPannelloIncassato();
            }
            return pannelloIncassato;
        }
    }

    public static Sprite PulsanteLegno
    {
        get
        {
            if (pulsanteLegno == null)
            {
                pulsanteLegno = CreaPulsanteLegno();
            }
            return pulsanteLegno;
        }
    }

    public static Sprite OttieniIcona(FarmPixelIcon tipo)
    {
        Sprite sprite;
        if (Icone.TryGetValue(tipo, out sprite) && sprite != null)
        {
            return sprite;
        }

        sprite = CreaIcona(tipo);
        Icone[tipo] = sprite;
        return sprite;
    }

    public static Image AggiungiIcona(
        Transform parent,
        string nome,
        FarmPixelIcon tipo,
        Vector2 posizione,
        Vector2 dimensioni
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        oggetto.transform.SetParent(parent, false);

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;

        Image immagine = oggetto.GetComponent<Image>();
        immagine.sprite = OttieniIcona(tipo);
        immagine.type = Image.Type.Simple;
        immagine.preserveAspect = true;
        immagine.raycastTarget = false;
        return immagine;
    }

    public static void ApplicaPannello(
        Image immagine,
        bool incassato = false,
        bool riceveRaycast = true
    )
    {
        if (immagine == null) return;

        immagine.sprite = incassato ? PannelloIncassato : PannelloLegno;
        immagine.type = Image.Type.Sliced;
        immagine.color = Color.white;
        immagine.raycastTarget = riceveRaycast;
    }

    public static void ApplicaPulsante(Button pulsante, Color tinta)
    {
        if (pulsante == null) return;

        Image immagine = pulsante.targetGraphic as Image;
        if (immagine == null)
        {
            immagine = pulsante.GetComponent<Image>();
            pulsante.targetGraphic = immagine;
        }

        if (immagine != null)
        {
            immagine.sprite = PulsanteLegno;
            immagine.type = Image.Type.Sliced;
            immagine.color = tinta;
        }

        ColorBlock colori = pulsante.colors;
        colori.normalColor = Color.white;
        colori.highlightedColor = new Color(1.08f, 1.08f, 1.02f, 1f);
        colori.pressedColor = new Color(0.72f, 0.72f, 0.68f, 1f);
        colori.selectedColor = Color.white;
        colori.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.72f);
        colori.colorMultiplier = 1f;
        colori.fadeDuration = 0.06f;
        pulsante.colors = colori;

        Shadow ombra = pulsante.GetComponent<Shadow>();
        if (ombra == null)
        {
            ombra = pulsante.gameObject.AddComponent<Shadow>();
        }
        ombra.effectColor = new Color(0.10f, 0.045f, 0.02f, 0.9f);
        ombra.effectDistance = new Vector2(3f, -3f);
        ombra.useGraphicAlpha = true;
    }

    public static void ApplicaTesto(TMP_Text testo, Color colore)
    {
        if (testo == null) return;

        testo.color = colore;
        testo.outlineColor = new Color32(42, 22, 14, 235);
        testo.outlineWidth = 0.12f;

        Shadow ombra = testo.GetComponent<Shadow>();
        if (ombra == null)
        {
            ombra = testo.gameObject.AddComponent<Shadow>();
        }
        ombra.effectColor = new Color(0.08f, 0.035f, 0.018f, 0.82f);
        ombra.effectDistance = new Vector2(2f, -2f);
        ombra.useGraphicAlpha = true;
    }

    private static Sprite CreaPannelloLegno()
    {
        const int lato = 32;
        Color32[] pixel = NuovaTela(lato, lato);

        for (int y = 0; y < lato; y++)
        {
            for (int x = 0; x < lato; x++)
            {
                Color32 colore;
                if (x < 2 || y < 2 || x >= lato - 2 || y >= lato - 2)
                {
                    colore = Contorno;
                }
                else if (x < 5 || y < 5 || x >= lato - 5 || y >= lato - 5)
                {
                    colore = ((x + y) & 1) == 0 ? LegnoScuro : Legno;
                }
                else
                {
                    int tavola = (y - 5) / 7;
                    colore = tavola % 2 == 0
                        ? new Color32(151, 78, 36, 255)
                        : new Color32(132, 64, 31, 255);

                    if ((y - 5) % 7 == 0)
                    {
                        colore = LegnoScuro;
                    }
                    else if ((x + y * 3) % 17 == 0)
                    {
                        colore = LegnoChiaro;
                    }
                }
                Imposta(pixel, lato, lato, x, y, colore);
            }
        }

        DisegnaChiodo(pixel, lato, lato, 5, 5);
        DisegnaChiodo(pixel, lato, lato, lato - 6, 5);
        DisegnaChiodo(pixel, lato, lato, 5, lato - 6);
        DisegnaChiodo(pixel, lato, lato, lato - 6, lato - 6);

        return CreaSprite("UI_PannelloLegno", pixel, lato, lato, 16f, 7f);
    }

    private static Sprite CreaPannelloIncassato()
    {
        const int lato = 24;
        Color32[] pixel = NuovaTela(lato, lato);

        for (int y = 0; y < lato; y++)
        {
            for (int x = 0; x < lato; x++)
            {
                int bordo = Math.Min(Math.Min(x, lato - 1 - x),
                    Math.Min(y, lato - 1 - y));
                Color32 colore;
                if (bordo < 2) colore = Contorno;
                else if (bordo < 4) colore = LegnoChiaro;
                else if (bordo < 6) colore = LegnoScuro;
                else
                {
                    colore = ((x + y) & 3) == 0
                        ? new Color32(72, 43, 28, 242)
                        : new Color32(61, 36, 25, 242);
                }
                Imposta(pixel, lato, lato, x, y, colore);
            }
        }

        return CreaSprite("UI_PannelloIncassato", pixel, lato, lato, 12f, 6f);
    }

    private static Sprite CreaPulsanteLegno()
    {
        const int lato = 24;
        Color32[] pixel = NuovaTela(lato, lato);

        for (int y = 0; y < lato; y++)
        {
            for (int x = 0; x < lato; x++)
            {
                int bordo = Math.Min(Math.Min(x, lato - 1 - x),
                    Math.Min(y, lato - 1 - y));
                Color32 colore;
                if (bordo < 2) colore = Contorno;
                else if (bordo < 4) colore = Paglia;
                else if (bordo < 6) colore = LegnoScuro;
                else
                {
                    colore = y > lato / 2
                        ? new Color32(190, 99, 40, 255)
                        : new Color32(151, 69, 31, 255);
                }
                Imposta(pixel, lato, lato, x, y, colore);
            }
        }

        return CreaSprite("UI_PulsanteLegno", pixel, lato, lato, 12f, 6f);
    }

    private static Sprite CreaIcona(FarmPixelIcon tipo)
    {
        Color32[] pixel = NuovaTela(DimensioneIcona, DimensioneIcona);

        switch (tipo)
        {
            case FarmPixelIcon.Ondata:
                DisegnaSpiga(pixel);
                break;
            case FarmPixelIcon.Cuore:
            case FarmPixelIcon.SaluteMassima:
                DisegnaCuore(pixel, tipo == FarmPixelIcon.SaluteMassima);
                break;
            case FarmPixelIcon.Moneta:
                DisegnaMoneta(pixel);
                break;
            case FarmPixelIcon.Uovo:
                DisegnaUovo(pixel);
                break;
            case FarmPixelIcon.Bottega:
                DisegnaBottega(pixel);
                break;
            case FarmPixelIcon.Movimento:
                DisegnaStivale(pixel);
                break;
            case FarmPixelIcon.Resistenza:
                DisegnaScudo(pixel);
                break;
            case FarmPixelIcon.Cura:
                DisegnaPozione(pixel);
                break;
            case FarmPixelIcon.Danno:
                DisegnaPatata(pixel, false);
                break;
            case FarmPixelIcon.Cadenza:
                DisegnaCadenza(pixel);
                break;
            case FarmPixelIcon.Penetrazione:
                DisegnaPatata(pixel, true);
                break;
            case FarmPixelIcon.Volpe:
                DisegnaVolpe(pixel);
                break;
        }

        return CreaSprite(
            "UI_Icona_" + tipo,
            pixel,
            DimensioneIcona,
            DimensioneIcona,
            20f,
            0f
        );
    }

    private static void DisegnaCuore(Color32[] pixel, bool conPlus)
    {
        bool Dentro(int x, int y)
        {
            bool loboSinistro = Quadrato(x - 6) + Quadrato(y - 12) <= 16;
            bool loboDestro = Quadrato(x - 13) + Quadrato(y - 12) <= 16;
            bool triangolo = y >= 3 && y <= 12 &&
                Math.Abs(x - 9.5f) <= (y - 2) * 0.74f;
            return loboSinistro || loboDestro || triangolo;
        }

        DisegnaForma(
            pixel,
            Dentro,
            new Color32(196, 47, 38, 255),
            Contorno
        );
        Rettangolo(pixel, 5, 13, 7, 14, new Color32(255, 121, 83, 255));

        if (conPlus)
        {
            Rettangolo(pixel, 13, 4, 17, 6, Crema);
            Rettangolo(pixel, 14, 3, 16, 7, Crema);
            ContornaRettangolo(pixel, 12, 2, 18, 8, Contorno);
        }
    }

    private static void DisegnaVolpe(Color32[] pixel)
    {
        Color32 arancioScuro = new Color32(139, 55, 25, 255);
        Color32 arancio = new Color32(221, 91, 29, 255);
        Color32 arancioLuce = new Color32(244, 137, 45, 255);
        Color32 muso = new Color32(247, 211, 157, 255);

        // Orecchie con contorno scuro.
        Linea(pixel, 3, 16, 6, 19, Contorno, 2);
        Linea(pixel, 6, 19, 8, 14, Contorno, 2);
        Linea(pixel, 16, 16, 13, 19, Contorno, 2);
        Linea(pixel, 13, 19, 11, 14, Contorno, 2);
        Rettangolo(pixel, 5, 15, 7, 17, arancioScuro);
        Rettangolo(pixel, 12, 15, 14, 17, arancioScuro);

        // Testa e guance.
        Rettangolo(pixel, 4, 8, 15, 15, Contorno);
        Rettangolo(pixel, 5, 7, 14, 14, arancio);
        Rettangolo(pixel, 6, 12, 13, 15, arancioLuce);
        Rettangolo(pixel, 6, 7, 9, 10, muso);
        Rettangolo(pixel, 10, 7, 13, 10, muso);
        Rettangolo(pixel, 8, 5, 11, 9, muso);

        // Occhi e naso.
        Rettangolo(pixel, 6, 11, 7, 12, Contorno);
        Rettangolo(pixel, 12, 11, 13, 12, Contorno);
        Rettangolo(pixel, 9, 5, 10, 6, Contorno);
        Imposta(pixel, DimensioneIcona, DimensioneIcona,
            9, 8, arancioScuro);
        Imposta(pixel, DimensioneIcona, DimensioneIcona,
            10, 8, arancioScuro);
    }

    private static void DisegnaMoneta(Color32[] pixel)
    {
        bool Dentro(int x, int y)
        {
            return Quadrato(x - 9.5f) + Quadrato(y - 9.5f) <= 54f;
        }

        DisegnaForma(pixel, Dentro, new Color32(239, 177, 38, 255), Contorno);
        for (int y = 5; y <= 14; y++)
        {
            for (int x = 5; x <= 14; x++)
            {
                float d = Quadrato(x - 9.5f) + Quadrato(y - 9.5f);
                if (d > 24f && d < 37f)
                {
                    Imposta(pixel, DimensioneIcona, DimensioneIcona,
                        x, y, new Color32(191, 112, 24, 255));
                }
            }
        }
        Rettangolo(pixel, 7, 12, 9, 14, new Color32(255, 230, 95, 255));
    }

    private static void DisegnaUovo(Color32[] pixel)
    {
        bool Dentro(int x, int y)
        {
            float nx = (x - 9.5f) / (y > 10 ? 5.1f : 6.2f);
            float ny = (y - 9f) / 7.5f;
            return nx * nx + ny * ny <= 1f;
        }

        DisegnaForma(pixel, Dentro, new Color32(245, 232, 194, 255), Contorno);
        Rettangolo(pixel, 7, 12, 9, 14, Color.white);
        Rettangolo(pixel, 12, 5, 14, 7, new Color32(224, 202, 153, 255));
    }

    private static void DisegnaSpiga(Color32[] pixel)
    {
        Linea(pixel, 9, 2, 10, 17, new Color32(92, 89, 35, 255), 2);
        Color32 oro = new Color32(226, 174, 53, 255);
        Color32 luce = new Color32(255, 215, 84, 255);

        for (int y = 6; y <= 15; y += 3)
        {
            Rettangolo(pixel, 5, y, 8, y + 2, Contorno);
            Rettangolo(pixel, 6, y + 1, 8, y + 2, oro);
            Rettangolo(pixel, 11, y - 1, 14, y + 1, Contorno);
            Rettangolo(pixel, 11, y, 13, y + 1, luce);
        }
        Rettangolo(pixel, 8, 15, 11, 18, Contorno);
        Rettangolo(pixel, 9, 16, 10, 18, luce);
    }

    private static void DisegnaBottega(Color32[] pixel)
    {
        Color32 rosso = new Color32(164, 67, 40, 255);
        Color32 blu = new Color32(39, 76, 101, 255);

        Rettangolo(pixel, 3, 5, 17, 13, Contorno);
        Rettangolo(pixel, 4, 6, 16, 12, Legno);
        for (int x = 2; x <= 17; x++)
        {
            int altezza = x <= 9 ? x - 1 : 18 - x;
            int cima = 12 + Math.Max(0, altezza / 2);
            for (int y = 12; y <= cima; y++)
            {
                Imposta(pixel, DimensioneIcona, DimensioneIcona,
                    x, y, y == cima ? Contorno : rosso);
            }
        }
        Rettangolo(pixel, 6, 5, 10, 10, Contorno);
        Rettangolo(pixel, 7, 5, 9, 9, blu);
        Rettangolo(pixel, 12, 7, 15, 10, Crema);
    }

    private static void DisegnaStivale(Color32[] pixel)
    {
        Color32 blu = new Color32(33, 76, 107, 255);
        Rettangolo(pixel, 7, 8, 13, 17, Contorno);
        Rettangolo(pixel, 8, 8, 12, 16, blu);
        Rettangolo(pixel, 4, 4, 14, 9, Contorno);
        Rettangolo(pixel, 5, 5, 13, 8, new Color32(109, 61, 32, 255));
        Rettangolo(pixel, 11, 5, 16, 7, Contorno);
        Rettangolo(pixel, 11, 6, 15, 7, new Color32(151, 84, 38, 255));
    }

    private static void DisegnaScudo(Color32[] pixel)
    {
        bool Dentro(int x, int y)
        {
            if (y < 3 || y > 17) return false;
            float mezza = y < 8 ? (y - 2) * 0.55f : 6.5f;
            return Math.Abs(x - 9.5f) <= mezza;
        }
        DisegnaForma(pixel, Dentro, new Color32(45, 91, 115, 255), Contorno);
        Linea(pixel, 10, 5, 10, 15, new Color32(104, 159, 170, 255), 2);
        Rettangolo(pixel, 6, 13, 8, 15, new Color32(146, 190, 187, 255));
    }

    private static void DisegnaPozione(Color32[] pixel)
    {
        Color32 rosso = new Color32(203, 52, 48, 255);
        Rettangolo(pixel, 7, 14, 12, 17, Contorno);
        Rettangolo(pixel, 8, 14, 11, 16, Crema);
        Rettangolo(pixel, 5, 5, 14, 14, Contorno);
        Rettangolo(pixel, 6, 6, 13, 12, rosso);
        Rettangolo(pixel, 7, 10, 12, 12, new Color32(244, 91, 69, 255));
        Rettangolo(pixel, 8, 7, 11, 8, new Color32(255, 183, 96, 255));
    }

    private static void DisegnaPatata(Color32[] pixel, bool perforante)
    {
        bool Dentro(int x, int y)
        {
            float nx = (x - 9.5f) / 6.5f;
            float ny = (y - 9.5f) / 5.3f;
            return nx * nx + ny * ny <= 1f;
        }
        DisegnaForma(pixel, Dentro, new Color32(173, 112, 53, 255), Contorno);
        Rettangolo(pixel, 6, 11, 7, 12, new Color32(105, 62, 29, 255));
        Rettangolo(pixel, 12, 7, 13, 8, new Color32(105, 62, 29, 255));
        Rettangolo(pixel, 10, 13, 11, 14, new Color32(229, 163, 76, 255));

        if (perforante)
        {
            Linea(pixel, 2, 3, 17, 17, new Color32(224, 215, 180, 255), 2);
            Imposta(pixel, DimensioneIcona, DimensioneIcona,
                17, 17, Contorno);
        }
    }

    private static void DisegnaCadenza(Color32[] pixel)
    {
        DisegnaPatata(pixel, false);
        Color32 luce = new Color32(255, 224, 102, 255);
        Linea(pixel, 2, 14, 5, 14, Contorno, 2);
        Linea(pixel, 1, 10, 5, 10, Contorno, 2);
        Linea(pixel, 2, 6, 5, 6, Contorno, 2);
        Linea(pixel, 2, 14, 4, 14, luce, 1);
        Linea(pixel, 1, 10, 4, 10, luce, 1);
        Linea(pixel, 2, 6, 4, 6, luce, 1);
    }

    private static void DisegnaForma(
        Color32[] pixel,
        Func<int, int, bool> dentro,
        Color32 riempimento,
        Color32 contorno
    )
    {
        for (int y = 0; y < DimensioneIcona; y++)
        {
            for (int x = 0; x < DimensioneIcona; x++)
            {
                if (!dentro(x, y)) continue;

                bool bordo = !dentro(x - 1, y) || !dentro(x + 1, y) ||
                    !dentro(x, y - 1) || !dentro(x, y + 1);
                Imposta(pixel, DimensioneIcona, DimensioneIcona,
                    x, y, bordo ? contorno : riempimento);
            }
        }
    }

    private static void DisegnaChiodo(
        Color32[] pixel,
        int larghezza,
        int altezza,
        int x,
        int y
    )
    {
        Imposta(pixel, larghezza, altezza, x, y, Contorno);
        Imposta(pixel, larghezza, altezza, x + 1, y, Contorno);
        Imposta(pixel, larghezza, altezza, x, y + 1, Contorno);
        Imposta(pixel, larghezza, altezza, x + 1, y + 1, Paglia);
    }

    private static void ContornaRettangolo(
        Color32[] pixel,
        int xMin,
        int yMin,
        int xMax,
        int yMax,
        Color32 colore
    )
    {
        Linea(pixel, xMin, yMin, xMax, yMin, colore, 1);
        Linea(pixel, xMin, yMax, xMax, yMax, colore, 1);
        Linea(pixel, xMin, yMin, xMin, yMax, colore, 1);
        Linea(pixel, xMax, yMin, xMax, yMax, colore, 1);
    }

    private static void Rettangolo(
        Color32[] pixel,
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
                Imposta(pixel, DimensioneIcona, DimensioneIcona,
                    x, y, colore);
            }
        }
    }

    private static void Linea(
        Color32[] pixel,
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
                    Imposta(pixel, DimensioneIcona, DimensioneIcona,
                        x0 + ox, y0 + oy, colore);
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

    private static Color32[] NuovaTela(int larghezza, int altezza)
    {
        Color32[] pixel = new Color32[larghezza * altezza];
        for (int i = 0; i < pixel.Length; i++) pixel[i] = Trasparente;
        return pixel;
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
        if (x < 0 || y < 0 || x >= larghezza || y >= altezza) return;
        pixel[y * larghezza + x] = colore;
    }

    private static Sprite CreaSprite(
        string nome,
        Color32[] pixel,
        int larghezza,
        int altezza,
        float pixelPerUnita,
        float bordo
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

        Vector4 bordi = bordo > 0f
            ? new Vector4(bordo, bordo, bordo, bordo)
            : Vector4.zero;

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, larghezza, altezza),
            new Vector2(0.5f, 0.5f),
            pixelPerUnita,
            0,
            SpriteMeshType.FullRect,
            bordi
        );
        sprite.name = nome;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static float Quadrato(float valore)
    {
        return valore * valore;
    }
}
