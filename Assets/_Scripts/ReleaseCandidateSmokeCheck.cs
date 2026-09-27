using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controllo minimale attivato solo dalla build di verifica. Permette di
/// provare l'avvio con una cartella salvataggi vuota e termina da solo.
/// </summary>
public sealed class ReleaseCandidateSmokeCheck : MonoBehaviour
{
    private const string Argomento = "-angryFarmerReleaseSmoke";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AvviaSeRichiesto()
    {
        if (!Array.Exists(
                Environment.GetCommandLineArgs(),
                valore => string.Equals(
                    valore,
                    Argomento,
                    StringComparison.OrdinalIgnoreCase
                )))
        {
            return;
        }

        GameObject oggetto = new GameObject("ReleaseCandidateSmokeCheck");
        DontDestroyOnLoad(oggetto);
        oggetto.AddComponent<ReleaseCandidateSmokeCheck>();
    }

    private IEnumerator Start()
    {
        yield return null;
        yield return null;

        List<string> errori = new List<string>();
        if (SceneManager.GetActiveScene().name !=
            MenuInizialeController.NomeScenaMenu)
        {
            errori.Add("la prima scena non e il menu iniziale");
        }
        if (!MenuInizialeController.Attivo ||
            !MenuInizialeController.Instance.InterfacciaCostruita)
        {
            errori.Add("il menu iniziale non ha costruito l'interfaccia");
        }

        SaveService.InizializzaProfiloOspite();
        if (SaveService.Profilo == null ||
            SaveService.Dispositivo == null)
        {
            errori.Add("il salvataggio locale non e stato inizializzato");
        }

        if (errori.Count == 0)
        {
            Debug.Log("[RELEASE_SMOKE_OK] Avvio pulito verificato.");
            Application.Quit(0);
            yield break;
        }

        Debug.LogError("[RELEASE_SMOKE_FAIL] " + string.Join("; ", errori));
        Application.Quit(2);
    }
}
