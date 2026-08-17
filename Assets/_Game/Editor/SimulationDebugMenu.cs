using Statecraft.Core;
using UnityEditor;
using UnityEngine;

namespace Statecraft.Editor
{
    public static class SimulationDebugMenu
    {
        private const string MenuPath = "Statecraft/Simulation/Apply Dashboard Test Mutation";

        [MenuItem(MenuPath, priority = 210)]
        private static void ApplyDashboardTestMutation()
        {
            var controller = Object.FindAnyObjectByType<GameUiController>();
            var session = controller != null ? controller.Runtime?.CurrentSession : null;
            if (session == null || !session.IsActive)
            {
                return;
            }

            session.PlayerCountryState.ModifyPublicApproval(-8f);
            session.PlayerCountryState.ModifyTreasury(-1_000_000_000d);
            session.PlayerCountryState.ModifyStability(3f);
            session.AdvanceDays(1);
        }

        [MenuItem(MenuPath, validate = true)]
        private static bool ValidateDashboardTestMutation()
        {
            if (!EditorApplication.isPlaying)
            {
                return false;
            }

            var controller = Object.FindAnyObjectByType<GameUiController>();
            return controller != null && controller.Runtime != null && controller.Runtime.HasActiveSession;
        }
    }
}
