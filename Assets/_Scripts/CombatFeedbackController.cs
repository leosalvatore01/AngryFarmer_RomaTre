using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class CombatFeedbackController : MonoBehaviour
{
    private const int NumeroSorgentiAudio = 6;
    private const int NumeroBurstInPool = 14;

    private static Sprite spriteMirino;

    private Canvas canvasMirino;
    private Image immagineMirino;
    private AudioSource[] sorgentiAudio;
    private AudioClip[] clipSparo;
    private AudioClip[] clipImpatto;
    private PixelImpactBurst[] poolBurst;
    private readonly System.Random casualitaCosmetica =
        new System.Random(1847);

    private CombatFeedbackSettings impostazioni;
    private int indiceSorgenteAudio;
    private int indiceBurst;
    private bool finestraAttiva = true;
    private bool cursoreDiSistemaNascosto;

    public static CombatFeedbackController Instance { get; private set; }

    public bool AudioAbilitato { get; set; }
    public bool VfxAbilitati { get; set; }
    public bool VibrazioneAbilitata { get; set; }

    public int SpariRegistrati { get; private set; }
    public int ImpattiRegistrati { get; private set; }
    public int SuoniSparoRiprodotti { get; private set; }
    public int SuoniImpattoRiprodotti { get; private set; }
    public int BurstEmessi { get; private set; }
    public int VibrazioniRichieste { get; private set; }
    public int UltimaVarianteAudio { get; private set; } = -1;
    public float UltimoPitch { get; private set; } = 1f;
    public bool MirinoVisibile =>
        immagineMirino != null && immagineMirino.enabled;

    public AudioClip[] ClipSparoDiagnostiche => clipSparo;
    public AudioClip[] ClipImpattoDiagnostiche => clipImpatto;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoStatico()
    {
        Instance = null;
        spriteMirino = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreaDopoCaricamentoScena()
    {
        CreaOTrova();
    }

    public static CombatFeedbackController CreaOTrova()
    {
        if (Instance != null) return Instance;

        CombatFeedbackController esistente =
            FindFirstObjectByType<CombatFeedbackController>();
        if (esistente != null)
        {
            Instance = esistente;
            return esistente;
        }

        GameObject oggetto = new GameObject("FeedbackCombattimento");
        return oggetto.AddComponent<CombatFeedbackController>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        impostazioni = GameBalanceConfig.Corrente.FeedbackCombattimento;
        AudioAbilitato = impostazioni.audioAttivo;
        VfxAbilitati = impostazioni.effettiVisiviAttivi;
        VibrazioneAbilitata = impostazioni.vibrazioneCameraAttiva;

        CreaMirino();
        CreaAudioProcedurale();
        CreaPoolBurst();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ImpostaVibrazione(!VibrazioneAbilitata);
            Debug.Log(
                "Vibrazione camera: " +
                (VibrazioneAbilitata ? "attiva" : "disattiva")
            );
        }
    }

    void LateUpdate()
    {
        AggiornaMirino();
    }

    public void RegistraSparo(
        Vector2 posizione,
        Vector2 direzione,
        bool potente,
        bool perforante
    )
    {
        SpariRegistrati++;
        RiproduciAudio(false, posizione, perforante);

        if (potente || perforante)
        {
            RichiediVibrazione(
                impostazioni.intensitaVibrazione *
                (perforante ? 1.15f : 0.82f),
                impostazioni.durataVibrazione
            );
        }
    }

    public void RegistraImpatto(
        Vector2 posizione,
        Vector2 direzione,
        SpriteRenderer rendererBersaglio,
        bool potente,
        bool perforazioneEffettiva
    )
    {
        ImpattiRegistrati++;

        if (VfxAbilitati)
        {
            PixelImpactBurst burst = OttieniBurst();
            if (burst != null)
            {
                int sortingLayer = rendererBersaglio != null
                    ? rendererBersaglio.sortingLayerID
                    : 0;
                int sortingOrder = rendererBersaglio != null
                    ? rendererBersaglio.sortingOrder + 5
                    : 5;

                burst.Attiva(
                    posizione,
                    direzione,
                    impostazioni.particelleImpatto,
                    impostazioni.durataParticelle,
                    sortingLayer,
                    sortingOrder,
                    potente,
                    perforazioneEffettiva
                );
                BurstEmessi++;
            }
        }

        RiproduciAudio(true, posizione, perforazioneEffettiva);

        if (perforazioneEffettiva)
        {
            RichiediVibrazione(
                impostazioni.intensitaVibrazione,
                impostazioni.durataVibrazione * 1.1f
            );
        }
    }

    public void ImpostaVibrazione(bool abilitata)
    {
        VibrazioneAbilitata = abilitata;

        Camera camera = Camera.main;
        CameraFollow follow = camera != null
            ? camera.GetComponent<CameraFollow>()
            : null;
        if (follow != null)
        {
            follow.ImpostaVibrazioneAbilitata(abilitata);
        }
    }

    private void RichiediVibrazione(float intensita, float durata)
    {
        if (!VibrazioneAbilitata || intensita <= 0f || durata <= 0f) return;

        Camera camera = Camera.main;
        CameraFollow follow = camera != null
            ? camera.GetComponent<CameraFollow>()
            : null;
        if (follow == null) return;

        follow.ImpostaVibrazioneAbilitata(VibrazioneAbilitata);
        if (follow.RichiediVibrazione(intensita, durata))
        {
            VibrazioniRichieste++;
        }
    }

    private void CreaMirino()
    {
        GameObject oggettoCanvas = new GameObject(
            "CanvasMirinoPixel",
            typeof(RectTransform),
            typeof(Canvas)
        );
        oggettoCanvas.transform.SetParent(transform, false);

        canvasMirino = oggettoCanvas.GetComponent<Canvas>();
        canvasMirino.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasMirino.overrideSorting = true;
        canvasMirino.sortingOrder = 32000;
        canvasMirino.pixelPerfect = true;

        GameObject oggettoMirino = new GameObject(
            "MirinoPixel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        oggettoMirino.transform.SetParent(oggettoCanvas.transform, false);

        immagineMirino = oggettoMirino.GetComponent<Image>();
        immagineMirino.sprite = OttieniSpriteMirino();
        immagineMirino.preserveAspect = true;
        immagineMirino.raycastTarget = false;
        immagineMirino.color = new Color(1f, 0.9f, 0.54f, 1f);
        immagineMirino.enabled = false;

        RectTransform rect = immagineMirino.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        float dimensione = Mathf.Clamp(
            impostazioni.dimensioneMirino,
            14f,
            48f
        );
        rect.sizeDelta = new Vector2(dimensione, dimensione);
    }

    private void AggiornaMirino()
    {
        if (immagineMirino == null) return;

        Vector3 posizioneMouse = Input.mousePosition;
        bool dentroFinestra =
            posizioneMouse.x >= 0f && posizioneMouse.y >= 0f &&
            posizioneMouse.x <= Screen.width &&
            posizioneMouse.y <= Screen.height;
        bool gameplayAttivo =
            GameManager.instance != null &&
            GameManager.instance.GameplayAttivo;
        bool visibile =
            impostazioni.mirinoPixelAttivo &&
            finestraAttiva &&
            dentroFinestra &&
            gameplayAttivo;

        immagineMirino.enabled = visibile;
        if (visibile)
        {
            immagineMirino.rectTransform.position = posizioneMouse;
            immagineMirino.color =
                MirinoSopraBersaglio(posizioneMouse)
                ? new Color(1f, 0.46f, 0.24f, 1f)
                : new Color(1f, 0.9f, 0.54f, 1f);
        }

        ImpostaVisibilitaCursoreSistema(!visibile);
    }

    private static bool MirinoSopraBersaglio(Vector3 posizioneSchermo)
    {
        Camera camera = Camera.main;
        if (camera == null) return false;

        posizioneSchermo.z = Mathf.Abs(camera.transform.position.z);
        Vector2 posizioneMondo = camera.ScreenToWorldPoint(posizioneSchermo);
        Collider2D collider = Physics2D.OverlapPoint(posizioneMondo);
        return collider != null &&
               collider.GetComponentInParent<IDanneggiabile>() != null;
    }

    private void ImpostaVisibilitaCursoreSistema(bool visibile)
    {
        if (Application.isBatchMode) return;
        if (Cursor.visible == visibile &&
            cursoreDiSistemaNascosto == !visibile) return;

        Cursor.visible = visibile;
        cursoreDiSistemaNascosto = !visibile;
    }

    private void CreaAudioProcedurale()
    {
        clipSparo = new AudioClip[3];
        clipImpatto = new AudioClip[3];
        for (int i = 0; i < 3; i++)
        {
            clipSparo[i] = CreaClipSparo(i);
            clipImpatto[i] = CreaClipImpatto(i);
        }

        sorgentiAudio = new AudioSource[NumeroSorgentiAudio];
        for (int i = 0; i < sorgentiAudio.Length; i++)
        {
            GameObject oggetto = new GameObject("AudioFeedback_" + i);
            oggetto.transform.SetParent(transform, false);
            AudioSource sorgente = oggetto.AddComponent<AudioSource>();
            sorgente.playOnAwake = false;
            sorgente.loop = false;
            sorgente.dopplerLevel = 0f;
            sorgente.ignoreListenerPause = true;
            sorgentiAudio[i] = sorgente;
        }
    }

    private void RiproduciAudio(
        bool impatto,
        Vector2 posizione,
        bool varianteSpeciale
    )
    {
        if (!AudioAbilitato || sorgentiAudio == null) return;

        AudioClip[] clip = impatto ? clipImpatto : clipSparo;
        if (clip == null || clip.Length == 0) return;

        int variante = varianteSpeciale
            ? clip.Length - 1
            : casualitaCosmetica.Next(0, clip.Length);
        float variazione = Mathf.Max(0f, impostazioni.variazioneIntonazione);
        float casuale = (float)casualitaCosmetica.NextDouble() * 2f - 1f;
        float pitch = 1f + casuale * variazione;
        if (varianteSpeciale) pitch += 0.045f;

        AudioSource sorgente = sorgentiAudio[indiceSorgenteAudio];
        indiceSorgenteAudio =
            (indiceSorgenteAudio + 1) % sorgentiAudio.Length;

        sorgente.Stop();
        sorgente.transform.position = posizione;
        sorgente.clip = clip[variante];
        sorgente.pitch = pitch;
        sorgente.volume = impatto
            ? impostazioni.volumeImpatto
            : impostazioni.volumeSparo;
        sorgente.spatialBlend = impatto ? 0.22f : 0f;
        sorgente.Play();

        UltimaVarianteAudio = variante;
        UltimoPitch = pitch;
        if (impatto) SuoniImpattoRiprodotti++;
        else SuoniSparoRiprodotti++;
    }

    private static AudioClip CreaClipSparo(int variante)
    {
        const int frequenzaCampionamento = 22050;
        float durata = 0.075f + variante * 0.007f;
        int campioni = Mathf.CeilToInt(durata * frequenzaCampionamento);
        float[] dati = new float[campioni];
        uint statoRumore = (uint)(971 + variante * 431);
        float fase = 0f;

        for (int i = 0; i < campioni; i++)
        {
            float t = i / (float)frequenzaCampionamento;
            float progresso = i / (float)Mathf.Max(1, campioni - 1);
            float frequenza = Mathf.Lerp(
                205f + variante * 17f,
                76f + variante * 8f,
                progresso
            );
            fase += frequenza / frequenzaCampionamento;
            statoRumore = statoRumore * 1664525u + 1013904223u;
            float rumore = ((statoRumore >> 9) & 0x7FFFFF) /
                           4194303.5f - 1f;
            float inviluppo = (1f - progresso);
            inviluppo *= inviluppo;
            float tono = Mathf.Sin(fase * Mathf.PI * 2f);
            float attacco = Mathf.Clamp01(t / 0.009f);
            float campione =
                (tono * 0.72f + rumore * 0.18f * (1f - progresso)) *
                inviluppo * attacco;
            dati[i] = Mathf.Round(campione * 32f) / 32f;
        }

        AudioClip clip = AudioClip.Create(
            "SparoPatata_" + (variante + 1),
            campioni,
            1,
            frequenzaCampionamento,
            false
        );
        clip.SetData(dati, 0);
        clip.hideFlags = HideFlags.HideAndDontSave;
        return clip;
    }

    private static AudioClip CreaClipImpatto(int variante)
    {
        const int frequenzaCampionamento = 22050;
        float durata = 0.065f + variante * 0.009f;
        int campioni = Mathf.CeilToInt(durata * frequenzaCampionamento);
        float[] dati = new float[campioni];
        uint statoRumore = (uint)(2467 + variante * 619);
        float filtro = 0f;

        for (int i = 0; i < campioni; i++)
        {
            float progresso = i / (float)Mathf.Max(1, campioni - 1);
            statoRumore = statoRumore * 1103515245u + 12345u;
            float rumore = ((statoRumore >> 9) & 0x7FFFFF) /
                           4194303.5f - 1f;
            filtro = Mathf.Lerp(filtro, rumore, 0.22f + variante * 0.025f);
            float tonfo = Mathf.Sin(
                progresso * Mathf.PI * (3.1f + variante * 0.35f)
            );
            float inviluppo = 1f - progresso;
            inviluppo *= inviluppo;
            float campione =
                (filtro * 0.58f + tonfo * 0.34f) * inviluppo;
            dati[i] = Mathf.Round(campione * 32f) / 32f;
        }

        AudioClip clip = AudioClip.Create(
            "ImpattoPatata_" + (variante + 1),
            campioni,
            1,
            frequenzaCampionamento,
            false
        );
        clip.SetData(dati, 0);
        clip.hideFlags = HideFlags.HideAndDontSave;
        return clip;
    }

    private void CreaPoolBurst()
    {
        poolBurst = new PixelImpactBurst[NumeroBurstInPool];
        for (int i = 0; i < poolBurst.Length; i++)
        {
            GameObject oggetto = new GameObject("BurstImpatto_" + i);
            oggetto.transform.SetParent(transform, false);
            PixelImpactBurst burst = oggetto.AddComponent<PixelImpactBurst>();
            poolBurst[i] = burst;
            oggetto.SetActive(false);
        }
    }

    private PixelImpactBurst OttieniBurst()
    {
        if (poolBurst == null || poolBurst.Length == 0) return null;

        for (int i = 0; i < poolBurst.Length; i++)
        {
            int indice = (indiceBurst + i) % poolBurst.Length;
            if (!poolBurst[indice].gameObject.activeSelf)
            {
                indiceBurst = (indice + 1) % poolBurst.Length;
                return poolBurst[indice];
            }
        }

        PixelImpactBurst riutilizzato = poolBurst[indiceBurst];
        indiceBurst = (indiceBurst + 1) % poolBurst.Length;
        riutilizzato.gameObject.SetActive(false);
        return riutilizzato;
    }

    private static Sprite OttieniSpriteMirino()
    {
        if (spriteMirino != null) return spriteMirino;

        const int dimensione = 21;
        Color32 trasparente = new Color32(0, 0, 0, 0);
        Color32 scuro = new Color32(70, 35, 19, 255);
        Color32 crema = new Color32(255, 222, 128, 255);
        Color32 luce = new Color32(255, 246, 198, 255);
        Color32[] pixel = new Color32[dimensione * dimensione];
        for (int i = 0; i < pixel.Length; i++) pixel[i] = trasparente;

        int centro = dimensione / 2;
        for (int distanza = 4; distanza <= 9; distanza++)
        {
            ImpostaPixel(pixel, dimensione, centro + distanza, centro, scuro);
            ImpostaPixel(pixel, dimensione, centro - distanza, centro, scuro);
            ImpostaPixel(pixel, dimensione, centro, centro + distanza, scuro);
            ImpostaPixel(pixel, dimensione, centro, centro - distanza, scuro);
        }
        for (int distanza = 5; distanza <= 8; distanza++)
        {
            ImpostaPixel(pixel, dimensione, centro + distanza, centro, crema);
            ImpostaPixel(pixel, dimensione, centro - distanza, centro, crema);
            ImpostaPixel(pixel, dimensione, centro, centro + distanza, crema);
            ImpostaPixel(pixel, dimensione, centro, centro - distanza, crema);
        }
        ImpostaPixel(pixel, dimensione, centro, centro, luce);
        ImpostaPixel(pixel, dimensione, centro + 1, centro, scuro);
        ImpostaPixel(pixel, dimensione, centro - 1, centro, scuro);
        ImpostaPixel(pixel, dimensione, centro, centro + 1, scuro);
        ImpostaPixel(pixel, dimensione, centro, centro - 1, scuro);

        Texture2D texture = new Texture2D(
            dimensione,
            dimensione,
            TextureFormat.RGBA32,
            false
        );
        texture.name = "TextureMirinoPixel";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels32(pixel);
        texture.Apply(false, true);

        spriteMirino = Sprite.Create(
            texture,
            new Rect(0f, 0f, dimensione, dimensione),
            new Vector2(0.5f, 0.5f),
            dimensione,
            0,
            SpriteMeshType.FullRect
        );
        spriteMirino.name = "MirinoPixel";
        spriteMirino.hideFlags = HideFlags.HideAndDontSave;
        return spriteMirino;
    }

    private static void ImpostaPixel(
        Color32[] pixel,
        int larghezza,
        int x,
        int y,
        Color32 colore
    )
    {
        if (x < 0 || y < 0 || x >= larghezza || y >= larghezza) return;
        pixel[y * larghezza + x] = colore;
    }

    void OnApplicationFocus(bool attiva)
    {
        finestraAttiva = attiva;
        if (!attiva)
        {
            if (immagineMirino != null) immagineMirino.enabled = false;
            ImpostaVisibilitaCursoreSistema(true);
        }
    }

    void OnDisable()
    {
        if (Instance != this) return;
        if (immagineMirino != null) immagineMirino.enabled = false;
        ImpostaVisibilitaCursoreSistema(true);
    }

    void OnDestroy()
    {
        if (Instance != this) return;
        Instance = null;
        ImpostaVisibilitaCursoreSistema(true);
    }
}

public sealed class CombatHitFeedback2D : MonoBehaviour
{
    private static Material materialeFlash;

    private Transform pivotFeedback;
    private Transform grafica;
    private SpriteRenderer rendererSorgente;
    private SpriteRenderer rendererFlash;
    private Vector2 direzioneRinculo;
    private float tempoRinculo;
    private float durataRinculo;
    private float distanzaRinculo;
    private float tempoFlash;
    private float durataFlash;
    private bool effettoPerforante;
    private float moltiplicatoreRinculo = 1f;

    public SpriteRenderer RendererSorgente => rendererSorgente;
    public bool FeedbackAttivo => tempoRinculo > 0f || tempoFlash > 0f;
    public Vector2 UltimaDirezioneRinculo => direzioneRinculo;
    public float MoltiplicatoreRinculo => moltiplicatoreRinculo;

    public void ConfiguraMoltiplicatoreRinculo(float valore)
    {
        moltiplicatoreRinculo = Mathf.Clamp(valore, 0f, 2f);
    }

    public void Configura(
        Transform trasformazioneGrafica,
        SpriteRenderer rendererVisibile
    )
    {
        if (trasformazioneGrafica == null || rendererVisibile == null) return;
        if (grafica == trasformazioneGrafica && rendererSorgente == rendererVisibile)
        {
            return;
        }

        grafica = trasformazioneGrafica;
        rendererSorgente = rendererVisibile;

        GameObject oggettoPivot = new GameObject("FeedbackImpatto");
        oggettoPivot.layer = gameObject.layer;
        pivotFeedback = oggettoPivot.transform;
        pivotFeedback.SetParent(transform, false);
        grafica.SetParent(pivotFeedback, false);

        GameObject oggettoFlash = new GameObject("FlashChiaroImpatto");
        oggettoFlash.layer = gameObject.layer;
        oggettoFlash.transform.SetParent(grafica, false);
        rendererFlash = oggettoFlash.AddComponent<SpriteRenderer>();
        rendererFlash.sharedMaterial = OttieniMaterialeFlash();
        rendererFlash.enabled = false;
        SincronizzaRendererFlash();
    }

    public void Riproduci(
        Vector2 direzioneColpo,
        bool potente,
        bool perforante
    )
    {
        CombatFeedbackSettings impostazioni =
            GameBalanceConfig.Corrente.FeedbackCombattimento;
        CombatFeedbackController controller = CombatFeedbackController.Instance;
        if (!impostazioni.effettiVisiviAttivi ||
            (controller != null && !controller.VfxAbilitati))
        {
            return;
        }

        direzioneRinculo = direzioneColpo.sqrMagnitude > 0.0001f
            ? direzioneColpo.normalized
            : Vector2.right;
        distanzaRinculo = impostazioni.distanzaRinculoBersaglio *
            (potente ? 1.2f : 1f) * moltiplicatoreRinculo;
        durataRinculo = Mathf.Max(
            0.03f,
            impostazioni.durataRinculoBersaglio
        );
        durataFlash = Mathf.Max(0.03f, impostazioni.durataFlashBersaglio);
        tempoRinculo = durataRinculo;
        tempoFlash = durataFlash;
        effettoPerforante = perforante;

        AggiornaFeedbackVisivo(0f);
    }

    void Update()
    {
        AggiornaFeedbackVisivo(Time.unscaledDeltaTime);
    }

    void LateUpdate()
    {
        SincronizzaRendererFlash();
    }

    private void AggiornaFeedbackVisivo(float delta)
    {
        if (tempoRinculo > 0f)
        {
            tempoRinculo = Mathf.Max(0f, tempoRinculo - delta);
            float t = tempoRinculo / Mathf.Max(0.01f, durataRinculo);
            float impulso = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI * 0.5f);
            if (pivotFeedback != null)
            {
                pivotFeedback.localPosition =
                    (Vector3)(direzioneRinculo * distanzaRinculo * impulso);
            }
        }
        else if (pivotFeedback != null)
        {
            pivotFeedback.localPosition = Vector3.zero;
        }

        if (tempoFlash > 0f)
        {
            tempoFlash = Mathf.Max(0f, tempoFlash - delta);
            if (rendererFlash != null)
            {
                float alpha = Mathf.Clamp01(
                    tempoFlash / Mathf.Max(0.01f, durataFlash)
                );
                Color colore = effettoPerforante
                    ? new Color(1f, 0.84f, 0.28f, alpha)
                    : new Color(1f, 0.96f, 0.72f, alpha);
                rendererFlash.color = colore;
                rendererFlash.enabled = true;
            }
        }
        else if (rendererFlash != null)
        {
            rendererFlash.enabled = false;
        }
    }

    private void SincronizzaRendererFlash()
    {
        if (rendererFlash == null || rendererSorgente == null) return;

        rendererFlash.sprite = rendererSorgente.sprite;
        rendererFlash.flipX = rendererSorgente.flipX;
        rendererFlash.flipY = rendererSorgente.flipY;
        rendererFlash.drawMode = rendererSorgente.drawMode;
        rendererFlash.size = rendererSorgente.size;
        rendererFlash.maskInteraction = rendererSorgente.maskInteraction;
        rendererFlash.spriteSortPoint = rendererSorgente.spriteSortPoint;
        rendererFlash.sortingLayerID = rendererSorgente.sortingLayerID;
        rendererFlash.sortingOrder = rendererSorgente.sortingOrder + 6;
    }

    private static Material OttieniMaterialeFlash()
    {
        if (materialeFlash != null) return materialeFlash;

        Shader shader = Resources.Load<Shader>("SpriteHitFlash");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        materialeFlash = new Material(shader);
        materialeFlash.name = "MaterialeFlashImpattoRuntime";
        materialeFlash.hideFlags = HideFlags.HideAndDontSave;
        return materialeFlash;
    }

    void OnDisable()
    {
        tempoRinculo = 0f;
        tempoFlash = 0f;
        if (pivotFeedback != null) pivotFeedback.localPosition = Vector3.zero;
        if (rendererFlash != null) rendererFlash.enabled = false;
    }
}

