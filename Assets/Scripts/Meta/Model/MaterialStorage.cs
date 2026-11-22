using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Meta
{
    public readonly struct MaterialStack
    {
        public readonly MaterialDefinition Material;
        public readonly int Count;

        public MaterialStack(MaterialDefinition material, int count)
        {
            Material = material;
            Count = count;
        }
    }

    public sealed class MaterialStorage
    {
        readonly IReadOnlyList<MaterialDefinition> materials;
        readonly SaveData data;
        readonly ISaveStore store;

        public MaterialStorage(IReadOnlyList<MaterialDefinition> materials, SaveData data, ISaveStore store)
        {
            this.materials = materials;
            this.data = data;
            this.store = store;
        }

        public IReadOnlyList<MaterialDefinition> All => materials;

        public IReadOnlyList<MaterialStack> Owned =>
            materials.Select(m => new MaterialStack(m, Count(m))).Where(s => s.Count > 0).ToList();

        public int Count(MaterialDefinition material) => Entry(material)?.count ?? 0;

        public void Add(MaterialDefinition material, int count)
        {
            if (material == null || count <= 0) return;
            var entry = Entry(material);
            if (entry == null) data.materials.Add(entry = new MaterialCount { id = material.name });
            entry.count += count;
            store.Save(data);
        }

        public void Add(IEnumerable<MaterialStack> stacks)
        {
            foreach (var stack in stacks) Add(stack.Material, stack.Count);
        }

        public void Clear()
        {
            data.materials.Clear();
            store.Save(data);
        }

        MaterialCount Entry(MaterialDefinition material) => material == null ? null : data.materials.FirstOrDefault(e => e.id == material.name);
    }
}
