public interface IClosablePopup
{
    void OpenPopup();
    void ClosePopup();
    bool IsOpen { get; }
}