public sealed class PixelImpactBurst : MonoBehaviour
{
    private const int NumeroMassimoParticelle = 8;
    private static Sprite spritePixel;

    private SpriteRenderer[] rendererParticelle;
    private Vector2[] velocitaParticelle;
    private float durata;
    private float tempo;

    void Awake()
    {
        rendererParticelle = new SpriteRenderer[NumeroMassimoParticelle];
        velocitaParticelle = new Vector2[NumeroMassimoParticelle];

        for (int i = 0; i < NumeroMassimoParticelle; i++)
        {
            GameObject particella = new GameObject("Pixel_" + i);
            particella.transform.SetParent(transform, false);
            SpriteRenderer renderer = particella.AddComponent<SpriteRenderer>();
            renderer.sprite = OttieniSpritePixel();
            renderer.enabled = false;
            rendererParticelle[i] = renderer;
        }
    }

    public void Attiva(
        Vector2 posizione,
        Vector2 direzione,
        int numeroParticelle,
        float nuovaDurata,
        int sortingLayer,
        int sortingOrder,
        bool potente,
        bool perforante
    )
    {
        gameObject.SetActive(true);
        transform.position = posizione;
        transform.rotation = Quaternion.identity;
        tempo = 0f;
        durata = Mathf.Max(0.05f, nuovaDurata);

        Vector2 avanti = direzione.sqrMagnitude > 0.0001f
            ? direzione.normalized
            : Vector2.right;
        int quantita = Mathf.Clamp(
            numeroParticelle + (potente ? 1 : 0) + (perforante ? 1 : 0),
            1,
            NumeroMassimoParticelle
        );

        for (int i = 0; i < rendererParticelle.Length; i++)
        {
            SpriteRenderer renderer = rendererParticelle[i];
            bool attiva = i < quantita;
            renderer.enabled = attiva;
            if (!attiva) continue;

            float apertura = quantita > 1
                ? Mathf.Lerp(-72f, 72f, i / (float)(quantita - 1))
                : 0f;
            Vector2 direzioneParticella =
                Quaternion.Euler(0f, 0f, apertura) * avanti;
            float velocita = (perforante ? 2.7f : 1.75f) +
                              (i % 3) * 0.23f;
            velocitaParticelle[i] = direzioneParticella * velocita;
            renderer.transform.localPosition = Vector3.zero;
            renderer.transform.localRotation = Quaternion.Euler(
                0f,
                0f,
                apertura
            );
            float scala = 0.9f + (i % 2) * 0.45f;
            renderer.transform.localScale = new Vector3(
                perforante ? scala * 1.8f : scala,
                scala,
                1f
            );
            renderer.sortingLayerID = sortingLayer;
            renderer.sortingOrder = sortingOrder + (i % 2);
            renderer.color = ColoreParticella(i, potente, perforante, 1f);
        }
    }

