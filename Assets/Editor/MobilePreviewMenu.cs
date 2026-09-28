using UnityEditor;

public static class MobilePreviewMenu
{
    private const string Voce = "Tools/Angry Farmer/Anteprima controlli mobile";

    [MenuItem(Voce)]
    private static void Cambia()
    {
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, !MobileExperience.Attiva);
    }

    [MenuItem(Voce, true)]
    private static bool Valida()
    {
        Menu.SetChecked(Voce, MobileExperience.Attiva);
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
