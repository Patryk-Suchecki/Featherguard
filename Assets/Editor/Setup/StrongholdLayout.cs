using System;
using System.IO;
using UnityEngine;

namespace Featherguard.EditorTools
{
    [Serializable]
    public sealed class StrongholdLayout
    {
        public CameraPlacement camera;
        public float[] campfire;
        public ModelPlacement[] models;
        public SeatPlacement[] seats;
        public LightPlacement[] lights;

        public static StrongholdLayout Load(string path) =>
            File.Exists(path) ? JsonUtility.FromJson<StrongholdLayout>(File.ReadAllText(path)) : null;

        public static Vector3 Point(float[] values) => new(values[0], values[1], values[2]);
    }

    [Serializable]
    public sealed class CameraPlacement
    {
        public float[] position;
        public float[] forward;
        public float[] up;
        public float fieldOfView;
    }

    [Serializable]
    public sealed class ModelPlacement
    {
        public string name;
        public float[] position;
        public float yaw;
        public float scale = 1f;
        public float height;
        public string place;
    }

    [Serializable]
    public sealed class SeatPlacement
    {
        public float[] position;
        public float yaw;
    }

    [Serializable]
    public sealed class LightPlacement
    {
        public string name;
        public string group;
        public float[] position;
        public float[] color;
        public float energy;
    }
}
