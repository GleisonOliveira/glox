using glox.Enums;
using glox.Exceptions;
using glox.Services.Error;
using glox.Services.Reader;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("glox.Tests")]

namespace glox
{
    internal class Program
    {
        // instance of reader
        private static IReader _reader = new FileReader();

        // instance of error reporter
        private static IErrorReporter _errorReporter = new ErrorReporter();

        static async Task<int> Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: glox [script]");
                return (int)ExitCode.IncorrectUse;
            }

            if (args.Length == 1)
            {
                return await RunFile(args[0]);
            }

            return RunPrompt();
        }

        private static async Task<ExitCode> SecureRun(Func<Task<ExitCode>> callback)
        {
            try
            {
                return await callback();
            }
            catch (SyntaxeException ex)
            {
                _errorReporter.Report(ex.Message);

                return ExitCode.IncorrectUse;
            }
            catch (FileToRunNotFoundException ex)
            {
                _errorReporter.Report(ex.Message);

                return ExitCode.FileNotFounded;
            }
            catch (Exception ex)
            {
                _errorReporter.Report(ex.Message, ex.StackTrace);

                return ExitCode.GeneralError;

            }
        }

        /// <summary>
        /// Read the file and executes
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static Task<int> RunFile(string filePath)
        {
            return SecureRun(() => new Glox(_reader, _errorReporter).RunScriptAsync(filePath)).ContinueWith(t => (int)t.Result);
        }

        /// <summary>
        /// Run an interactive shell
        /// </summary>
        private static int RunPrompt()
        {
            return 0;
        }
    }
}
