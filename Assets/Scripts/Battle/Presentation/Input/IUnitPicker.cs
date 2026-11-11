using System.Collections.Generic;

namespace Featherguard.Battle.Presentation
{
    public interface IUnitPicker
    {
        bool TryPick(IEnumerable<UnitView> candidates, out UnitView unit);
    }
}
