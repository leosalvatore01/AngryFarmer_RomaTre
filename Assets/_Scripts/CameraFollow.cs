using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Range(0.1f, 0.9f)]
    public float zonaMortaOrizzontale = 0.5f;

    [Range(0.1f, 0.9f)]
    public float zonaMortaVerticale = 0.5f;

    public float tempoFluidita = 0.25f;

    private Transform giocatore;
    private Camera cameraPrincipale;
    private Vector3 velocitaCamera;

    void Start()
    {
        cameraPrincipale = GetComponent<Camera>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            giocatore = player.transform;

            transform.position = new Vector3(
                giocatore.position.x,
                giocatore.position.y,
                transform.position.z
            );
        }
    }

    void LateUpdate()
    {
        if (giocatore == null) return;

        float mezzaLarghezza = cameraPrincipale.orthographicSize * cameraPrincipale.aspect;
        float mezzaAltezza = cameraPrincipale.orthographicSize;

        // Con valore 0.5: il contadino può muoversi
        // nel 50% centrale della schermata senza spostare la camera.
        float mezzaZonaMortaX = mezzaLarghezza * zonaMortaOrizzontale;
        float mezzaZonaMortaY = mezzaAltezza * zonaMortaVerticale;

        Vector3 destinazione = transform.position;

        if (giocatore.position.x < transform.position.x - mezzaZonaMortaX)
        {
            destinazione.x = giocatore.position.x + mezzaZonaMortaX;
        }
        else if (giocatore.position.x > transform.position.x + mezzaZonaMortaX)
        {
            destinazione.x = giocatore.position.x - mezzaZonaMortaX;
        }

        if (giocatore.position.y < transform.position.y - mezzaZonaMortaY)
        {
            destinazione.y = giocatore.position.y + mezzaZonaMortaY;
        }
        else if (giocatore.position.y > transform.position.y + mezzaZonaMortaY)
        {
            destinazione.y = giocatore.position.y - mezzaZonaMortaY;
        }

        Vector3 posizioneFluida = Vector3.SmoothDamp(
            transform.position,
            destinazione,
            ref velocitaCamera,
            tempoFluidita
        );

        transform.position = new Vector3(
            posizioneFluida.x,
            posizioneFluida.y,
            transform.position.z
        );
    }
}