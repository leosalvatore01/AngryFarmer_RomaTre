using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopRegressionTests
{
    private readonly List<GameObject> oggettiCreati =
        new List<GameObject>();

    private GameManager gameManager;
    private GameManager gameManagerPrecedente;
    private PlayerUpgrades potenziamenti;
    private PlayerShooting sparo;

    [SetUp]
    public void SetUp()
    {
        gameManagerPrecedente = GameManager.instance;
        GameManager.instance = null;

        GameObject oggettoManager = CreaOggetto("GameManager_Test");
        oggettoManager.SetActive(false);
        gameManager = oggettoManager.AddComponent<GameManager>();
        GameManager.instance = gameManager;

        GameObject giocatore = CreaOggetto("Contadino_Test");
        Rigidbody2D corpo = giocatore.AddComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Kinematic;

        PlayerMovement movimento =
            giocatore.AddComponent<PlayerMovement>();
        PlayerHealth salute = giocatore.AddComponent<PlayerHealth>();
        sparo = giocatore.AddComponent<PlayerShooting>();
        potenziamenti = giocatore.GetComponent<PlayerUpgrades>();
        if (potenziamenti == null)
        {
            potenziamenti = giocatore.AddComponent<PlayerUpgrades>();
        }

        ImpostaCampoPrivato(potenziamenti, "movimento", movimento);
        ImpostaCampoPrivato(potenziamenti, "salute", salute);
        ImpostaCampoPrivato(potenziamenti, "sparo", sparo);
    }

    [TearDown]
    public void TearDown()
    {
        GameManager.instance = gameManagerPrecedente;
        gameManagerPrecedente = null;

        for (int i = oggettiCreati.Count - 1; i >= 0; i--)
        {
            if (oggettiCreati[i] != null)
            {
                Object.DestroyImmediate(oggettiCreati[i]);
            }
        }

        oggettiCreati.Clear();
    }

    [Test]
    public void Acquisto_SaldoInsufficiente_NonModificaSaldoOLivello()
    {
        int costo = potenziamenti.OttieniCosto(TipoPotenziamento.Danno);
        gameManager.monete = Mathf.Max(0, costo - 1);
        int saldoIniziale = gameManager.monete;
        int livelloIniziale =
            potenziamenti.OttieniLivello(TipoPotenziamento.Danno);

        bool acquistato = potenziamenti.ProvaAcquistare(
            TipoPotenziamento.Danno,
            out string messaggio
        );

        Assert.That(acquistato, Is.False);
        Assert.That(gameManager.monete, Is.EqualTo(saldoIniziale));
        Assert.That(
            potenziamenti.OttieniLivello(TipoPotenziamento.Danno),
            Is.EqualTo(livelloIniziale)
        );
        StringAssert.Contains("mancano", messaggio.ToLowerInvariant());
    }

    [Test]
    public void Acquisto_SaldoEsatto_ScalaMoneteEApplicaPotenziamento()
    {
        int costo = potenziamenti.OttieniCosto(TipoPotenziamento.Danno);
        gameManager.monete = costo;
        int dannoIniziale = sparo.DannoFinale;

        bool acquistato = potenziamenti.ProvaAcquistare(
            TipoPotenziamento.Danno,
            out string messaggio
        );

        Assert.That(acquistato, Is.True, messaggio);
        Assert.That(gameManager.monete, Is.Zero);
        Assert.That(
            potenziamenti.OttieniLivello(TipoPotenziamento.Danno),
            Is.EqualTo(1)
        );
        Assert.That(sparo.DannoFinale, Is.GreaterThan(dannoIniziale));
    }

    [Test]
    public void CarteOfferta_SenzaMonete_SonoDisabilitate()
    {
        gameManager.monete = 0;
        ShopInterOndata shop = CreaShop();
        shop.ImpostaSeedOffertePerTest(4217);
        shop.RigeneraOffertePerTest(1, gameManager.monete);

        List<Button> pulsanti = OttieniPulsantiOfferta(shop);

        Assert.That(pulsanti, Is.Not.Empty);
        VerificaConteggioOfferte(shop, pulsanti);
        foreach (Button pulsante in pulsanti)
        {
            Assert.That(pulsante.interactable, Is.False);
            TMP_Text etichetta = pulsante.GetComponentInChildren<TMP_Text>(true);
            Assert.That(etichetta, Is.Not.Null);
            StringAssert.StartsWith("MANCANO", etichetta.text);
        }
    }

    [Test]
    public void CarteOfferta_ConMoneteSufficienti_SonoInteragibili()
    {
        gameManager.monete = OttieniSaldoSufficiente();
        ShopInterOndata shop = CreaShop();
        shop.ImpostaSeedOffertePerTest(4217);
        shop.RigeneraOffertePerTest(1, gameManager.monete);

        List<Button> pulsanti = OttieniPulsantiOfferta(shop);

        Assert.That(pulsanti, Is.Not.Empty);
        VerificaConteggioOfferte(shop, pulsanti);
        foreach (Button pulsante in pulsanti)
        {
            Assert.That(pulsante.interactable, Is.True);
        }
    }

    private ShopInterOndata CreaShop()
    {
        GameObject radice = CreaOggetto(
            "ShopInterOndata_Test",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        ShopInterOndata shop = radice.AddComponent<ShopInterOndata>();

        if (radice.transform.Find("BottegaBuild") == null)
        {
            InvocaMetodoPrivato(shop, "Awake");
        }
        ImpostaCampoPrivato(shop, "potenziamenti", potenziamenti);

        return shop;
    }

    private static List<Button> OttieniPulsantiOfferta(
        ShopInterOndata shop
    )
    {
        List<Button> risultato = new List<Button>();
        Transform bottega = shop.transform.Find("BottegaBuild");

        Assert.That(bottega, Is.Not.Null);
        for (int indice = 1; indice <= 4; indice++)
        {
            Transform carta = bottega.Find("Offerta_" + indice);
            if (carta == null || !carta.gameObject.activeSelf)
            {
                continue;
            }

            Transform acquista = carta.Find("Acquista");
            Assert.That(acquista, Is.Not.Null);
            risultato.Add(acquista.GetComponent<Button>());
        }

        return risultato;
    }

    private void VerificaConteggioOfferte(
        ShopInterOndata shop,
        List<Button> pulsanti
    )
    {
        int numeroConfigurato = Mathf.Clamp(
            GameBalanceConfig.Corrente.Shop.numeroOfferte,
            3,
            4
        );
        Assert.That(
            shop.OfferteCorrenti,
            Has.Count.EqualTo(numeroConfigurato)
        );
        Assert.That(
            pulsanti,
            Has.Count.EqualTo(shop.OfferteCorrenti.Count)
        );
    }

    private int OttieniSaldoSufficiente()
    {
        int saldo = 0;
        foreach (TipoPotenziamento tipo in
                 System.Enum.GetValues(typeof(TipoPotenziamento)))
        {
            saldo = Mathf.Max(saldo, potenziamenti.OttieniCosto(tipo));
        }

        return saldo;
    }

    private GameObject CreaOggetto(
        string nome,
        params System.Type[] componenti
    )
    {
        GameObject oggetto = componenti == null || componenti.Length == 0
            ? new GameObject(nome)
            : new GameObject(nome, componenti);
        oggettiCreati.Add(oggetto);
        return oggetto;
    }

    private static void ImpostaCampoPrivato(
        object destinazione,
        string nomeCampo,
        object valore
    )
    {
        FieldInfo campo = destinazione.GetType().GetField(
            nomeCampo,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(campo, Is.Not.Null, "Campo non trovato: " + nomeCampo);
        campo.SetValue(destinazione, valore);
    }

    private static void InvocaMetodoPrivato(
        object destinazione,
        string nomeMetodo
    )
    {
        MethodInfo metodo = destinazione.GetType().GetMethod(
            nomeMetodo,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(metodo, Is.Not.Null, "Metodo non trovato: " + nomeMetodo);
        metodo.Invoke(destinazione, null);
    }
}
