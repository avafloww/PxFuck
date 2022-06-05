// See https://aka.ms/new-console-template for more information

using System.Text;
using PxFuck.Pixelflut;
using PxFuck.Pixelflut.Stream;

using var pool = new PixelflutPool("localhost", 1337, 100, 100, 1);
using var stream = new PixelflutStream(pool);

// for (var x = 0; x < pool.CanvasWidth; x++)
// {
//     for (var y = 0; y < pool.CanvasHeight; y++)
//     {
//         pool.Set(x, y, 0x000000);
//     }
// }

stream.Write(Encoding.UTF8.GetBytes("Hello World!"));

while (!pool.IsQueueEmpty())
{
    Thread.Sleep(50);
}
