using glox.Enums;
using glox.Core;

namespace glox.Services
{
    interface IReader
    {
        /// <summary>
        /// Read the file and return the string of file as UTF8
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Task<Result<string, ExitCode>> ReadAsStringAsync(string filePath);
    }
}
