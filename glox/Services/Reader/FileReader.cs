using glox.Core;
using glox.Enums;
using glox.Exceptions;
using FileNotFoundException = System.IO.FileNotFoundException;

namespace glox.Services.Reader
{
    internal class FileReader : IReader
    {
        /// <summary>
        /// Read the file and return the string of file as UTF8
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<Result<string, ExitCode>> ReadAsStringAsync(string filePath)
        {
            try
            {
                var reader = new StreamReader(filePath, System.Text.Encoding.UTF8);

                return new Result<string, ExitCode>.Success(await reader.ReadToEndAsync());
            }
            catch (FileNotFoundException)
            {
                throw new FileToRunNotFoundException($"The requested file '{filePath}' was not found");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
