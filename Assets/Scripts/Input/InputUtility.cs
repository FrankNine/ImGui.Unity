using System;

using ImGui.Unity.Assets;

namespace ImGui.Unity.Input
{
    public enum InputSourceType
    {
        InputManager,
        InputSystem
    }

    internal static class InputUtility
    {
        internal static IInputSource Create(InputSourceType type, CursorShapesAsset cursors, IniSettingsAsset iniSettings) 
            => type switch
            {
                InputSourceType.InputManager => new InputManagerSource(cursors, iniSettings),
                InputSourceType.InputSystem => new InputSystemSource(cursors, iniSettings),
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}