using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisualController : MonoBehaviour
{
    public float frameAlSecondo = 6f;

    private SpriteRenderer spriteRenderer;
    private PlayerMovement movimento;
    private Sprite[] frameCamminata;

    private int direzioneCorrente = 0;
    private float timerAnimazione;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movimento = GetComponent<PlayerMovement>();

        frameCamminata = Resources.LoadAll<Sprite>("FarmerWalk-v1");

        Array.Sort(frameCamminata, (a, b) =>
            NumeroFrame(a.name).CompareTo(NumeroFrame(b.name))
        );

        if (frameCamminata.Length < 16)
        {
            Debug.LogError("FarmerWalk-v1 deve contenere 16 frame.");
        }
    }

    void Update()
    {
        if (frameCamminata.Length < 16 || movimento == null) return;

        Vector2 direzione = movimento.DirezioneMovimento;
        bool staCamminando = direzione.sqrMagnitude > 0.01f;

        if (staCamminando)
        {
            direzioneCorrente = OttieniDirezione(direzione);
            timerAnimazione += Time.deltaTime;
        }
        else
        {
            timerAnimazione = 0f;
        }

        int passo = staCamminando
            ? Mathf.FloorToInt(timerAnimazione * frameAlSecondo) % 2
            : 0;

        int indiceSprite = direzioneCorrente * 2 + passo;
        spriteRenderer.sprite = frameCamminata[indiceSprite];
    }

    int OttieniDirezione(Vector2 direzione)
    {
        float angolo = Mathf.Atan2(direzione.y, direzione.x) * Mathf.Rad2Deg;

        if (angolo >= -112.5f && angolo < -67.5f) return 0;  // Sud
        if (angolo >= -67.5f && angolo < -22.5f) return 1;   // Sud-est
        if (angolo >= -22.5f && angolo < 22.5f) return 2;    // Est
        if (angolo >= 22.5f && angolo < 67.5f) return 3;     // Nord-est
        if (angolo >= 67.5f && angolo < 112.5f) return 4;    // Nord
        if (angolo >= 112.5f && angolo < 157.5f) return 5;   // Nord-ovest
        if (angolo >= 157.5f || angolo < -157.5f) return 6;  // Ovest

        return 7; // Sud-ovest
    }

    int NumeroFrame(string nome)
    {
        string[] parti = nome.Split('_');

        if (parti.Length > 1 &&
            int.TryParse(parti[parti.Length - 1], out int numero))
        {
            return numero;
        }

        return 0;
    }
}