using UnityEngine;

public static class MobileHaptics
{
    private static float prossimoImpulso;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Azzera() => prossimoImpulso = 0f;

    public static void Emetti()
    {
        // L'anteprima non deve attivare dispositivi reali o modificare il feedback PC.
        if (!Application.isMobilePlatform || !MobileExperience.GameplayTouchAttivo ||
            GameOptionsController.Instance == null ||
            !GameOptionsController.Instance.ApticaAttiva ||
            Time.unscaledTime < prossimoImpulso) return;
        prossimoImpulso = Time.unscaledTime + 0.35f;
        Handheld.Vibrate();
    }
}
