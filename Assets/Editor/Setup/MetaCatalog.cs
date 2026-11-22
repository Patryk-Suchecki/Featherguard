using Featherguard.Meta;

namespace Featherguard.EditorTools
{
    public static class MetaCatalog
    {
        public static MetaConfig Build(Seeder seeder) => seeder.Seed<MetaConfig>($"{ContentPaths.Data}/MetaConfig.asset", null);
    }
}
