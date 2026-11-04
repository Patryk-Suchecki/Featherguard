using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.UI
{
    [CreateAssetMenu(menuName = "Featherguard/UI Sprites")]
    public class UiSprites : ScriptableObject
    {
        public Sprite[] sprites;

        Dictionary<string, Sprite> lookup;

        public Sprite Get(string name)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<string, Sprite>();
                foreach (var sprite in sprites)
                    if (sprite != null)
                        lookup[sprite.name] = sprite;
            }
            return lookup.TryGetValue(name, out var result) ? result : null;
        }
    }
}
