using System;
using System.Collections;
using System.Net.Mail;
using UnityEngine;

public enum OperazioneAccesso { Accedi, CreaAccount, RecuperaPassword }

public sealed class EsitoAccesso
{
    public bool Riuscito;
    public string UserId = "";
    public string Email = "";
    public string Messaggio = "";
}

/// <summary>Confine per una futura integrazione. La versione corrente non usa rete o credenziali persistenti.</summary>
public interface ILoginProvider
{
    bool Disponibile { get; }
    IEnumerator Esegui(OperazioneAccesso operazione, string email, string password, Action<EsitoAccesso> completato);
}

public sealed class LoginDisattivatoProvider : ILoginProvider
{
    public bool Disponibile => false;
    public IEnumerator Esegui(OperazioneAccesso operazione, string email, string password, Action<EsitoAccesso> completato)
    {
        completato(new EsitoAccesso { Messaggio = "Accesso online non attivo. Per ora puoi giocare come ospite." });
        yield break;
    }
}

public static class ValidazioneAccesso
{
    public static bool EmailValida(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254) return false;
        try { var indirizzo = new MailAddress(email.Trim()); return indirizzo.Address == email.Trim() && indirizzo.Host.Contains("."); }
        catch (FormatException) { return false; }
    }

    public static string Errore(OperazioneAccesso operazione, string email, string password, string conferma)
    {
        if (!EmailValida(email)) return "Inserisci un indirizzo email valido.";
        if (operazione == OperazioneAccesso.RecuperaPassword) return "";
        if (string.IsNullOrEmpty(password)) return "Inserisci la password.";
        if (operazione == OperazioneAccesso.CreaAccount)
        {
            if (password.Length < 8) return "Scegli una password di almeno 8 caratteri.";
            if (password != conferma) return "Le due password non coincidono.";
        }
        return "";
    }
}

public static class LoginSession
{
    public static bool AccessoLocaleScelto { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Azzera() { AccessoLocaleScelto = false; }
    public static void ContinuaComeOspite() { AccessoLocaleScelto = true; }
}
