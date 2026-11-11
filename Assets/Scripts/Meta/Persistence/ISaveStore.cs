namespace Featherguard.Meta
{
    public interface ISaveStore
    {
        SaveData Load();
        void Save(SaveData data);
    }
}
