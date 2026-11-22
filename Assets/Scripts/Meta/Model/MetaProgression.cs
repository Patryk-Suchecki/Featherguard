using System.Collections.Generic;
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
    }
}
