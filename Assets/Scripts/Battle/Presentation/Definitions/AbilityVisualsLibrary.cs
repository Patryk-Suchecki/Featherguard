using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class AbilityVisualsLibrary
    {
        readonly Dictionary<AbilityDefinition, AbilityVisuals> byAbility = new();
        readonly AbilityVisuals fallback;

        public AbilityVisualsLibrary(IEnumerable<AbilityVisuals> visuals)
        {
            foreach (var entry in visuals)
                if (entry != null && entry.ability != null)
                    byAbility[entry.ability] = entry;
            fallback = ScriptableObject.CreateInstance<AbilityVisuals>();
        }

        public AbilityVisuals Of(AbilityDefinition ability) =>
            ability != null && byAbility.TryGetValue(ability, out var visuals) ? visuals : fallback;
    }
}
