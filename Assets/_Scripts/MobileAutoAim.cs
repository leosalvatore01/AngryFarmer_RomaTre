using UnityEngine;

/// <summary>Mira ai nemici visibili; i maialini sono bersagli solo senza volpi.</summary>
public sealed class MobileAutoAim
{
    private EnemyAI bersaglio;

    public bool ProvaDirezione(Camera camera, Vector3 origine, out Vector2 direzione)
    {
        direzione = Vector2.zero;
        if (camera == null) return false;
        EnemyAI migliore = null;
        float distanza = float.PositiveInfinity;
        // Enumeratori struct: niente array o allocazioni ad ogni aggiornamento della mira.
        var volpi = EnemyAI.EnumeraAttive();
        while (volpi.MoveNext())
        {
            EnemyAI volpe = volpi.Current;
            if (!BersaglioValido(volpe, camera)) continue;
            float candidata = ((Vector2)(volpe.transform.position - origine)).sqrMagnitude;
            // Piccola preferenza al bersaglio corrente: niente tremolio tra due volpi vicine.
            if (volpe == bersaglio) candidata *= 0.8f;
            if (candidata >= distanza) continue;
            migliore = volpe;
            distanza = candidata;
        }
        bersaglio = migliore;
        Transform obiettivo = migliore != null ? migliore.transform : null;
        if (obiettivo == null)
        {
            var maialini = MaialinoBonus.EnumeraAttivi();
            while (maialini.MoveNext())
            {
                MaialinoBonus maialino = maialini.Current;
                if (maialino == null || !maialino.isActiveAndEnabled ||
                    maialino.VitaCorrente <= 0 || !Visibile(camera, maialino.transform.position)) continue;
                float candidata = ((Vector2)(maialino.transform.position - origine)).sqrMagnitude;
                if (candidata >= distanza) continue;
                obiettivo = maialino.transform;
                distanza = candidata;
            }
        }
        if (obiettivo == null) return false;
        Vector2 scarto = obiettivo.position - origine;
        if (scarto.sqrMagnitude < 0.0001f) return false;
        direzione = scarto.normalized;
        return true;
    }

    public static bool BersaglioValido(EnemyAI volpe, Camera camera)
    {
        return volpe != null && volpe.isActiveAndEnabled && !volpe.IsDead &&
            !volpe.StaScavando && Visibile(camera, volpe.transform.position);
    }

    public static bool Visibile(Camera camera, Vector3 posizione)
    {
        if (camera == null) return false;
        Vector3 punto = camera.WorldToViewportPoint(posizione);
        return punto.z > 0 && punto.x >= 0.02f && punto.x <= 0.98f &&
            punto.y >= 0.02f && punto.y <= 0.98f;
    }
}
