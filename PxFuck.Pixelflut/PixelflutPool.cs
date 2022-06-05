namespace PxFuck.Pixelflut;

public class PixelflutPool : IPixelflut
{
    public int CanvasWidth { get; }
    public int CanvasHeight { get; }

    private int _nextClient;
    private readonly int _clientCount;
    private readonly List<PixelflutClient> _clients = new();

    public PixelflutPool(string host, int port, int canvasWidth, int canvasHeight, int clientCount)
    {
        CanvasWidth = canvasWidth;
        CanvasHeight = canvasHeight;

        _clientCount = clientCount;

        for (var i = 0; i < clientCount; i++)
        {
            _clients.Add(new PixelflutClient(host, port, canvasWidth, canvasHeight));
        }
    }

    public void Set(int x, int y, uint data)
    {
        _clients[Interlocked.Increment(ref _nextClient) % _clientCount].Set(x, y, data);
    }

    public void Set(int x, int y, byte[] data)
    {
        _clients[Interlocked.Increment(ref _nextClient) % _clientCount].Set(x, y, data);
    }

    public bool IsQueueEmpty()
    {
        return _clients.TrueForAll(c => c.IsQueueEmpty());
    }

    public void Dispose()
    {
        foreach (var client in _clients)
        {
            try
            {
                client.Dispose();
            }
            catch { }
        }
    }
}
