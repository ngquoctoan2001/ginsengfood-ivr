using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Ivr.IntegrationTests;

// Test-only diagnostics: never retain exception messages, SQL, parameters or connection strings.
internal sealed class ApiMatrixDatabaseDiagnostics : ILoggerProvider
{
    private readonly ConcurrentQueue<string> sqlStates = new();
    public string[] SqlStates => sqlStates.Distinct().Order().ToArray();
    public ILogger CreateLogger(string categoryName) => new SafeLogger(sqlStates);
    public void Dispose() { }

    private sealed class SafeLogger(ConcurrentQueue<string> states) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            for (Exception? current = exception; current is not null; current = current.InnerException)
                if (current is PostgresException postgres) states.Enqueue(postgres.SqlState);
        }
    }
}
