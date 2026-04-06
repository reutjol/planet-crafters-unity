public interface IWheelStateRepository
{
    WheelState Load();
    void Save(WheelState state);
}