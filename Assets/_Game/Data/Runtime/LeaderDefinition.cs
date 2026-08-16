using System;
using System.Collections.Generic;
using UnityEngine;

namespace Statecraft.Data
{
    [Serializable]
    public sealed class LeaderSkillDefinition
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string type = string.Empty;
        [SerializeField] private string shortDescription = string.Empty;
        [SerializeField] private bool isLocked;

        public LeaderSkillDefinition(
            string skillId,
            string skillName,
            string skillType,
            string description,
            bool locked = false)
        {
            id = skillId;
            displayName = skillName;
            type = skillType;
            shortDescription = description;
            isLocked = locked;
        }

        public string Id => id;
        public string DisplayName => displayName;
        public string Type => type;
        public string ShortDescription => shortDescription;
        public bool IsLocked => isLocked;
    }

    [Serializable]
    public struct LeaderStats
    {
        [Range(0, 100)] public int charisma;
        [Range(0, 100)] public int diplomacy;
        [Range(0, 100)] public int authority;
        [Range(0, 100)] public int strategy;
        [Range(0, 100)] public int economy;
        [Range(0, 100)] public int eloquence;
    }

    [CreateAssetMenu(fileName = "Leader", menuName = "Statecraft/Data/Leader")]
    public sealed class LeaderDefinition : ScriptableObject
    {
        public const int SkillSlotCount = 4;

        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string title;
        [SerializeField] private Sprite portrait = null;
        [SerializeField] private LeaderStats stats;
        [SerializeField] private LeaderSkillDefinition[] skills = Array.Empty<LeaderSkillDefinition>();

        public string Id => id;
        public string DisplayName => displayName;
        public string Title => title;
        public Sprite Portrait => portrait;
        public LeaderStats Stats => stats;
        public IReadOnlyList<LeaderSkillDefinition> Skills => skills;

#if UNITY_EDITOR
        public void Configure(
            string leaderId,
            string leaderName,
            string leaderTitle,
            LeaderStats leaderStats,
            LeaderSkillDefinition[] leaderSkills)
        {
            id = leaderId;
            displayName = leaderName;
            title = leaderTitle;
            stats = leaderStats;
            skills = leaderSkills ?? Array.Empty<LeaderSkillDefinition>();
        }
#endif
    }
}
