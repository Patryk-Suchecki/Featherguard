using System;
using Featherguard.Battle;
using UnityEngine;

namespace Featherguard.Meta
{
    [Serializable]
    public class Ingredient
    {
        public MaterialDefinition material;
        [Min(1)] public int amount = 1;
    }

    [CreateAssetMenu(menuName = "Featherguard/Recipe")]
    public class RecipeDefinition : ScriptableObject
    {
        public PotionDefinition potion;
        public Ingredient[] ingredients = new Ingredient[0];
        [Tooltip("Known without its schematic.")]
        public bool knownFromStart;
    }
}
