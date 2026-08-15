using System;
using UnityEngine;

namespace Statecraft.Data
{
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
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string title;
        [SerializeField] private Sprite portrait = null;
        [SerializeField] private LeaderStats stats;

        public string Id => id;
        public string DisplayName => displayName;
        public string Title => title;
        public Sprite Portrait => portrait;
        public LeaderStats Stats => stats;

#if UNITY_EDITOR
        public void Configure(string leaderId, string leaderName, string leaderTitle, LeaderStats leaderStats)
        {
            id = leaderId;
            displayName = leaderName;
            title = leaderTitle;
            stats = leaderStats;
        }
#endif
    }
}
