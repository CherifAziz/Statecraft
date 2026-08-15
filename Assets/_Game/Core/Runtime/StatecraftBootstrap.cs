using UnityEngine;

namespace Statecraft.Core
{
    public static class StatecraftBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateApplication()
        {
            if (Object.FindAnyObjectByType<GameUiController>() != null)
            {
                return;
            }

            var applicationObject = new GameObject("[Statecraft Application]");
            Object.DontDestroyOnLoad(applicationObject);
            applicationObject.AddComponent<GameUiController>();
        }
    }
}
