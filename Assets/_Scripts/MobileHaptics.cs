using UnityEngine;

public static class MobileHaptics
{
#if UNITY_ANDROID || UNITY_IOS
    private static float prossimoImpulso;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Azzera() => prossimoImpulso = 0f;
#endif

    public static void Emetti()
    {
#if UNITY_ANDROID || UNITY_IOS
        // L'anteprima non deve attivare dispositivi reali o modificare il feedback PC.
        if (!Application.isMobilePlatform || !MobileExperience.GameplayTouchAttivo ||
            GameOptionsController.Instance == null ||
            !GameOptionsController.Instance.ApticaAttiva ||
            Time.unscaledTime < prossimoImpulso) return;
        prossimoImpulso = Time.unscaledTime + 0.35f;
        Handheld.Vibrate();
#endif
    }
}
