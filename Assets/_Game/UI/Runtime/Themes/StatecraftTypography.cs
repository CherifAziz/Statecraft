using UnityEngine;

namespace Statecraft.UI.Themes
{
    [CreateAssetMenu(fileName = "StatecraftTypography", menuName = "Statecraft/UI/Typography")]
    public sealed class StatecraftTypography : ScriptableObject
    {
        [SerializeField] private Font prestigeMedium = null;
        [SerializeField] private Font prestigeSemibold = null;
        [SerializeField] private Font utilityRegular = null;
        [SerializeField] private Font utilityMedium = null;
        [SerializeField] private Font utilitySemibold = null;

        public Font PrestigeMedium => prestigeMedium;
        public Font PrestigeSemibold => prestigeSemibold;
        public Font UtilityRegular => utilityRegular;
        public Font UtilityMedium => utilityMedium;
        public Font UtilitySemibold => utilitySemibold;
    }
}
