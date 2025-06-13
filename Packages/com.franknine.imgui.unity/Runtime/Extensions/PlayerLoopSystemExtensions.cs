using System;

using UnityEngine.LowLevel;

namespace ImGui.Unity.Extensions
{
    public static class PlayerLoopSystemExtensions
    {
        public static void AppendToPlayerLoop
        (
            this PlayerLoopSystem playerLoopSystem, 
            Type type, 
            PlayerLoopSystem.UpdateFunction updateFunction
        )
        {
            var newLength = playerLoopSystem.subSystemList.Length + 1;
            var newPlayerLoopSystem = new PlayerLoopSystem[newLength];
            for (var i = 0; i < playerLoopSystem.subSystemList.Length; i++)
            {
                newPlayerLoopSystem[i] = playerLoopSystem.subSystemList[i];
            }
            newPlayerLoopSystem[newLength - 1] = new PlayerLoopSystem
            {
                type = type,
                updateDelegate = updateFunction
            };
            playerLoopSystem.subSystemList = newPlayerLoopSystem;
            PlayerLoop.SetPlayerLoop(playerLoopSystem); 
        }
        
        public static bool HasPlayerLoopSystem
        (
            this PlayerLoopSystem playerLoopSystem, 
            Type type,
            PlayerLoopSystem.UpdateFunction updateFunction
        )
        {
            for (var i = 0; i < playerLoopSystem.subSystemList.Length; i++)
            {
                if (playerLoopSystem.subSystemList[i].type == type &&
                    playerLoopSystem.subSystemList[i].updateDelegate == updateFunction)
                    return true;
            }

            return false;
        }

        public static void RemovePlayerLoopSystem
        (
            this PlayerLoopSystem playerLoopSystem,
            Type type,
            PlayerLoopSystem.UpdateFunction updateFunction
        )
        {
            var newLength = 0;
            for (var i = 0; i < playerLoopSystem.subSystemList.Length; i++)
            {
                if (playerLoopSystem.subSystemList[i].type == type &&
                    playerLoopSystem.subSystemList[i].updateDelegate == updateFunction)
                    continue;
                newLength++;
            }
            
            var newPlayerLoopSystem = new PlayerLoopSystem[newLength];
            var newIndex = 0;
            for (var i = 0; i < playerLoopSystem.subSystemList.Length; i++)
            {
                if (playerLoopSystem.subSystemList[i].type == type &&
                    playerLoopSystem.subSystemList[i].updateDelegate == updateFunction)
                    continue;
                newPlayerLoopSystem[newIndex] = playerLoopSystem.subSystemList[i];
                newIndex++;
            }
            playerLoopSystem.subSystemList = newPlayerLoopSystem;
            PlayerLoop.SetPlayerLoop(playerLoopSystem);  
        }
    }
}