namespace PxFuck.Pixelflut;

public interface IPixelflut : IDisposable
{
    public int CanvasWidth { get; }
    public int CanvasHeight { get; }

    public void Set(int x, int y, uint data);

    public void Set(int x, int y, byte[] data);

    public bool IsQueueEmpty();
}
