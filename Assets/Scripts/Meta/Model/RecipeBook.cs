using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Meta
{
    public sealed class RecipeBook
    {
        readonly IReadOnlyList<RecipeDefinition> recipes;
        readonly SaveData data;
        readonly ISaveStore store;

        public RecipeBook(IReadOnlyList<RecipeDefinition> recipes, SaveData data, ISaveStore store)
        {
            this.recipes = recipes;
            this.data = data;
            this.store = store;
        }

        public IReadOnlyList<RecipeDefinition> All => recipes;

        public static bool IsKnown(RecipeDefinition recipe, SaveData data) => recipe != null && (recipe.knownFromStart || data.schematics.Contains(recipe.name));

        public bool Knows(RecipeDefinition recipe) => IsKnown(recipe, data);

        public void Learn(RecipeDefinition recipe)
        {
            if (recipe == null || Knows(recipe)) return;
            data.schematics.Add(recipe.name);
            store.Save(data);
        }

        public void LearnAll()
        {
            foreach (var recipe in recipes.Where(r => !Knows(r)))
                data.schematics.Add(recipe.name);
            store.Save(data);
        }
    }
}
