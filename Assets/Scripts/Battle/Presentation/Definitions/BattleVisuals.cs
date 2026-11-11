namespace Featherguard.Battle.Presentation
{
    public sealed class BattleVisuals
    {
        public VisualsLibrary<AbilityDefinition, AbilityVisuals> Abilities { get; }
        public VisualsLibrary<UnitDefinition, UnitVisuals> Units { get; }
        public VisualsLibrary<StatusDefinition, StatusVisuals> Statuses { get; }

        public BattleVisuals(BattlePresentationConfig config)
        {
            Abilities = new(config.abilities);
            Units = new(config.units);
            Statuses = new(config.statuses);
        }
    }
}
