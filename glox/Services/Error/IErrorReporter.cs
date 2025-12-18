namespace glox.Services.Error
{
    internal interface IErrorReporter
    {
        /// <summary>
        /// Report only the stacktrace and message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stacktrace"></param>
        public void Report(string message, string? stacktrace);

        /// <summary>
        /// Report only the stacktrace and message
        /// </summary>
        /// <param name="message"></param>
        public void Report(string message);
    }
}
