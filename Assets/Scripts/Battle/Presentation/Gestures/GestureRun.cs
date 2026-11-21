using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Battle.Presentation
{
    public abstract class GestureRun
    {
        public const float HitShare = 0.6f;

        readonly Queue<TimingResult> cues = new();

        public bool Finished { get; private set; }
        public TimingResult Result { get; private set; }
        public virtual bool AtTarget => false;
        protected float Time { get; private set; }

        public void Tick(float deltaTime, in GestureSample input)
        {
            if (Finished) return;
            Time += deltaTime;
            Step(input);
        }

        public bool TryTakeCue(out TimingResult cue) => cues.TryDequeue(out cue);

        public abstract void Draw(GestureSketch sketch);

        protected abstract void Step(in GestureSample input);

        protected void Finish(TimingResult result)
        {
            if (Finished) return;
            Finished = true;
            Result = result;
        }

        protected void Cue(TimingResult part) => cues.Enqueue(part);

        public static TimingResult Aggregate(IReadOnlyCollection<TimingResult> parts)
        {
            if (parts.Count == 0) return TimingResult.Miss;
            if (parts.All(p => p == TimingResult.Perfect)) return TimingResult.Perfect;
            return parts.Count(p => p != TimingResult.Miss) >= parts.Count * HitShare ? TimingResult.Good : TimingResult.Miss;
        }

        protected static SketchTone ToneOf(TimingResult result) => result == TimingResult.Miss ? SketchTone.Miss : SketchTone.Hit;
    }
}
