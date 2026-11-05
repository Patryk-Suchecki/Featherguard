using System;
using System.IO;
using Featherguard.Feel;
using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class SfxSynth
    {
        const int Rate = 44100;

        enum Wave { Sine, Triangle, Square }

        public static AudioClip[] CreateAll(string folder)
        {
            Directory.CreateDirectory(folder);
            var ids = (SfxId[])Enum.GetValues(typeof(SfxId));
            var clips = new AudioClip[ids.Length];
            foreach (var id in ids)
            {
                if (id == SfxId.None) continue;
                string path = $"{folder}/{id}.wav";
                if (GeneratedFiles.Write(path, EncodeWav(Generate(id))))
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                clips[(int)id] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            return clips;
        }

        static float[] Generate(SfxId id)
        {
            var rng = new System.Random((int)id * 7919);
            float[] b;
            switch (id)
            {
                case SfxId.Slash:
                    b = Buffer(0.22f);
                    Noise(b, rng, 0f, 0.2f, 0.9f, 0.004f, 16f, 0.55f, 0.08f);
                    Tone(b, 0f, 0.12f, 1300f, 420f, 0.22f, 0.002f, 24f, Wave.Sine);
                    break;
                case SfxId.Smash:
                    b = Buffer(0.4f);
                    Tone(b, 0f, 0.38f, 130f, 38f, 1f, 0.002f, 8f, Wave.Sine);
                    Noise(b, rng, 0f, 0.14f, 0.8f, 0.001f, 26f, 0.5f, 0.04f);
                    Tone(b, 0f, 0.09f, 320f, 90f, 0.3f, 0.001f, 30f, Wave.Square);
                    break;
                case SfxId.Hit:
                    b = Buffer(0.22f);
                    Tone(b, 0f, 0.2f, 190f, 70f, 1f, 0.001f, 16f, Wave.Sine);
                    Noise(b, rng, 0f, 0.06f, 0.7f, 0.001f, 55f, 0.7f, 0.2f);
                    break;
                case SfxId.Arrow:
                    b = Buffer(0.3f);
                    Tone(b, 0f, 0.26f, 270f, 240f, 0.5f, 0.001f, 13f, Wave.Triangle);
                    Noise(b, rng, 0.02f, 0.22f, 0.45f, 0.03f, 11f, 0.06f, 0.3f);
                    break;
                case SfxId.Magic:
                    b = Buffer(0.5f);
                    Tone(b, 0f, 0.35f, 660f, 880f, 0.35f, 0.01f, 7f, Wave.Sine);
                    Tone(b, 0.05f, 0.35f, 880f, 1180f, 0.3f, 0.01f, 7f, Wave.Sine);
                    Tone(b, 0.1f, 0.35f, 1320f, 1760f, 0.25f, 0.01f, 7f, Wave.Sine);
                    Noise(b, rng, 0f, 0.4f, 0.12f, 0.02f, 8f, 0.95f, 0.95f);
                    break;
                case SfxId.Heal:
                    b = Buffer(0.9f);
                    Tone(b, 0f, 0.7f, 880f, 880f, 0.4f, 0.005f, 5f, Wave.Sine);
                    Tone(b, 0.08f, 0.7f, 1320f, 1320f, 0.35f, 0.005f, 5f, Wave.Sine);
                    Tone(b, 0.16f, 0.7f, 1760f, 1760f, 0.3f, 0.005f, 5f, Wave.Sine);
                    break;
                case SfxId.Shield:
                    b = Buffer(0.6f);
                    Tone(b, 0f, 0.5f, 1250f, 1250f, 0.35f, 0.002f, 7f, Wave.Sine);
                    Tone(b, 0f, 0.5f, 1870f, 1870f, 0.25f, 0.002f, 8f, Wave.Sine);
                    Tone(b, 0f, 0.5f, 2490f, 2490f, 0.15f, 0.002f, 9f, Wave.Sine);
                    Tone(b, 0f, 0.15f, 400f, 380f, 0.12f, 0.002f, 15f, Wave.Square);
                    break;
                case SfxId.Buff:
                    b = Buffer(0.45f);
                    Tone(b, 0f, 0.4f, 300f, 900f, 0.25f, 0.02f, 4f, Wave.Square);
                    Tone(b, 0f, 0.4f, 600f, 1800f, 0.2f, 0.02f, 4f, Wave.Sine);
                    break;
                case SfxId.Death:
                    b = Buffer(0.7f);
                    Tone(b, 0f, 0.65f, 420f, 110f, 0.6f, 0.01f, 3.5f, Wave.Triangle);
                    Noise(b, rng, 0f, 0.1f, 0.3f, 0.001f, 30f, 0.4f, 0.1f);
                    break;
                case SfxId.Victory:
                    b = Buffer(1.4f);
                    Tone(b, 0f, 0.2f, 523f, 523f, 0.4f, 0.005f, 6f, Wave.Triangle);
                    Tone(b, 0.13f, 0.2f, 659f, 659f, 0.4f, 0.005f, 6f, Wave.Triangle);
                    Tone(b, 0.26f, 0.2f, 784f, 784f, 0.4f, 0.005f, 6f, Wave.Triangle);
                    Tone(b, 0.39f, 0.9f, 1047f, 1047f, 0.45f, 0.005f, 2.5f, Wave.Triangle);
                    Tone(b, 0.39f, 0.9f, 784f, 784f, 0.25f, 0.005f, 2.5f, Wave.Triangle);
                    break;
                case SfxId.Defeat:
                    b = Buffer(1.8f);
                    Tone(b, 0f, 0.4f, 392f, 392f, 0.4f, 0.01f, 4f, Wave.Triangle);
                    Tone(b, 0.35f, 0.4f, 311f, 311f, 0.4f, 0.01f, 4f, Wave.Triangle);
                    Tone(b, 0.7f, 1f, 262f, 250f, 0.45f, 0.01f, 2.5f, Wave.Triangle);
                    break;
                case SfxId.Click:
                    b = Buffer(0.06f);
                    Tone(b, 0f, 0.05f, 1800f, 1500f, 0.5f, 0.001f, 70f, Wave.Sine);
                    break;
                case SfxId.Rage:
                    b = Buffer(0.7f);
                    Tone(b, 0f, 0.6f, 180f, 1400f, 0.25f, 0.05f, 3f, Wave.Square);
                    Noise(b, rng, 0f, 0.55f, 0.3f, 0.1f, 5f, 0.1f, 0.6f);
                    break;
                case SfxId.Whoosh:
                    b = Buffer(0.3f);
                    Noise(b, rng, 0f, 0.28f, 0.6f, 0.08f, 10f, 0.05f, 0.25f);
                    break;
                case SfxId.Tick:
                    b = Buffer(0.04f);
                    Tone(b, 0f, 0.035f, 2400f, 2000f, 0.45f, 0.0005f, 90f, Wave.Square);
                    break;
                case SfxId.Coin:
                    b = Buffer(0.35f);
                    Tone(b, 0f, 0.08f, 1976f, 1976f, 0.4f, 0.001f, 20f, Wave.Square);
                    Tone(b, 0.07f, 0.28f, 2637f, 2637f, 0.45f, 0.001f, 9f, Wave.Square);
                    break;
                case SfxId.Jackpot:
                    b = Buffer(1.6f);
                    for (int i = 0; i < 8; i++)
                        Tone(b, i * 0.07f, 0.5f, 523f * Mathf.Pow(2f, i / 4f), 523f * Mathf.Pow(2f, i / 4f), 0.3f, 0.002f, 6f, Wave.Square);
                    Tone(b, 0.56f, 1f, 2093f, 2093f, 0.3f, 0.005f, 3f, Wave.Triangle);
                    Tone(b, 0.56f, 1f, 1568f, 1568f, 0.3f, 0.005f, 3f, Wave.Triangle);
                    Noise(b, rng, 0.5f, 1f, 0.15f, 0.01f, 4f, 0.2f, 0.9f);
                    break;
                case SfxId.Lose:
                    b = Buffer(0.8f);
                    Tone(b, 0f, 0.25f, 330f, 320f, 0.4f, 0.005f, 6f, Wave.Square);
                    Tone(b, 0.22f, 0.5f, 247f, 200f, 0.4f, 0.005f, 4f, Wave.Square);
                    break;
                default:
                    b = Buffer(0.05f);
                    break;
            }
            Normalize(b, 0.85f);
            return b;
        }

        static float[] Buffer(float seconds) => new float[Mathf.CeilToInt(seconds * Rate)];

        static float Envelope(float t, float attack, float decay) =>
            t < attack ? t / attack : Mathf.Exp(-(t - attack) * decay);

        static void Tone(float[] buffer, float start, float duration, float f0, float f1, float amp, float attack, float decay, Wave wave)
        {
            int offset = (int)(start * Rate);
            int length = Mathf.Min((int)(duration * Rate), buffer.Length - offset);
            double phase = 0;
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / Rate;
                float freq = f0 * Mathf.Pow(f1 / f0, t / duration);
                phase += freq / Rate;
                float p = (float)(phase - Math.Floor(phase));
                float s = wave switch
                {
                    Wave.Triangle => 4f * Mathf.Abs(p - 0.5f) - 1f,
                    Wave.Square => p < 0.5f ? 0.6f : -0.6f,
                    _ => Mathf.Sin(p * Mathf.PI * 2f),
                };
                buffer[offset + i] += s * amp * Envelope(t, attack, decay);
            }
        }

        static void Noise(float[] buffer, System.Random rng, float start, float duration, float amp, float attack, float decay, float filterStart, float filterEnd)
        {
            int offset = (int)(start * Rate);
            int length = Mathf.Min((int)(duration * Rate), buffer.Length - offset);
            float state = 0f;
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / Rate;
                float k = Mathf.Lerp(filterStart, filterEnd, t / duration);
                state += k * ((float)(rng.NextDouble() * 2.0 - 1.0) - state);
                buffer[offset + i] += state * amp * Envelope(t, attack, decay);
            }
        }

        static void Normalize(float[] buffer, float peak)
        {
            float max = 0f;
            foreach (var s in buffer) max = Mathf.Max(max, Mathf.Abs(s));
            if (max <= 0f) return;
            for (int i = 0; i < buffer.Length; i++) buffer[i] *= peak / max;
        }

        static byte[] EncodeWav(float[] samples)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
                WriteWav(writer, samples);
            return stream.ToArray();
        }

        static void WriteWav(BinaryWriter writer, float[] samples)
        {
            int dataSize = samples.Length * 2;
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + dataSize);
            writer.Write("WAVE".ToCharArray());
            writer.Write("fmt ".ToCharArray());
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(Rate);
            writer.Write(Rate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write("data".ToCharArray());
            writer.Write(dataSize);
            foreach (var s in samples)
                writer.Write((short)(Mathf.Clamp(s, -1f, 1f) * short.MaxValue));
        }
    }
}