    void Update()
    {
        float delta = Time.unscaledDeltaTime;
        tempo += delta;
        float progresso = Mathf.Clamp01(tempo / durata);

        for (int i = 0; i < rendererParticelle.Length; i++)
        {
            SpriteRenderer renderer = rendererParticelle[i];
            if (!renderer.enabled) continue;

            renderer.transform.localPosition +=
                (Vector3)(velocitaParticelle[i] * delta);
            velocitaParticelle[i] *= Mathf.Exp(-8f * delta);
            Color colore = renderer.color;
            colore.a = 1f - progresso;
            renderer.color = colore;
        }

        if (tempo >= durata)
        {
            gameObject.SetActive(false);
        }
    }

    private static Color ColoreParticella(
        int indice,
        bool potente,
        bool perforante,
        float alpha
    )
    {
        if (perforante)
        {
            Color[] oro =
            {
                new Color(1f, 0.96f, 0.62f, alpha),
                new Color(1f, 0.72f, 0.16f, alpha),
                new Color(0.76f, 0.32f, 0.09f, alpha)
            };
            return oro[indice % oro.Length];
        }

        if (potente)
        {
            Color[] caldo =
            {
                new Color(1f, 0.86f, 0.45f, alpha),
                new Color(0.76f, 0.34f, 0.12f, alpha),
                new Color(1f, 0.95f, 0.72f, alpha)
            };
            return caldo[indice % caldo.Length];
        }

        Color[] terra =
        {
            new Color(0.64f, 0.34f, 0.16f, alpha),
            new Color(0.88f, 0.65f, 0.34f, alpha),
            new Color(1f, 0.91f, 0.66f, alpha)
        };
        return terra[indice % terra.Length];
    }

    private static Sprite OttieniSpritePixel()
    {
        if (spritePixel != null) return spritePixel;

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.name = "TexturePixelImpatto";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels(new[]
        {
            Color.white, Color.white,
            Color.white, Color.white
        });
        texture.Apply(false, true);

        spritePixel = Sprite.Create(
            texture,
            new Rect(0f, 0f, 2f, 2f),
            new Vector2(0.5f, 0.5f),
            32f,
            0,
            SpriteMeshType.FullRect
        );
        spritePixel.name = "PixelImpatto";
        spritePixel.hideFlags = HideFlags.HideAndDontSave;
        return spritePixel;
    }
}
