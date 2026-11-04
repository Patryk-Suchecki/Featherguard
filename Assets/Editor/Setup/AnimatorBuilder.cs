using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class AnimatorBuilder
    {
        static readonly string[] States = { "Idle", "Attack", "Cast", "Hit", "Death", "Victory" };
        static readonly string[] ReturnToIdle = { "Attack", "Cast", "Hit" };

        public static AnimatorController Build(string model)
        {
            Seeder.EnsureFolder(ContentPaths.Animation);
            string fbx = $"{ContentPaths.Characters}/{model}.fbx";
            string path = $"{ContentPaths.Animation}/{model}.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path) ?? AnimatorController.CreateAnimatorControllerAtPath(path);
            var machine = controller.layers[0].stateMachine;
            foreach (var child in machine.states.ToArray())
                machine.RemoveState(child.state);

            var clips = AssetDatabase.LoadAllAssetsAtPath(fbx)
                .OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__"))
                .ToDictionary(c => c.name);

            var states = new Dictionary<string, AnimatorState>();
            for (int i = 0; i < States.Length; i++)
            {
                var state = machine.AddState(States[i], new Vector3(300f, 60f * i, 0f));
                state.motion = clips[States[i]];
                states[States[i]] = state;
            }
            machine.defaultState = states["Idle"];

            foreach (var name in ReturnToIdle)
            {
                var transition = states[name].AddTransition(states["Idle"]);
                transition.hasExitTime = true;
                transition.exitTime = 0.95f;
                transition.duration = 0.1f;
                transition.hasFixedDuration = true;
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }
    }
}
