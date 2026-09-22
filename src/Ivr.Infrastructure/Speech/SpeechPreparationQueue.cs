namespace Ivr.Infrastructure.Speech;

/// <summary>One complete preparation at a time per worker, with bounded FIFO waiters.</summary>
internal sealed class SpeechPreparationQueue
{
    private readonly object gate = new();
    private readonly LinkedList<TaskCompletionSource<bool>> waiting = new();
    private bool occupied;

    public async Task<IDisposable> EnterAsync(
        int maximumWaiting,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LinkedListNode<TaskCompletionSource<bool>> node;
        lock (gate)
        {
            if (!occupied)
            {
                occupied = true;
                return new Turn(this);
            }

            if (waiting.Count >= maximumWaiting)
                throw new TtsSynthesisException("TTS_QUEUE_FULL", "The speech preparation queue is full.");

            node = waiting.AddLast(new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously));
        }

        try
        {
            await node.Value.Task.WaitAsync(timeout, cancellationToken);
            return new Turn(this);
        }
        catch
        {
            bool assigned = false;
            lock (gate)
            {
                if (node.List is not null)
                    waiting.Remove(node);
                else
                    assigned = node.Value.Task.GetAwaiter().GetResult();
            }

            // Release a grant racing with timeout/cancellation; never strand the next order.
            if (assigned) Release();
            cancellationToken.ThrowIfCancellationRequested();
            throw new TtsSynthesisException("TTS_QUEUE_TIMEOUT", "Speech preparation waited beyond its queue budget.");
        }
    }

    private void Release()
    {
        lock (gate)
        {
            if (waiting.First is not { } next)
                occupied = false;
            else
            {
                waiting.RemoveFirst();
                next.Value.SetResult(true);
            }
        }
    }

    private sealed class Turn(SpeechPreparationQueue owner) : IDisposable
    {
        private SpeechPreparationQueue? queue = owner;

        public void Dispose() => Interlocked.Exchange(ref queue, null)?.Release();
    }
}
