using System.Collections;
using System.Collections.Generic;
using Featherguard.Run;
using UnityEngine;

namespace Featherguard.UI
{
    public sealed class PathStep : IRewardStep
    {
        static readonly Dictionary<PathNodeType, IRewardStep> Handlers = new()
        {
            [PathNodeType.Elite] = new EliteStep(),
            [PathNodeType.Shop] = new ShopStep(),
            [PathNodeType.Chest] = new ChestStep(),
            [PathNodeType.Campfire] = new CampfireStep(),
        };

        public IEnumerator Run(RewardContext context)
        {
            var session = context.Session;
            var nodes = PathRewards.Choices(session);
            if (nodes.Count == 0) yield break;

            var screen = context.Screen;
            screen.Open(session.NextIsFinal ? "BEFORE THE BOSS" : "CHOOSE YOUR PATH", session.NextIsFinal ? $"{session.Definition.encounters[^1].displayName} awaits. Get ready." : "Where to next?");
            float spacing = 580f;
            float start = -(nodes.Count - 1) * spacing * 0.5f;
            for (int i = 0; i < nodes.Count; i++)
                screen.Run(Tweens.Flip(NodeCard(screen, nodes[i], new Vector2(start + i * spacing, -40f), i)));
            yield return screen.WaitChoice();

            if (Handlers.TryGetValue(nodes[screen.Choice].type, out var handler))
                yield return handler.Run(context);
        }

        static RectTransform NodeCard(RewardScreen screen, PathNode node, Vector2 position, int value)
        {
            var rect = Ui.Centered(node.title, screen.Root, position, new Vector2(540f, 520f));
            Ui.Button(rect, node.color, () => screen.Choose(value));
            var icon = Ui.Icon(rect, "Flag", new Vector2(0f, 120f), 200f);
            if (node.icon != null) icon.sprite = node.icon;
            Ui.Label(Ui.Centered("Title", rect, new Vector2(0f, -30f), new Vector2(500f, 70f)), node.title, 54, TextAnchor.MiddleCenter, Color.white);
            Ui.Wrapped(Ui.Centered("Description", rect, new Vector2(0f, -160f), new Vector2(460f, 150f)), node.description, 32, TextAnchor.MiddleCenter, new Color(0.92f, 0.94f, 1f));
            return rect;
        }
    }

    public sealed class EliteStep : IRewardStep
    {
        public IEnumerator Run(RewardContext context)
        {
            PathRewards.ChooseElite(context.Session);
            yield break;
        }
    }
}
