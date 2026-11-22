using System;
using System.Linq;

namespace Featherguard.Meta
{
    public sealed class PotionBrewer
    {
        readonly RecipeBook book;
        readonly MaterialStorage storage;
        readonly PotionBag bag;
        readonly int maxBrew;

        public PotionBrewer(RecipeBook book, MaterialStorage storage, PotionBag bag, int maxBrew)
        {
            this.book = book;
            this.storage = storage;
            this.bag = bag;
            this.maxBrew = maxBrew;
        }

        public int MaxBrew => maxBrew;

        public int MostBrewable(RecipeDefinition recipe)
        {
            if (!book.Knows(recipe) || recipe.potion == null) return 0;
            var ingredients = recipe.ingredients.Where(i => i.material != null).ToList();
            int most = ingredients.Count == 0 ? maxBrew : ingredients.Min(i => storage.Count(i.material) / Math.Max(1, i.amount));
            return Math.Min(most, maxBrew);
        }

        public bool CanBrew(RecipeDefinition recipe, int count) => count >= 1 && count <= MostBrewable(recipe);

        public bool Brew(RecipeDefinition recipe, int count)
        {
            if (!CanBrew(recipe, count)) return false;
            foreach (var ingredient in recipe.ingredients.Where(i => i.material != null))
                storage.Remove(ingredient.material, ingredient.amount * count);
            bag.Add(recipe.potion, count);
            return true;
        }
    }
}
