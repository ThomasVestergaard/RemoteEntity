using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteEntity.Tests;

public class Flag
{
    private readonly object _sync = new();
    private FlagPosition _position;
    private List<TaskCompletionSource> _waiters = new();
    
    public FlagPosition Position {
        get
        {
            lock (_sync)
                return _position;
        }
    }

    public bool IsRaised
    {
        get
        {
            lock (_sync)
                return _position == FlagPosition.Raised;
        }
    }

    public Task WaitForRaised(TimeSpan maxWait)
    {
        var tcs = new TaskCompletionSource();
        Task.Delay(maxWait)
            .ContinueWith(_ => tcs.TrySetException(new TimeoutException("Flag was not raised within threshold: " + maxWait)));
        
        lock (_sync)
            if (_position == FlagPosition.Raised)
                return Task.CompletedTask;
            else
                _waiters.Add(tcs);

        return tcs.Task;
    }

    public void Raise()
    {
        List<TaskCompletionSource> waiters;
        lock (_sync)
        {
            _position = FlagPosition.Raised;
            waiters = _waiters;
            _waiters = new List<TaskCompletionSource>();
        }

        foreach (var waiter in waiters)
            waiter.TrySetResult();
    }
}

public enum FlagPosition
{
    Lowered, Raised
}