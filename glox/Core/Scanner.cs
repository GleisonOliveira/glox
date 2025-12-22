using glox.Enums;
using glox.Records;
using System.Collections.Immutable;
using System.Globalization;

namespace glox.Core
{
    internal sealed class Scanner
    {
        public List<string> Errors { get; init; }

        private readonly List<Token> tokens = new List<Token>();
        private readonly string _source;
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        private readonly ImmutableDictionary<SpecialChar, TokenType> _simpleTypes = ImmutableDictionary.CreateRange(new[]{
            KeyValuePair.Create(SpecialChar.LEFT_PAREN, TokenType.LEFT_PAREN),
            KeyValuePair.Create(SpecialChar.RIGHT_PAREN, TokenType.RIGHT_PAREN),
            KeyValuePair.Create(SpecialChar.LEFT_BRACE, TokenType.LEFT_BRACE),
            KeyValuePair.Create(SpecialChar.RIGHT_BRACE, TokenType.RIGHT_BRACE),
            KeyValuePair.Create(SpecialChar.COMMA, TokenType.COMMA),
            KeyValuePair.Create(SpecialChar.DOT, TokenType.DOT),
            KeyValuePair.Create(SpecialChar.MINUS, TokenType.MINUS),
            KeyValuePair.Create(SpecialChar.PLUS, TokenType.PLUS),
            KeyValuePair.Create(SpecialChar.SEMICOLON, TokenType.SEMICOLON),
            KeyValuePair.Create(SpecialChar.STAR, TokenType.STAR),

        });

        private readonly ImmutableDictionary<SpecialChar, (SpecialChar match, TokenType trueValue, TokenType falseValue)> _combinationTypes = ImmutableDictionary.CreateRange(new[]{
            KeyValuePair.Create(SpecialChar.BANG, (SpecialChar.EQUAL, TokenType.BANG_EQUAL, TokenType.BANG)),
            KeyValuePair.Create(SpecialChar.EQUAL, (SpecialChar.EQUAL, TokenType.EQUAL_EQUAL, TokenType.EQUAL)),
            KeyValuePair.Create(SpecialChar.LESS, (SpecialChar.EQUAL, TokenType.LESS_EQUAL, TokenType.LESS)),
            KeyValuePair.Create(SpecialChar.GREATER, (SpecialChar.EQUAL, TokenType.GREATER_EQUAL, TokenType.GREATER)),
        });

        private readonly ImmutableDictionary<string, TokenType> _identifierTypes = ImmutableDictionary.CreateRange(new[]{
            KeyValuePair.Create("&&", TokenType.AND),
            KeyValuePair.Create("||", TokenType.OR),
            KeyValuePair.Create("and", TokenType.AND),
            KeyValuePair.Create("or", TokenType.OR),
            KeyValuePair.Create("class", TokenType.CLASS),
            KeyValuePair.Create("else", TokenType.ELSE),
            KeyValuePair.Create("false", TokenType.FALSE),
            KeyValuePair.Create("for", TokenType.FOR),
            KeyValuePair.Create("function", TokenType.FUN),
            KeyValuePair.Create("if", TokenType.IF),
            KeyValuePair.Create("null", TokenType.NIL),
            KeyValuePair.Create("print", TokenType.PRINT),
            KeyValuePair.Create("return", TokenType.RETURN),
            KeyValuePair.Create("super", TokenType.SUPER),
            KeyValuePair.Create("this", TokenType.THIS),
            KeyValuePair.Create("true", TokenType.TRUE),
            KeyValuePair.Create("false", TokenType.FALSE),
            KeyValuePair.Create("var", TokenType.VAR),
            KeyValuePair.Create("while", TokenType.WHILE),
            KeyValuePair.Create("foreach", TokenType.FOREACH),
        });

        public Scanner(string source)
        {
            Errors = new List<string>();
            _source = source;
        }

        /// <summary>
        /// Scan all tokens
        /// </summary>
        /// <returns></returns>
        public List<Token> Scan()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                char c = Peek();

                if (c == (char)SpecialChar.NULL)
                {
                    break;
                }

                ProcessToken(c);

