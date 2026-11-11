namespace Featherguard.Battle.Presentation
{
    public interface ITimingInput
    {
        float FrameTime { get; }
        bool Pressed();
    }
}
