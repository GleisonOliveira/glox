using glox.Core;
using glox.Enums;
using glox.Exceptions;
using glox.Services.Reader;

namespace glox.Tests.Unit.Services.Reader
{
    public class FileReaderTest
    {
        private readonly FileReader _fileReader;
        private string _fixturesPath;

        public FileReaderTest()
        {
            _fileReader = new FileReader();
            _fixturesPath = Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                "SimpleCode.glox"
            );
        }

        [Fact]
        public void FileReader_Must_Be_Instance_Of_IReader()
        {
            Assert.True(typeof(IReader).IsAssignableFrom(typeof(FileReader)));
        }

        [Fact]
        public async Task FileReader_Should_Throw_Exception_When_File_Is_Not_Found()
        {
            await Assert.ThrowsAsync<FileToRunNotFoundException>(async () =>
            {
                await _fileReader.ReadAsStringAsync("file_not_found.txt");
            });
        }

        [Fact]
        public async Task FileReader_Should_Throw_Previous_Exception_When_Is_Another_Error()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                await _fileReader.ReadAsStringAsync(null);
                #pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            });
        }

        [Fact]
        public async Task FileReader_Should_Open_File_And_Return_String()
        {
            var content = await _fileReader.ReadAsStringAsync(_fixturesPath);

            var success = Assert.IsType<Result<string, ExitCode>.Success>(content);

            Assert.Equal("hello world", success.Value);
        }


    }
}
