using System.Collections.Concurrent;

namespace PizzaMauiApp.Kitchen;

public interface IProcessStackOrder<T>
{
    void Enqueue(T item);
    T? Dequeue();
}

public class ProcessStackOrder<T> : IProcessStackOrder<T>
{
    private readonly ConcurrentQueue<T> _queue;
    
    public ProcessStackOrder()
    {
        _queue = new ConcurrentQueue<T>();
    }

    public void Enqueue(T item)
    {
        _queue.Enqueue(item);
    }

    public T? Dequeue()
    {
        return _queue.TryDequeue(out var item) ? item : default;
    }
}