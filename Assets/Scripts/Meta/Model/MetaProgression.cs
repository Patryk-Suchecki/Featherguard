using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Core;

namespace Featherguard.Meta
{
    public sealed class MetaProgression
    {
        readonly ISaveStore store;

        public MetaConfig Config { get; }
        public SaveData Data { get; }

        public MetaProgression(MetaConfig config, ISaveStore store)
        {
            Config = config;
            this.store = store;
            Data = store.Load();
        }

        public WorldProgress CreateWorld(WorldDefinition world, IRandomSource random) => new(world, Data, store, random);

        public HeroRoster CreateRoster(IReadOnlyList<UnitDefinition> heroes, int seats) => new(heroes, seats, Data, store);

        public MaterialStorage CreateStorage(MaterialConfig materials) => new(materials != null ? materials.materials : new MaterialDefinition[0], Data, store);

        public PotionBag CreatePotionBag(PotionConfig potions) => new(Recipes(potions).Select(r => r.potion).Where(p => p != null).ToList(), Data, store);

        public RecipeBook CreateRecipeBook(PotionConfig potions) => new(Recipes(potions), Data, store);

        public PotionLoadout CreateLoadout(PotionConfig potions) => new(CreatePotionBag(potions), potions != null ? potions.battleSlots : 0, Data, store);

        static RecipeDefinition[] Recipes(PotionConfig potions) => potions != null ? potions.recipes.Where(r => r != null).ToArray() : new RecipeDefinition[0];
    }
}