                Next();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, _line));

            return tokens;
        }

        /// <summary>
        /// Process each token
        /// </summary>
        /// <param name="c"></param>
        private void ProcessToken(char c)
        {
            if (AddSimpleTokenIfMatch(c))
            {
                return;
            }

            if (AddCombinatedTokenIfMatch(c))
            {
                return;
            }

            switch (c)
            {
                case '/':
                    // if has two slashes, then is a comment, iterate over the line until the end of line or end of file
                    if (Match('/'))
                    {
                        while (Peek() != (char)(char)SpecialChar.BREAKLINE && !IsAtEnd()) Next();
                    }
                    else if (Match('*'))
                    {
                        while (true)
                        {
                            var next = Next();

                            if (IsAtEnd())
                            {
                                Errors.Add($"Invalid comment termination on line {_line}");
                                break;
                            }

                            if (c == (char)SpecialChar.BREAKLINE)
                            {
                                _line++;
                                continue;
                            }

                            if (next == '*' && Next() == '/')
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case (char)SpecialChar.SPACE:
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;
                case (char)SpecialChar.BREAKLINE:
                    _line++;
                    break;
                case '"':
                    AddStringToken('"');

                    break;
                case '\'':
                    AddStringToken('\'');

                    break;
                default:
                    if (char.IsDigit(c))
                    {
                        AddNumberToken();
                    }
                    else if (IsAlphaOrUnderline(c))
                    {
                        AddIdentifierToken();
                    }
                    else
                    {
                        Errors.Add($"Invalid character '{c}' at line '{_line}'");
                    }
                    break;
            }
        }

        #region "Add tokens"
        /// <summary>
        /// Add a simple token if match
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool AddSimpleTokenIfMatch(char c)
        {
            if (_simpleTypes.Keys.Contains((SpecialChar)c))
            {
                var token = _simpleTypes[(SpecialChar)c];

                AddToken(token);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a combinated token if match
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool AddCombinatedTokenIfMatch(char c)
        {
            foreach (var combinationType in _combinationTypes)
            {
                if ((SpecialChar)c != combinationType.Key)
                {
                    continue;
                }

                var token = combinationType.Value;

                AddToken(Match((char)token.match) ? token.trueValue : token.falseValue);

                return true;
            }

            return false;
        }

        /// <sumary>
        /// Add a string token
        /// </summary>
        /// <param name="limiter"></param>
        /// <returns></returns>
        /// 
        private void AddStringToken(char limiter)
        {
            var initialLine = _line;

            while (PeekNext() != limiter && !IsAtEnd())
            {
                if (PeekNext() == (char)SpecialChar.BREAKLINE) _line++;

                Next();
            }

            if (IsAtEnd())
            {
                Errors.Add($"Unterminated string at line '{initialLine}'");

                return;
            }

            // The closing ".
            Next();

            // Trim the surrounding quotes.
            var value = _source.Substring(_start + 1, _current - 1 - _start);

            AddToken(TokenType.STRING, value);
        }

        /// <sumary>
        /// Add a number token (double or int)
        /// </summary>
        /// <returns></returns>
        /// 
        private void AddNumberToken()
        {
            // Consume initial digits
            while (char.IsDigit(PeekNext()))
                Next();

            // Decimal part
            if (PeekNext() == '.')
            {
                // Consume the dot
                Next();

                if (char.IsDigit(PeekNext()))
                {
                    // Repeat until is not more number
                    while (char.IsDigit(PeekNext()))
                        Next();
                }
            }

            var text = _source.Substring(_start, _current - _start + 1);

            if (text.Contains('.'))
            {
                SecureTry(NumberType.Double, () => AddToken(TokenType.NUMBER, double.Parse(text, CultureInfo.InvariantCulture)));
            }
            else
            {
                SecureTry(NumberType.Double, () => AddToken(TokenType.INTEGER_NUMBER, int.Parse(text)));
            }
        }

        /// <sumary>
        /// Add an identifier token
        /// </summary>
        /// <returns></returns>
        /// 
        private void AddIdentifierToken()
        {
            while (IsAlphaNumericOrUnderline(PeekNext())) Next();

            var text = _source.Substring(_start, _current - _start + 1);
            var type = TokenType.IDENTIFIER;

            if (_identifierTypes.ContainsKey(text))
            {
                type = _identifierTypes[text];
            }

            AddToken(type);
        }

        #endregion

        #region "Utilities
        /// <summary>
        /// Match the token 
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        /// 
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;

            if (_source[_current++] != expected) return false;

            return true;
        }

        /// <summary>
        /// Verify if is the end of file
        /// </summary>
        /// <returns></returns>
        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        /// <summary>
        /// Change to next char and return it
        /// </summary>
        /// <returns></returns>
        private char Next()
        {
            _current++;

            if (_current >= _source.Length)
            {
                return (char)SpecialChar.NULL;
            }

            return _source[_current];
        }

        /// <summary>
        /// Peek the current char
        /// </summary>
        /// <returns></returns>
        private char Peek()
        {
            if (IsAtEnd()) return (char)SpecialChar.NULL;

            return _source[_current];
        }


        /// <summary>
        /// Peek the next char but not move the pointer
        /// </summary>
        /// <returns></returns>
        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return (char)SpecialChar.NULL;
            return _source[_current + 1];
        }


        /// <summary>
        /// Verify if is an alpha char or underline
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsAlphaOrUnderline(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        /// <summary>
        /// Verify if is an alphanumeric char or underline
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsAlphaNumericOrUnderline(char c)
        {
            return IsAlphaOrUnderline(c) || char.IsDigit(c);
        }

        #endregion

        /// <summary>
        /// Add a simple token
        /// </summary>
        /// <param name="type"></param>
        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        /// <summary>
        /// Add a token with literal
        /// </summary>
        /// <param name="type"></param>
        /// <param name="literal"></param>
        private void AddToken(TokenType type, object? literal)
        {
            var text = _source.Substring(_start, _current - _start + 1).Trim();

            tokens.Add(new Token(type, text, literal, _line));
        }

        private void SecureTry(NumberType type, Action conversion)
        {
            try
            {
                conversion();
            }
            catch (OverflowException)
            {
                Errors.Add($"Number of type {type} overflow at line {_line}");
            }
            catch (Exception)
            {
                Errors.Add($"Invalid number type {type} at line {_line}");
            }
        }
    }
}
