using glox.Services.Error;

namespace glox.Tests.Unit.Services.Error
{
    public class ErrorReporterTest
    {
        private readonly ErrorReporter _reporter =  new ErrorReporter();
        private readonly StringWriter _output = new StringWriter();

        [Fact]
        public void ErrorReporter_Must_Be_Instance_Of_IReporter()
        {
            Assert.True(typeof(IErrorReporter).IsAssignableFrom(typeof(ErrorReporter)));
        }

        [Fact]
        public void ErrorReporter_Should_Report_On_Console()
        {
            
            Console.SetOut(_output);

            _reporter.Report("test");

            var content = _output.ToString();

            Assert.Contains("test", content);
        }

        [Fact]
        public void ErrorReporter_Should_Report_StackTrace_On_Console()
        {
            Console.SetOut(_output);

            _reporter.Report("test", "stacktrace");

            var content = _output.ToString();

            Assert.Contains("test", content);
            Assert.Contains("stacktrace", content);
        }
    }
}
