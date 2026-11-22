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
