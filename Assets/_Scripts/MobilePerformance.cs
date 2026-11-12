using UnityEngine;

/// <summary>Le opzioni mobile non cambiano le preferenze video del PC o il bilanciamento.</summary>
public static class MobilePerformance
{
    public static bool ModalitaLeggera => MobileExperience.Attiva &&
        GameOptionsController.Instance != null && GameOptionsController.Instance.QualitaMobileRidotta;

    public static int NormalizzaFps(int valore) => valore > 0 && valore <= 30 ? 30 : 60;

    public static void Applica(int fps)
    {
        if (!MobileExperience.Attiva) return;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = NormalizzaFps(fps);
        // Pixel art 2D: niente filtri costosi destinati alle scene 3D.
        if (Application.isMobilePlatform)
        {
            QualitySettings.antiAliasing = 0;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.pixelLightCount = 0;
        }
    }
}
