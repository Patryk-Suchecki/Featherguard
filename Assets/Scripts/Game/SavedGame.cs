using System.IO;
using Featherguard.Core;
using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.Game
{
    public static class SavedGame
    {
        static MetaProgression meta;

        public static MetaProgression Meta(GameConfig config) =>
            meta ??= new MetaProgression(config.meta, Store(config));

        public static HeroRoster Roster(GameConfig config) => Meta(config).CreateRoster(config.heroes, config.campSeats);

        public static WorldProgress World(GameConfig config) => Meta(config).CreateWorld(config.world, UnityRandomSource.Shared);

        public static MaterialStorage Storage(GameConfig config) => Meta(config).CreateStorage(config.materials);

        public static PotionBag Potions(GameConfig config) => Meta(config).CreatePotionBag(config.potions);

        public static RecipeBook Recipes(GameConfig config) => Meta(config).CreateRecipeBook(config.potions);

        public static PotionBrewer Brewer(GameConfig config) =>
            new(Recipes(config), Storage(config), Potions(config), config.potions != null ? config.potions.maxBrew : 1);

        public static void ResetProgress(GameConfig config)
        {
            Store(config).Save(new SaveData());
            meta = null;
        }

        static ISaveStore Store(GameConfig config) => new JsonFileSaveStore(Path.Combine(Application.persistentDataPath, config.meta.saveFileName));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLoad() => meta = null;
    }
}
