using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerDodgeFeedback : MonoBehaviour
{
    private static Sprite spriteScia;

    private SpriteRenderer rendererScia;
    private SpriteRenderer rendererGiocatore;
    private float durata;
    private float tempoRimasto;

    public bool Visibile => rendererScia != null && rendererScia.enabled;

    public static PlayerDodgeFeedback AggiungiOTrova(GameObject giocatore)
    {
        if (giocatore == null) return null;
        PlayerDodgeFeedback esistente =
            giocatore.GetComponent<PlayerDodgeFeedback>();
        return esistente != null
            ? esistente
            : giocatore.AddComponent<PlayerDodgeFeedback>();
    }

    private void Awake()
    {
        CreaScia();
    }

    public void Avvia(Vector2 direzione, float nuovaDurata)
    {
        if (rendererScia == null) CreaScia();
        durata = Mathf.Max(0.05f, nuovaDurata);
        tempoRimasto = durata;
        float angolo = Mathf.Atan2(direzione.y, direzione.x) *
            Mathf.Rad2Deg;
        rendererScia.transform.localRotation = Quaternion.Euler(
            0f,
            0f,
            angolo
        );
        rendererScia.enabled = true;
        AggiornaAspetto();
    }

    private void Update()
    {
        if (tempoRimasto <= 0f)
        {
            if (rendererScia != null) rendererScia.enabled = false;
            return;
        }

        tempoRimasto = Mathf.Max(0f, tempoRimasto - Time.deltaTime);
        AggiornaAspetto();
    }

    private void CreaScia()
    {
        if (rendererScia != null) return;
        GameObject scia = new GameObject("SciaSchivata");
        scia.layer = gameObject.layer;
        scia.transform.SetParent(transform, false);
        scia.transform.localPosition = Vector3.zero;
        rendererScia = scia.AddComponent<SpriteRenderer>();
        rendererScia.sprite = OttieniSpriteScia();
        rendererScia.enabled = false;
        TrovaRendererGiocatore();
    }

    private void AggiornaAspetto()
    {
        if (rendererScia == null || tempoRimasto <= 0f) return;
        if (rendererGiocatore == null) TrovaRendererGiocatore();
        if (rendererGiocatore != null)
        {
            rendererScia.sortingLayerID = rendererGiocatore.sortingLayerID;
            rendererScia.sortingOrder = rendererGiocatore.sortingOrder - 1;
        }

        float progresso = durata > 0f
            ? Mathf.Clamp01(tempoRimasto / durata)
            : 0f;
        rendererScia.color = new Color(
            0.34f,
            0.94f,
            1f,
            Mathf.Lerp(0f, 0.88f, progresso)
        );
        rendererScia.transform.localScale = new Vector3(
            Mathf.Lerp(0.75f, 1.35f, progresso),
            Mathf.Lerp(0.7f, 1f, progresso),
            1f
        );
        rendererScia.enabled = true;
    }

    private void TrovaRendererGiocatore()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(
            true
        );
        foreach (SpriteRenderer candidato in renderers)
        {
            if (candidato == null || candidato == rendererScia) continue;
            if (candidato.gameObject.name == "ScudoInvulnerabilita" ||
                candidato.gameObject.name == "LampoSparo")
            {
                continue;
            }
            rendererGiocatore = candidato;
            break;
        }
    }

    private static Sprite OttieniSpriteScia()
    {
        if (spriteScia != null) return spriteScia;

        const int larghezza = 32;
        const int altezza = 14;
        Color32[] pixel = new Color32[larghezza * altezza];
        Color32 trasparente = new Color32(0, 0, 0, 0);
        Color32 pieno = new Color32(255, 255, 255, 255);
        for (int i = 0; i < pixel.Length; i++) pixel[i] = trasparente;

        DisegnaLinea(pixel, larghezza, 2, 4, 21, 4, pieno);
        DisegnaLinea(pixel, larghezza, 0, 7, 26, 7, pieno);
        DisegnaLinea(pixel, larghezza, 5, 10, 19, 10, pieno);
        DisegnaLinea(pixel, larghezza, 24, 5, 30, 7, pieno);
        DisegnaLinea(pixel, larghezza, 24, 9, 30, 7, pieno);

        Texture2D texture = new Texture2D(
            larghezza,
            altezza,
            TextureFormat.RGBA32,
            false
        );
        texture.name = "TextureSciaSchivata";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels32(pixel);
        texture.Apply(false, true);

        spriteScia = Sprite.Create(
            texture,
            new Rect(0f, 0f, larghezza, altezza),
            new Vector2(0.82f, 0.5f),
            32f,
            0,
            SpriteMeshType.FullRect
        );
        spriteScia.name = "SciaSchivataPixel";
        spriteScia.hideFlags = HideFlags.HideAndDontSave;
        return spriteScia;
    }

    private static void DisegnaLinea(
        Color32[] pixel,
        int larghezza,
        int x0,
        int y0,
        int x1,
        int y1,
        Color32 colore
    )
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int errore = dx + dy;
        while (true)
        {
            if (x0 >= 0 && y0 >= 0 &&
                x0 < larghezza && y0 < pixel.Length / larghezza)
            {
                pixel[y0 * larghezza + x0] = colore;
            }
            if (x0 == x1 && y0 == y1) break;
            int doppioErrore = errore * 2;
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
}
