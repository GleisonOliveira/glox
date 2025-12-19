using glox.Enums;
using glox.Services.Error;
using glox.Services.Reader;

namespace glox
{
    internal class Glox
    {
        private readonly IReader _reader;
        private readonly IErrorReporter _errorReporter;

        public Glox(IReader reader, IErrorReporter errorReporter)
        {
            _reader = reader;
            _errorReporter = errorReporter;
        }
        private Task<ExitCode> Run(string command)
        {
            return Task.FromResult(ExitCode.Success);

        }

        public async Task<ExitCode> RunScriptAsync(string filePath)
        {
            var result = await _reader.ReadAsStringAsync(filePath);

            return await result.MatchAsync(
                async success => await Run(success),
                error => Task.FromResult(error)
            );

        }

        public async Task<ExitCode> RunShell()
        {
            while (true)
            {
                Console.WriteLine("> ");
                var line = Console.ReadLine();

                if (line == null) break;


                await Run(line);
            }

            return ExitCode.Success;
        }
    }
}