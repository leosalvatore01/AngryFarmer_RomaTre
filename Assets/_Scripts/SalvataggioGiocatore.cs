using System;

/// <summary>
/// Facade piccola e stabile per menu e schermate profilo. Nasconde il formato
/// JSON e permette di sostituire il profilo ospite con un account autenticato
/// senza cambiare l'interfaccia utente.
/// </summary>
public static class SalvataggioGiocatore
{
    public static string NomeProfilo
    {
        get
        {
            string nome = SaveService.Profilo.nomeProfilo;
            return string.IsNullOrWhiteSpace(nome)
                ? "Contadino"
                : nome;
        }
    }

    public static string IdProfiloBreve
    {
        get
        {
            string id = SaveService.Profilo.idProfilo;
            if (string.IsNullOrWhiteSpace(id) ||
                string.Equals(
                    id,
                    SaveService.IdProfiloOspite,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                return "OSPITE";
            }

            id = id.Trim();
            return id.Length <= 8
                ? id.ToUpperInvariant()
                : id.Substring(0, 8).ToUpperInvariant();
        }
    }

    public static int MiglioreOndata =>
        ProgressionePartita.MiglioreOndataAssoluta;

    public static bool ImpostaNomeProfilo(string nome)
    {
        string nomeValido = string.IsNullOrWhiteSpace(nome)
            ? "Contadino"
            : nome.Trim();
        if (nomeValido.Length > 20)
        {
            nomeValido = nomeValido.Substring(0, 20);
        }

        if (string.Equals(
                NomeProfilo,
                nomeValido,
                StringComparison.Ordinal
            ))
        {
            return true;
        }

        return SaveService.ModificaProfilo(dati =>
        {
            dati.nomeProfilo = nomeValido;
        });
    }
}
