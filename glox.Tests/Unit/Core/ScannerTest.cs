using glox.Core;
using glox.Enums;

namespace glox.Tests.Unit.Services.Core
{
    public class ScannerTest
    {
        private Dictionary<string, string> _fixtures = new();

        public ScannerTest()
        {
            var pathCombine = (string filename) => Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                filename
            );

            _fixtures.Add("code", pathCombine("Code.glox"));
            _fixtures.Add("invalidString", pathCombine("CodeInvalidString.glox"));
            _fixtures.Add("invalidSimpleStringFixturePath", pathCombine("CodeInvalidSimpleString.glox"));
            _fixtures.Add("codeValidString", pathCombine("CodeValidString.glox"));
            _fixtures.Add("codeValidInt", pathCombine("CodeValidInt.glox"));
            _fixtures.Add("codeInvalidInt", pathCombine("CodeInvalidInt.glox"));
            _fixtures.Add("codeValidDouble", pathCombine("CodeValidDouble.glox"));
        }

        private Scanner GetInstance(string filePath)
        {
            var content = new StreamReader(filePath);

            return new Scanner(content.ReadToEnd());
        }

        private void AssertTokens(List<Token> tokens)
        {
            // class
            Assert.Equal(TokenType.CLASS, tokens[0].type);

            // class name
            Assert.Equal(TokenType.IDENTIFIER, tokens[1].type);
            Assert.Equal("Glox", tokens[1].lexeme);

            // parens
            Assert.Equal(TokenType.LEFT_PAREN, tokens[5].type);
            Assert.Equal(TokenType.RIGHT_PAREN, tokens[6].type);

            // braces
            Assert.Equal(TokenType.LEFT_BRACE, tokens[2].type);
            Assert.Equal(TokenType.LEFT_BRACE, tokens[7].type);
            Assert.Equal(TokenType.RIGHT_BRACE, tokens[13].type);
            Assert.Equal(TokenType.RIGHT_BRACE, tokens[14].type);

            Assert.Equal("esse é um texto", tokens[11].literal);
        }

        [Fact]
        public void Scanner_Should_Not_Return_Any_Error()
        {
            var scanner = GetInstance(_fixtures["code"]);

            scanner.Scan();

            Assert.Empty(scanner.Errors);
        }

        [Fact]
        public void Scanner_Should_Return_String_Not_Enclosed_Error()
        {
            var scanner = GetInstance(_fixtures["invalidString"]);

            scanner.Scan();

            Assert.Single(scanner.Errors);
            Assert.Contains("Unterminated", scanner.Errors[0]);
        }

        [Fact]
        public void Scanner_Should_Return_String_Not_Enclosed_Error_With_Simple_String()
        {
            var scanner = GetInstance(_fixtures["invalidSimpleStringFixturePath"]);

            scanner.Scan();

            Assert.Single(scanner.Errors);
            Assert.Contains("Unterminated", scanner.Errors[0]);
        }

        [Fact]
        public void Scanner_Should_Parse_String()
        {
            var scanner = GetInstance(_fixtures["codeValidString"]);

           var tokens = scanner.Scan();

            Assert.Empty(scanner.Errors);

            AssertTokens(tokens);
        }

        [Fact]
        public void Scanner_Should_Parse_Int_With_Spaces_Before_Semicolon()
        {
            var scanner = GetInstance(_fixtures["codeValidInt"]);

            var tokens = scanner.Scan();

            Assert.Empty(scanner.Errors);
            Assert.Equal(123, tokens[11].literal);
            Assert.Equal(123, tokens[16].literal);
            Assert.Equal(123, tokens[21].literal);
        }

        [Fact]
        public void Scanner_Should_Return_Parse_Number_And_Separate_Strings_In_Another_Token()
        {
            var scanner = GetInstance(_fixtures["codeInvalidInt"]);

            var tokens = scanner.Scan();

            Assert.Empty(scanner.Errors);
            Assert.Equal("a", tokens[12].lexeme);
        }

        
        [Fact]
        public void Scanner_Should_Parse_Double_With_Spaces_Before_Semicolon()
        {
            var scanner = GetInstance(_fixtures["codeValidDouble"]);

            var tokens = scanner.Scan();

            Assert.Empty(scanner.Errors);
            Assert.Equal(123.45, tokens[11].literal);
            Assert.Equal(123.56, tokens[16].literal);
            Assert.Equal(123.78, tokens[21].literal);
        }
    }
}
