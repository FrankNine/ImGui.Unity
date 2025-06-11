using ImGui.Unity.Assets;

namespace ImGui.Unity.Input
{
    public class InputUtility
    {
        internal static IInputSource Create(CursorShapesAsset cursors, IniSettingsAsset iniSettings)
            => new InputManagerPlatform(cursors, iniSettings);
    }
}