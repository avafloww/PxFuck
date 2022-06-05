using System.Diagnostics;
using ReedSolomonCore;

namespace PxFuck.Pixelflut.Stream;

public class PixelflutStream : System.IO.Stream
{
    private static readonly uint[] SENTINEL =
    {
        0x504653, // PFS
        0x696969, // funny sex number
        0x420420, // funny weed number
    };

    public string? ReadUrl { get; }
    public TimeSpan? ReadFrequency { get; }
    public IPixelflut Pixelflut { get; }

    public override bool CanRead { get; }
    public override bool CanSeek { get; }
    public override bool CanWrite { get; }
    public override long Length { get; }
    public override long Position { get; set; }

    public (int x, int y) CanvasPosition
    {
        get => ((int) (Position % Pixelflut.CanvasWidth), (int) (Position / Pixelflut.CanvasWidth));
        set => Position = value.y * Pixelflut.CanvasWidth + value.x;
    }

    public PixelflutStream(IPixelflut pixelflut, string readUrl, TimeSpan readFrequency)
    {
        Pixelflut = pixelflut;
        ReadUrl = readUrl;
        ReadFrequency = readFrequency;
    }

    public PixelflutStream(IPixelflut pixelflut)
    {
        Pixelflut = pixelflut;
        ReadUrl = null;
        ReadFrequency = null;
    }

    public override void Flush() { }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException();
        }

        if (offset < 0 || count < 0)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (offset + count > buffer.Length)
        {
            throw new ArgumentException();
        }

        // need this for the ECC encoding... todo: fix it lmao
        var data = new byte[count];
        Buffer.BlockCopy(buffer, offset, data, 0, count);

        // the payload length is the sum of the number of bytes, plus the number of ECC bytes
        var eccLength = count / 2;
        var length = count + eccLength;

        var ecc = ReedSolomonAlgorithm.Encode(data, eccLength);

        lock (Pixelflut)
        {
            // write the three sentinel pixels
            WriteSentinel();

            // write the buffer length in two pixels
            WriteColor(EncodeToColor((byte) (length >> 24), (byte) (length >> 16)));
            WriteColor(EncodeToColor((byte) (length >> 8), (byte) length));

            // write the actual data
            for (var i = offset; i < offset + count; i += 2)
            {
                WriteColor(EncodeToColor(buffer[i], i + 1 >= offset + count ? (byte) 0 : buffer[i + 1]));
            }

            // write the ecc data
            for (var i = 0; i < ecc.Length; i += 2)
            {
                WriteColor(EncodeToColor(ecc[i], i + 1 >= ecc.Length ? (byte) 0 : ecc[i + 1]));
            }
        }
    }

    private void WriteSentinel()
    {
        foreach (var t in SENTINEL)
        {
            WriteColor(t);
        }
    }

    private bool ReadSentinel()
    {
        throw new NotImplementedException();
    }

    private void WriteColor(uint color)
    {
        var (x, y) = CanvasPosition;
        Pixelflut.Set(x, y, color);
        Position++;
    }

    private uint ReadColor()
    {
        throw new NotImplementedException();
    }

    private uint EncodeToColor(byte b1, byte b2)
    {
        // encode 2 bytes into the R/G components of the pixel, then leave B for ECC
        byte[] ecc = ReedSolomonAlgorithm.Encode(new[] {b1, b2}, 1);
        Debug.Assert(ecc.Length == 1);
        return (uint) (b1 << 16 | b2 << 8 | ecc[0]);
    }

    private byte[]? DecodeFromColor(uint color)
    {
        byte[] data =
        {
            (byte) ((color >> 16) & 0xFF),
            (byte) ((color >> 8) & 0xFF)
        };
        byte[] ecc = {(byte) (color & 0xFF)};

        return ReedSolomonAlgorithm.Decode(data, ecc);
    }
}
