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
            meta ??= new MetaProgression(config.meta, new JsonFileSaveStore(Path.Combine(Application.persistentDataPath, config.meta.saveFileName)));

        public static HeroRoster Roster(GameConfig config) => Meta(config).CreateRoster(config.heroes, config.campSeats);

        public static WorldProgress World(GameConfig config) => Meta(config).CreateWorld(config.world, UnityRandomSource.Shared);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLoad() => meta = null;
    }
}
