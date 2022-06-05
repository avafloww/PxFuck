using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace PxFuck.Pixelflut;

public class PixelflutClient : IPixelflut
{
    public int CanvasWidth { get; }
    public int CanvasHeight { get; }

    private static readonly byte[] NewLine = Encoding.UTF8.GetBytes("\n");
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly CancellationTokenSource _cancelTokenSource;
    private readonly CancellationToken _cancelToken;
    private readonly Thread _workerThread;

    public PixelflutClient(string host, int port, int canvasWidth, int canvasHeight)
    {
        CanvasWidth = canvasWidth;
        CanvasHeight = canvasHeight;

        _client = new TcpClient(host, port);
        _stream = _client.GetStream();
        _cancelTokenSource = new CancellationTokenSource();
        _cancelToken = _cancelTokenSource.Token;

        _workerThread = new Thread(ProcessQueue);
        _workerThread.Start();
    }

    public void Set(int x, int y, uint data)
    {
        byte b1 = (byte) ((data >> 16) & 0xFF);
        byte b2 = (byte) ((data >> 8) & 0xFF);
        byte b3 = (byte) (data & 0xFF);

        _queue.Enqueue($"PX {x} {y} {b1:X2}{b2:X2}{b3:X2}");
    }

    public void Set(int x, int y, byte[] data)
    {
        if (data.Length != 3)
        {
            throw new Exception("provided byte array must be of length 3");
        }

        _queue.Enqueue($"PX {x} {y} {data[0]:X2}{data[1]:X2}{data[2]:X2}");
    }

    private void ProcessQueue()
    {
        string? next;
        while (!_cancelToken.IsCancellationRequested)
        {
            while (_queue.TryDequeue(out next))
            {
                _stream.Write(Encoding.UTF8.GetBytes(next));
                _stream.Write(NewLine);
            }

            Thread.Yield();
        }
    }

    public bool IsQueueEmpty()
    {
        return _queue.IsEmpty;
    }

    public void Dispose()
    {
        _cancelTokenSource.Cancel();
        _workerThread.Join();

        _stream.Close();
        _stream.Dispose();

        _client.Close();
        _client.Dispose();
    }
}
