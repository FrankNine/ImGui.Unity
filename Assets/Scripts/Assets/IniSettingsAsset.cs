using UnityEngine;

namespace ImGui.Unity.Assets
{
    // TODO: Ability to save to asset, in player prefs with custom key, custom ini file, etc
    /// <summary>
    /// Used to store ImGui Ini settings in an asset instead of the default imgui.ini file
    /// </summary>
    [CreateAssetMenu(menuName = "Dear ImGui/Ini Settings")]
    internal sealed class IniSettingsAsset : ScriptableObject
    {
        [TextArea(3, 20)] [SerializeField] private string _data;

        public void Save(string data) 
            => _data = data;
        
        public string Load() 
            => _data;
    }
}