public interface ISubject
{
    void Attach(IObserver observer);
    void Dettach(IObserver observer);
    void OnNotify(string message);
}
