using System;

namespace ImGui.Unity.Input
{
    public enum InputSourceType
    {
        InputManager,
        InputSystem
    }

    internal static class InputUtility
    {
        internal static IInputSource Create(InputSourceType type) 
            => type switch
            {
                InputSourceType.InputManager => new InputManagerSource(),
                InputSourceType.InputSystem => new InputSystemSource(),
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}