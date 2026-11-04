using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class IconFactory
    {
        const int Size = 128;
        const int Samples = 3;

        static readonly Color Ink = new(0.08f, 0.06f, 0.1f, 1f);
        static readonly Color Gold = new(1f, 0.78f, 0.18f);
        static readonly Color GoldDark = new(0.8f, 0.5f, 0.08f);
        static readonly Color Red = new(0.95f, 0.25f, 0.3f);
        static readonly Color Green = new(0.45f, 0.9f, 0.3f);
        static readonly Color Blue = new(0.3f, 0.6f, 1f);
        static readonly Color Orange = new(1f, 0.5f, 0.12f);
        static readonly Color Purple = new(0.65f, 0.4f, 0.95f);
        static readonly Color Steel = new(0.82f, 0.86f, 0.92f);
        static readonly Color Bone = new(0.95f, 0.93f, 0.86f);
        static readonly Color Wood = new(0.55f, 0.33f, 0.15f);

        class Painter
        {
            public readonly Color[] Pixels = new Color[Size * Size];

            public void Fill(Func<Vector2, float> sdf, Color color, float outline = 0.035f)
            {
                if (outline > 0f) Layer(p => sdf(p) - outline, Ink);
                Layer(sdf, color);
            }

            public void Shine(Func<Vector2, float> sdf, Color color, Vector2 offset, float alpha = 0.45f) =>
                Layer(p => Mathf.Max(sdf(p), -sdf(p + offset)), new Color(1f, 1f, 1f, alpha) * color);

            public void Layer(Func<Vector2, float> sdf, Color color)
            {
                for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    float coverage = 0f;
                    for (int sy = 0; sy < Samples; sy++)
                    for (int sx = 0; sx < Samples; sx++)
                    {
                        var p = new Vector2((x + (sx + 0.5f) / Samples) / Size, (y + (sy + 0.5f) / Samples) / Size);
                        if (sdf(p) <= 0f) coverage += 1f;
                    }
                    coverage /= Samples * Samples;
                    if (coverage <= 0f) continue;
                    float a = color.a * coverage;
                    var dst = Pixels[y * Size + x];
                    float outA = a + dst.a * (1f - a);
                    var rgb = (new Vector3(color.r, color.g, color.b) * a + new Vector3(dst.r, dst.g, dst.b) * dst.a * (1f - a)) / Mathf.Max(outA, 1e-5f);
                    Pixels[y * Size + x] = new Color(rgb.x, rgb.y, rgb.z, outA);
                }
            }
        }

        static float Circle(Vector2 p, Vector2 c, float r) => (p - c).magnitude - r;

        static float Box(Vector2 p, Vector2 c, Vector2 half, float round = 0f)
        {
            var d = new Vector2(Mathf.Abs(p.x - c.x), Mathf.Abs(p.y - c.y)) - half + Vector2.one * round;
            return new Vector2(Mathf.Max(d.x, 0f), Mathf.Max(d.y, 0f)).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0f) - round;
        }

        static float Segment(Vector2 p, Vector2 a, Vector2 b, float width)
        {
            var pa = p - a;
            var ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude - width;
        }

        static float Polygon(Vector2 p, params Vector2[] v)
        {
            float d = Vector2.Dot(p - v[0], p - v[0]);
            float s = 1f;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
            {
                var e = v[j] - v[i];
                var w = p - v[i];
                var b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                d = Mathf.Min(d, Vector2.Dot(b, b));
                bool c1 = p.y >= v[i].y, c2 = p.y < v[j].y, c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) s = -s;
            }
            return s * Mathf.Sqrt(d);
        }

        static Vector2 V(float x, float y) => new(x, y);
        static float Union(params float[] d) => Mathf.Min(d);

        static Vector2[] Star(Vector2 c, float outer, float inner, int points, float rotation = 90f)
        {
            var v = new Vector2[points * 2];
            for (int i = 0; i < v.Length; i++)
            {
                float a = (rotation + i * 180f / points) * Mathf.Deg2Rad;
                v[i] = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (i % 2 == 0 ? outer : inner);
            }
            return v;
        }

        static float Heart(Vector2 p, Vector2 c, float s) => Union(
            Circle(p, c + V(-0.18f, 0.08f) * s, 0.2f * s),
            Circle(p, c + V(0.18f, 0.08f) * s, 0.2f * s),
            Polygon(p, c + V(-0.36f, 0.02f) * s, c + V(0f, -0.38f) * s, c + V(0.36f, 0.02f) * s, c + V(0f, 0.12f) * s));

        static float Drop(Vector2 p, Vector2 c, float s) => Union(
            Circle(p, c + V(0f, -0.1f) * s, 0.26f * s),
            Polygon(p, c + V(-0.235f, -0.02f) * s, c + V(0f, 0.42f) * s, c + V(0.235f, -0.02f) * s));

        static float Flame(Vector2 p, Vector2 c, float s) => Union(
            Circle(p, c + V(0f, -0.14f) * s, 0.26f * s),
            Polygon(p, c + V(-0.25f, -0.08f) * s, c + V(-0.12f, 0.2f) * s, c + V(0.02f, 0.44f) * s, c + V(0.12f, 0.16f) * s, c + V(0.25f, -0.08f) * s));

        static float ShieldShape(Vector2 p, Vector2 c, float s) => Polygon(p,
            c + V(-0.32f, 0.34f) * s, c + V(0f, 0.42f) * s, c + V(0.32f, 0.34f) * s, c + V(0.3f, 0f) * s, c + V(0f, -0.42f) * s, c + V(-0.3f, 0f) * s);

        static float Sword(Vector2 p) => Union(
            Polygon(p, V(0.36f, 0.42f), V(0.78f, 0.84f), V(0.84f, 0.78f), V(0.42f, 0.36f)),
            Segment(p, V(0.28f, 0.48f), V(0.48f, 0.28f), 0.045f),
            Segment(p, V(0.2f, 0.2f), V(0.36f, 0.36f), 0.045f));

        static float Skull(Vector2 p, Vector2 c, float s) => Mathf.Max(
            Union(Circle(p, c + V(0f, 0.08f) * s, 0.32f * s), Box(p, c + V(0f, -0.22f) * s, V(0.17f, 0.12f) * s, 0.05f * s)),
            -Union(Circle(p, c + V(-0.13f, 0.04f) * s, 0.09f * s), Circle(p, c + V(0.13f, 0.04f) * s, 0.09f * s),
                Polygon(p, c + V(-0.05f, -0.12f) * s, c + V(0.05f, -0.12f) * s, c + V(0f, -0.04f) * s)));

        static readonly Vector2 Mid = V(0.5f, 0.5f);

        static Dictionary<string, Action<Painter>> Icons() => new()
        {
            ["Shield"] = g =>
            {
                g.Fill(p => ShieldShape(p, Mid, 1f), Blue);
                g.Fill(p => ShieldShape(p, Mid, 0.6f), new Color(0.55f, 0.8f, 1f), 0f);
            },
            ["Empower"] = g =>
            {
                g.Fill(p => Polygon(p, V(0.5f, 0.92f), V(0.86f, 0.52f), V(0.64f, 0.52f), V(0.64f, 0.1f), V(0.36f, 0.1f), V(0.36f, 0.52f), V(0.14f, 0.52f)), Orange);
                g.Shine(p => Polygon(p, V(0.5f, 0.92f), V(0.86f, 0.52f), V(0.64f, 0.52f), V(0.64f, 0.1f), V(0.36f, 0.1f), V(0.36f, 0.52f), V(0.14f, 0.52f)), Color.white, V(-0.06f, 0.04f));
            },
            ["Poison"] = g =>
            {
                g.Fill(p => Drop(p, V(0.5f, 0.46f), 1.1f), Green);
                g.Fill(p => Circle(p, V(0.42f, 0.4f), 0.07f), new Color(0.8f, 1f, 0.7f), 0f);
            },
            ["Stun"] = g =>
            {
                g.Fill(p => Polygon(p, Star(Mid, 0.44f, 0.2f, 5)), Gold);
                g.Fill(p => Polygon(p, Star(Mid, 0.2f, 0.09f, 5)), new Color(1f, 0.95f, 0.6f), 0f);
            },
            ["Weaken"] = g =>
            {
                g.Fill(p => Polygon(p, V(0.5f, 0.08f), V(0.86f, 0.48f), V(0.64f, 0.48f), V(0.64f, 0.9f), V(0.36f, 0.9f), V(0.36f, 0.48f), V(0.14f, 0.48f)), Purple);
            },
            ["Taunt"] = g =>
            {
                g.Fill(p => Circle(p, Mid, 0.42f), Red);
                g.Fill(p => Union(Box(p, V(0.5f, 0.6f), V(0.07f, 0.2f), 0.06f), Circle(p, V(0.5f, 0.24f), 0.075f)), Color.white, 0f);
            },
            ["Coin"] = g =>
            {
                g.Fill(p => Circle(p, Mid, 0.42f), Gold);
                g.Fill(p => Circle(p, Mid, 0.3f), GoldDark, 0f);
                g.Fill(p => Circle(p, Mid, 0.26f), Gold, 0f);
                g.Fill(p => Box(p, Mid, V(0.06f, 0.16f), 0.04f), GoldDark, 0f);
                g.Shine(p => Circle(p, Mid, 0.42f), Color.white, V(-0.05f, 0.05f), 0.35f);
            },
            ["Heart"] = g =>
            {
                g.Fill(p => Heart(p, V(0.5f, 0.5f), 1.15f), Red);
                g.Fill(p => Circle(p, V(0.36f, 0.62f), 0.07f), new Color(1f, 0.75f, 0.78f), 0f);
            },
            ["Card"] = g =>
            {
                g.Fill(p => Box(p, Mid, V(0.28f, 0.4f), 0.06f), Blue);
                g.Fill(p => Box(p, Mid, V(0.2f, 0.32f), 0.03f), new Color(0.15f, 0.3f, 0.65f), 0f);
                g.Fill(p => Polygon(p, Star(Mid, 0.16f, 0.07f, 5)), Gold, 0f);
            },
            ["Rage"] = g =>
            {
                g.Fill(p => Flame(p, V(0.5f, 0.44f), 1.1f), Orange);
                g.Fill(p => Flame(p, V(0.5f, 0.36f), 0.6f), Gold, 0f);
            },
            ["Skull"] = g => g.Fill(p => Skull(p, Mid, 1.05f), Bone),
            ["Gem"] = g =>
            {
                g.Fill(p => Polygon(p, V(0.2f, 0.62f), V(0.34f, 0.82f), V(0.66f, 0.82f), V(0.8f, 0.62f), V(0.5f, 0.14f)), new Color(0.3f, 0.95f, 0.95f));
                g.Fill(p => Polygon(p, V(0.34f, 0.62f), V(0.42f, 0.78f), V(0.58f, 0.78f), V(0.66f, 0.62f), V(0.5f, 0.3f)), new Color(0.7f, 1f, 1f), 0f);
            },
            ["Clover"] = g =>
            {
                g.Fill(p => Union(Circle(p, V(0.5f, 0.7f), 0.15f), Circle(p, V(0.3f, 0.5f), 0.15f), Circle(p, V(0.7f, 0.5f), 0.15f), Circle(p, V(0.5f, 0.32f), 0.15f),
                    Segment(p, V(0.5f, 0.4f), V(0.66f, 0.1f), 0.035f)), Green);
            },
            ["Feather"] = g =>
            {
                g.Fill(p => Union(Polygon(p, V(0.22f, 0.22f), V(0.3f, 0.6f), V(0.62f, 0.88f), V(0.84f, 0.84f), V(0.76f, 0.52f), V(0.4f, 0.26f)),
                    Segment(p, V(0.14f, 0.12f), V(0.3f, 0.3f), 0.03f)), Gold);
                g.Fill(p => Segment(p, V(0.28f, 0.28f), V(0.74f, 0.78f), 0.018f), GoldDark, 0f);
            },
            ["Sword"] = g =>
            {
                g.Fill(Sword, Steel);
                g.Fill(p => Segment(p, V(0.2f, 0.2f), V(0.36f, 0.36f), 0.045f), Wood, 0f);
            },
            ["Crit"] = g =>
            {
                g.Fill(p => Polygon(p, Star(Mid, 0.46f, 0.22f, 8, 0f)), Gold);
                g.Fill(p => Circle(p, Mid, 0.14f), Red, 0f);
            },
            ["Heal"] = g =>
            {
                g.Fill(p => Union(Box(p, Mid, V(0.13f, 0.36f), 0.05f), Box(p, Mid, V(0.36f, 0.13f), 0.05f)), Green);
            },
            ["Lifesteal"] = g =>
            {
                g.Fill(p => Drop(p, V(0.5f, 0.46f), 1.1f), new Color(0.85f, 0.1f, 0.2f));
                g.Fill(p => Union(Box(p, V(0.5f, 0.38f), V(0.05f, 0.13f)), Box(p, V(0.5f, 0.38f), V(0.13f, 0.05f))), Color.white, 0f);
            },
            ["Defense"] = g =>
            {
                g.Fill(p => ShieldShape(p, Mid, 1f), Steel);
                g.Fill(p => Box(p, V(0.5f, 0.52f), V(0.05f, 0.28f)), Blue, 0f);
            },
            ["Pause"] = g =>
            {
                g.Fill(p => Union(Box(p, V(0.36f, 0.5f), V(0.09f, 0.3f), 0.04f), Box(p, V(0.64f, 0.5f), V(0.09f, 0.3f), 0.04f)), Color.white);
            },
            ["Sound"] = g =>
            {
                g.Fill(p => Union(Box(p, V(0.26f, 0.5f), V(0.1f, 0.13f)), Polygon(p, V(0.26f, 0.5f), V(0.52f, 0.82f), V(0.52f, 0.18f))), Color.white);
                g.Fill(p => Mathf.Max(Mathf.Abs(Circle(p, V(0.5f, 0.5f), 0.26f)) - 0.035f, -(p.x - 0.62f)), Color.white);
            },
            ["Mute"] = g =>
            {
                g.Fill(p => Union(Box(p, V(0.26f, 0.5f), V(0.1f, 0.13f)), Polygon(p, V(0.26f, 0.5f), V(0.52f, 0.82f), V(0.52f, 0.18f))), Color.white);
                g.Fill(p => Union(Segment(p, V(0.62f, 0.36f), V(0.86f, 0.64f), 0.04f), Segment(p, V(0.62f, 0.64f), V(0.86f, 0.36f), 0.04f)), Red);
            },
            ["Elite"] = g =>
            {
                g.Fill(p => Union(Polygon(p, V(0.12f, 0.2f), V(0.24f, 0.08f), V(0.88f, 0.72f), V(0.76f, 0.84f)), Polygon(p, V(0.88f, 0.2f), V(0.76f, 0.08f), V(0.12f, 0.72f), V(0.24f, 0.84f))), Steel);
                g.Fill(p => Skull(p, V(0.5f, 0.52f), 0.55f), Red);
            },
            ["Shop"] = g =>
            {
                g.Fill(p => Union(Circle(p, V(0.5f, 0.38f), 0.3f), Box(p, V(0.5f, 0.72f), V(0.12f, 0.1f), 0.04f)), Wood);
                g.Fill(p => Circle(p, V(0.5f, 0.36f), 0.13f), Gold, 0.02f);
            },
            ["Chest"] = g =>
            {
                g.Fill(p => Box(p, V(0.5f, 0.38f), V(0.38f, 0.22f), 0.04f), Wood);
                g.Fill(p => Box(p, V(0.5f, 0.68f), V(0.38f, 0.12f), 0.08f), Purple);
                g.Fill(p => Box(p, V(0.5f, 0.52f), V(0.07f, 0.1f), 0.02f), Gold);
            },
            ["Campfire"] = g =>
            {
                g.Fill(p => Union(Segment(p, V(0.18f, 0.14f), V(0.82f, 0.3f), 0.06f), Segment(p, V(0.18f, 0.3f), V(0.82f, 0.14f), 0.06f)), Wood);
                g.Fill(p => Flame(p, V(0.5f, 0.52f), 0.9f), Orange);
                g.Fill(p => Flame(p, V(0.5f, 0.46f), 0.5f), Gold, 0f);
            },
            ["Crown"] = g =>
            {
                g.Fill(p => Polygon(p, V(0.14f, 0.22f), V(0.14f, 0.7f), V(0.32f, 0.5f), V(0.5f, 0.82f), V(0.68f, 0.5f), V(0.86f, 0.7f), V(0.86f, 0.22f)), Gold);
                g.Fill(p => Circle(p, V(0.5f, 0.36f), 0.06f), Red, 0f);
            },
            ["Dice"] = g =>
            {
                g.Fill(p => Box(p, Mid, V(0.36f, 0.36f), 0.1f), Bone);
                foreach (var c in new[] { V(0.32f, 0.68f), V(0.68f, 0.68f), Mid, V(0.32f, 0.32f), V(0.68f, 0.32f) })
                    g.Fill(p => Circle(p, c, 0.065f), Ink, 0f);
            },
            ["Flag"] = g =>
            {
                g.Fill(p => Union(Box(p, V(0.24f, 0.5f), V(0.035f, 0.4f)), Polygon(p, V(0.27f, 0.88f), V(0.84f, 0.74f), V(0.27f, 0.56f))), Red);
            },
        };

        public static Dictionary<string, Sprite> CreateAll(string folder)
        {
            Directory.CreateDirectory(folder);
            var sprites = new Dictionary<string, Sprite>();
            foreach (var pair in Icons())
            {
                var painter = new Painter();
                pair.Value(painter);
                sprites[pair.Key] = Save($"{folder}/{pair.Key}.png", painter.Pixels, Size, Vector4.zero);
            }
            sprites["RoundRect"] = Save($"{folder}/RoundRect.png", Shape(64, p => Box(p, Mid, V(0.5f, 0.5f), 0.34f), Color.white), 64, Vector4.one * 24f);
            sprites["Frame"] = Save($"{folder}/Frame.png", Shape(64, p => Mathf.Abs(Box(p, Mid, V(0.47f, 0.47f), 0.32f)) - 0.035f, Color.white), 64, Vector4.one * 24f);
            sprites["Pill"] = Save($"{folder}/Pill.png", Shape(64, p => Box(p, Mid, V(0.5f, 0.5f), 0.5f), Color.white), 64, Vector4.one * 31f);
            sprites["Circle"] = Save($"{folder}/Circle.png", Shape(128, p => Circle(p, Mid, 0.49f), Color.white), 128, Vector4.zero);
            sprites["Ring"] = Save($"{folder}/Ring.png", Shape(128, p => Mathf.Abs(Circle(p, Mid, 0.44f)) - 0.05f, Color.white), 128, Vector4.zero);
            sprites["Shade"] = Save($"{folder}/Shade.png", Gradient(), 64, new Vector4(0f, 8f, 0f, 8f));
            return sprites;
        }

        static Color[] Shape(int size, Func<Vector2, float> sdf, Color color)
        {
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2((x + 0.5f) / size, (y + 0.5f) / size);
                float d = sdf(p) * size;
                pixels[y * size + x] = new Color(color.r, color.g, color.b, Mathf.Clamp01(0.5f - d) * color.a);
            }
            return pixels;
        }

        static Color[] Gradient()
        {
            var pixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
                pixels[y * 64 + x] = Color.Lerp(new Color(1f, 1f, 1f, 1f), new Color(0.72f, 0.72f, 0.72f, 1f), 1f - y / 63f);
            return pixels;
        }

        static Sprite Save(string path, Color[] pixels, int size, Vector4 border)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return ImportSprite(path, border);
        }

        public static Sprite ImportSprite(string path, Vector4 border)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
