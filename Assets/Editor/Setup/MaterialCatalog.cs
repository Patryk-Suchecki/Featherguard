using System.Linq;
using Featherguard.Meta;

namespace Featherguard.EditorTools
{
    public static class MaterialCatalog
    {
        static readonly (string Id, string Name, MaterialRarity Rarity, string Land)[] Rows =
        {
            ("Flax", "Flax", MaterialRarity.Common, "RyeFields"),
            ("WolfPelt", "Wolf Pelt", MaterialRarity.Uncommon, "RyeFields"),
            ("LastSheaf", "Last Sheaf", MaterialRarity.Rare, "RyeFields"),
            ("MarshHerbs", "Marsh Herbs", MaterialRarity.Common, "Marshes"),
            ("Amber", "Amber", MaterialRarity.Uncommon, "Marshes"),
            ("WispInAJar", "Wisp in a Jar", MaterialRarity.Rare, "Marshes"),
            ("Brimstone", "Brimstone", MaterialRarity.Common, "BaldMountain"),
            ("MountainOre", "Mountain Ore", MaterialRarity.Uncommon, "BaldMountain"),
            ("Thunderstone", "Thunderstone", MaterialRarity.Rare, "BaldMountain"),
            ("YewWood", "Yew Wood", MaterialRarity.Common, "OldForest"),
            ("TinderFungus", "Tinder Fungus", MaterialRarity.Uncommon, "OldForest"),
            ("OldOakHeartwood", "Old Oak Heartwood", MaterialRarity.Rare, "OldForest"),
            ("BoneAsh", "Bone Ash", MaterialRarity.Common, "Nav"),
            ("GromnicaWax", "Gromnica Wax", MaterialRarity.Uncommon, "Nav"),
            ("NavEssence", "Nav Essence", MaterialRarity.Rare, "Nav"),
            ("FirebirdFeather", "Firebird Feather", MaterialRarity.Epic, null),
            ("DeadWater", "Dead Water", MaterialRarity.Epic, null),
            ("SerpentKingsCrown", "Serpent King's Crown", MaterialRarity.Epic, null),
            ("KupalaDew", "Kupala Dew", MaterialRarity.Epic, null),
            ("FernFlower", "Fern Flower", MaterialRarity.Legendary, null),
            ("LivingWater", "Living Water", MaterialRarity.Legendary, null),
            ("AlatyrStone", "Alatyr Stone", MaterialRarity.Legendary, null),
        };

        public static MaterialConfig Build(Seeder seeder, WorldDefinition world)
        {
            Seeder.EnsureFolder(ContentPaths.MaterialData);
            var materials = Rows.Select(row => seeder.Seed<MaterialDefinition>($"{ContentPaths.MaterialData}/{row.Id}.asset", m =>
            {
                m.displayName = row.Name;
                m.rarity = row.Rarity;
            }, m => m.land = row.Land == null ? null : world.lands.FirstOrDefault(l => l != null && l.name == row.Land))).ToArray();
            return seeder.Seed<MaterialConfig>($"{ContentPaths.Data}/MaterialConfig.asset", null, c => c.materials = materials);
        }
    }
}
