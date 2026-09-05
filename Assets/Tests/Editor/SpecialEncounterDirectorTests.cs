using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class SpecialEncounterDirectorTests
{
    [Test]
    public void IncontroElite_AssegnaUnaSolaAlfaSpeciale()
    {
        Assert.That(
            SpecialEncounterDirector.ScegliRuolo(
                TipoIncontroOndata.Elite,
                TipoVolpe.Comune,
                false
            ),
            Is.EqualTo(RuoloIncontroSpeciale.Nessuno)
        );
        Assert.That(
            SpecialEncounterDirector.ScegliRuolo(
                TipoIncontroOndata.Elite,
                TipoVolpe.Alfa,
                false
            ),
            Is.EqualTo(RuoloIncontroSpeciale.Elite)
        );
        Assert.That(
            SpecialEncounterDirector.ScegliRuolo(
                TipoIncontroOndata.Elite,
                TipoVolpe.Alfa,
                true
            ),
            Is.EqualTo(RuoloIncontroSpeciale.Nessuno)
        );
    }

    [Test]
    public void IncontroBoss_TrasformaLaPrimaAlfaNelBoss()
    {
        Assert.That(
            SpecialEncounterDirector.ScegliRuolo(
                TipoIncontroOndata.Boss,
                TipoVolpe.Alfa,
                false
            ),
            Is.EqualTo(RuoloIncontroSpeciale.Boss)
        );
    }

    [Test]
    public void Boss_RicresceAdOgniSuccessivaApparizione()
    {
        SpecialEncounterBalanceSettings impostazioni =
            new SpecialEncounterBalanceSettings();

        int primaVita = SpecialEncounterDirector.CalcolaVitaBoss(
            20,
            1,
            impostazioni
        );
        int secondaVita = SpecialEncounterDirector.CalcolaVitaBoss(
            20,
            2,
            impostazioni
        );
        int primaRicompensa = SpecialEncounterDirector.CalcolaMoneteBoss(
            1,
            impostazioni
        );
        int secondaRicompensa = SpecialEncounterDirector.CalcolaMoneteBoss(
            2,
            impostazioni
        );

        Assert.That(secondaVita, Is.GreaterThan(primaVita));
        Assert.That(secondaRicompensa, Is.GreaterThan(primaRicompensa));
        Assert.That(
            SpecialEncounterDirector.CalcolaApparizioneBoss(30, 10),
            Is.EqualTo(3)
        );
    }

    [Test]
    public void BilanciamentoBoss_PrevedeTreAttacchiEDueFasi()
    {
        SpecialEncounterBalanceSettings impostazioni =
            GameBalanceConfig.Corrente.Ondate.IncontriSpeciali;

        Assert.That(impostazioni.durataPreavvisoCarica, Is.GreaterThan(0f));
        Assert.That(impostazioni.durataPreavvisoSchianto, Is.GreaterThan(0f));
        Assert.That(impostazioni.durataPreavvisoRaffica, Is.GreaterThan(0f));
        Assert.That(impostazioni.sogliaSecondaFase, Is.InRange(0.2f, 0.8f));
        Assert.That(
            impostazioni.proiettiliSecondaFase,
            Is.GreaterThan(impostazioni.proiettiliPrimaFase)
        );
    }

    [Test]
    public void Boss_HaUnaGraficaDedicataImportataComeSprite()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Resources/Foxes/Boss/IlReDelBranco.png"
        );

        Assert.That(sprite, Is.Not.Null);
        Assert.That(sprite.pixelsPerUnit, Is.EqualTo(512f));
    }
}
