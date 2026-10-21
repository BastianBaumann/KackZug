#if DEBUG
namespace KackZug.Desktop;

/// <summary>Liest bis zur Haelfte und wartet dann, damit die Tour den Fortschritt im Update-Hinweis fotografieren kann</summary>
internal sealed class DemoPausingStream : Stream
{
    private readonly TaskCompletionSource _resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _paused = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    private byte[] _data = Array.Empty<byte>();
    private int _position;

    public Task Paused
    {
        get { return _paused.Task; }
    }

    public void SetData(byte[] data)
    {
        _data = data;
        _position = 0;
    }

    public void Resume()
    {
        _resume.TrySetResult();
    }

    public override bool CanRead
    {
        get { return true; }
    }

    public override bool CanSeek
    {
        get { return false; }
    }

    public override bool CanWrite
    {
        get { return false; }
    }

    public override long Length
    {
        get { return _data.Length; }
    }

    public override long Position
    {
        get { return _position; }
        set { throw new NotSupportedException(); }
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var half = _data.Length / 2;
        if (_position == half && !_resume.Task.IsCompleted)
        {
            _paused.TrySetResult();
            await _resume.Task.WaitAsync(cancellationToken);
        }

        var limit = _position < half ? half : _data.Length;
        var count = Math.Min(buffer.Length, limit - _position);
        _data.AsMemory(_position, count).CopyTo(buffer);
        _position += count;
        return count;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
#endif
