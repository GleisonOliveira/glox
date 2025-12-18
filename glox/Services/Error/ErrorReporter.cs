namespace glox.Services.Error
{
    internal class ErrorReporter : IErrorReporter
    {
        public void Report(string message, string? stacktrace)
        {
            var stacktraceError = stacktrace ?? "";

            Console.WriteLine($"Error '{message}' on {stacktraceError}");
        }

        public void Report(string message)
        {
            Console.WriteLine($"Error '{message}'");
        }
    }
}
