using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public interface IVisualsFor<out TDefinition>
    {
        TDefinition Definition { get; }
    }

    public sealed class VisualsLibrary<TDefinition, TVisuals>
        where TDefinition : Object
        where TVisuals : ScriptableObject, IVisualsFor<TDefinition>
    {
        readonly Dictionary<TDefinition, TVisuals> byDefinition = new();
        readonly TVisuals fallback;

        public VisualsLibrary(IEnumerable<TVisuals> visuals)
        {
            if (visuals != null)
                foreach (var entry in visuals)
                    if (entry != null && entry.Definition != null)
                        byDefinition[entry.Definition] = entry;
            fallback = ScriptableObject.CreateInstance<TVisuals>();
        }

        public TVisuals Of(TDefinition definition) =>
            definition != null && byDefinition.TryGetValue(definition, out var visuals) ? visuals : fallback;
    }
}
