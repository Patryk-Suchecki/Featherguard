using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;

namespace Featherguard.Meta
{
    public enum PartyChange { Added, Removed, PartyFull, NotJoined }

    public sealed class HeroRoster
    {
        public const int PartySize = 3;

        readonly SaveData data;
        readonly ISaveStore store;
        readonly List<UnitDefinition> party = new();

        public IReadOnlyList<UnitDefinition> Heroes { get; }
        public int Seats { get; }
        public IReadOnlyList<UnitDefinition> Party => party;
        public bool IsPartyComplete => party.Count == PartySize;

        public HeroRoster(IReadOnlyList<UnitDefinition> heroes, int seats, SaveData data, ISaveStore store)
        {
            Heroes = heroes.Where(h => h != null).Take(seats).ToList();
            Seats = seats;
            this.data = data;
            this.store = store;

            foreach (var id in data.party)
            {
                var hero = Heroes.FirstOrDefault(h => h.name == id);
                if (hero != null && !party.Contains(hero) && party.Count < PartySize)
                    party.Add(hero);
            }
            if (party.Count == 0)
                party.AddRange(Heroes.Take(PartySize));
        }

        public UnitDefinition AtSeat(int seat) => seat >= 0 && seat < Heroes.Count ? Heroes[seat] : null;

        public bool InParty(UnitDefinition hero) => party.Contains(hero);

        public PartyChange Toggle(UnitDefinition hero)
        {
            if (!Heroes.Contains(hero)) return PartyChange.NotJoined;
            if (party.Remove(hero))
            {
                Save();
                return PartyChange.Removed;
            }
            if (party.Count >= PartySize) return PartyChange.PartyFull;
            party.Add(hero);
            Save();
            return PartyChange.Added;
        }

        void Save()
        {
            data.party = party.Select(h => h.name).ToList();
            store.Save(data);
        }
    }
}
