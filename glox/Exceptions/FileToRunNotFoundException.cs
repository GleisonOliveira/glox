namespace glox.Exceptions
{
    internal class FileToRunNotFoundException : Exception
    {
        public FileToRunNotFoundException(string? message) : base(message)
        {
        }
    }
}
