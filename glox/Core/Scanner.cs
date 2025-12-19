using glox.Enums;
using System.Collections.Immutable;

namespace glox.Core
{
    internal sealed class Scanner
    {
        List<string> errors { get; init; }

        private readonly List<Token> tokens = new List<Token>();
        private readonly string _source;
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        private readonly ImmutableDictionary<char, TokenType> _simpleTypes = ImmutableDictionary.CreateRange(new[]{
            KeyValuePair.Create('(', TokenType.LEFT_PAREN),
            KeyValuePair.Create(')', TokenType.RIGHT_PAREN),
            KeyValuePair.Create('{', TokenType.LEFT_BRACE),
            KeyValuePair.Create('}', TokenType.RIGHT_BRACE),
            KeyValuePair.Create(',', TokenType.COMMA),
            KeyValuePair.Create('.', TokenType.DOT),
            KeyValuePair.Create('-', TokenType.MINUS),
            KeyValuePair.Create('+', TokenType.PLUS),
            KeyValuePair.Create(';', TokenType.SEMICOLON),
            KeyValuePair.Create('*', TokenType.STAR),

        });

        private readonly ImmutableDictionary<char, (char match, TokenType trueValue, TokenType falseValue)> _combinationTypes = ImmutableDictionary.CreateRange(new[]{
            KeyValuePair.Create('!', ('=', TokenType.BANG_EQUAL, TokenType.BANG)),
            KeyValuePair.Create('=', ('=', TokenType.EQUAL_EQUAL, TokenType.EQUAL)),
            KeyValuePair.Create('<', ('=', TokenType.LESS_EQUAL, TokenType.LESS)),
            KeyValuePair.Create('>', ('=', TokenType.GREATER_EQUAL, TokenType.GREATER)),
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
            errors = new List<string>();
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
                char c = Next();

                ProcessToken(c);
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
                        while (Peek() != '\n' && !IsAtEnd()) Next();
                    }
                    else if (Match('*'))
                    {
                        while (true)
                        {
                            var next = Next();

                            if (IsAtEnd())
                            {
                                errors.Add($"Invalid comment termination on line {_line}");
                                break;
                            }

                            if (c == '\n')
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
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;
                case '\n':
                    _line++;
                    break;
                case '"':
                    AddStringToken();
                    break;
                case '\'':
                    AddStringToken('\'');
                    break;
                default:
                    if (char.IsAsciiDigit(c))
                    {
                        AddNumberToken();
                    }
                    else if (IsAlphaOrUnderline(c))
                    {
                        AddIdentifierToken();
                    }
                    else
                    {
                        errors.Add($"Invalid character '{c}' at line '{_line}'");
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
            if (_simpleTypes.Keys.Contains(c))
            {
                var token = _simpleTypes[c];

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
                if (c != combinationType.Key)
                {
                    continue;
                }

                var token = combinationType.Value;

                AddToken(Match(token.match) ? token.trueValue : token.falseValue);

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
        private void AddStringToken(char limiter = '"')
        {
            while (Peek() != limiter && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;

                Next();
            }

            if (IsAtEnd())
            {
                errors.Add($"Unterminated string at line '{_line}'");

                return;
            }

            // The closing ".
            Next();

            // Trim the surrounding quotes.
            String value = _source.Substring(_start + 1, _current - 1);

            AddToken(TokenType.STRING, value);
        }

        /// <sumary>
        /// Add a number token (double or int)
        /// </summary>
        /// <returns></returns>
        /// 
        private void AddNumberToken()
        {
            while (true)
            {
                var c = Peek();

                if (c == '\n' || IsAtEnd())
                {
                    errors.Add($"Invalid character '{c}' at line '{_line}'");
                    break;
                }

                if (char.IsAsciiDigit(c))
                {
                    Next();

                    continue;
                }

                if (c == '.' && char.IsAsciiDigit(PeekNext()))
                {
                    Next();

                    while (char.IsAsciiDigit(Peek())) Next();

                    try
                    {
                        AddToken(TokenType.NUMBER, double.Parse(_source.Substring(_start, _current).Trim()));
                    }
                    catch (OverflowException)
                    {
                        errors.Add($"The maximum number to float value is {double.Max}");
                    }

                    break;
                }

                if (c != '.')
                {
                    try
                    {
                        AddToken(TokenType.INTEGER_NUMBER, int.Parse(_source.Substring(_start, _current).Trim()));
                    }
                    catch (OverflowException)
                    {
                        errors.Add($"The maximum number to int value is {int.Max}");
                    }

                    break;
                }
            }
        }

        /// <sumary>
        /// Add an identifier token
        /// </summary>
        /// <returns></returns>
        /// 
        private void AddIdentifierToken()
        {
            while (IsAlphaNumericOrUnderline(Peek())) Next();

            var text = _source.Substring(_start, _current);
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

            _current++;

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

            return _source[_current];
        }

        /// <summary>
        /// Peek the current char
        /// </summary>
        /// <returns></returns>
        private char Peek()
        {
            if (IsAtEnd()) return '\0';

            return _source[_current];
        }


        /// <summary>
        /// Peek the next char but not move the pointer
        /// </summary>
        /// <returns></returns>
        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }


        /// <summary>
        /// Verify if is an alpha char or underline
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsAlphaOrUnderline(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        /// <summary>
        /// Verify if is an alphanumeric char or underline
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsAlphaNumericOrUnderline(char c)
        {
            return IsAlphaOrUnderline(c) || char.IsAsciiDigit(c);
        }

        #endregion

        /// <summary>
        /// Add a simple token
        /// </summary>
        /// <param name="type"></param>
        private void AddToken(TokenType type)
        {
            this.AddToken(type, null);
        }

        /// <summary>
        /// Add a token with literal
        /// </summary>
        /// <param name="type"></param>
        /// <param name="literal"></param>
        private void AddToken(TokenType type, object? literal)
        {
            var text = _source.Substring(_start, _current);

            this.tokens.Add(new Token(type, text, literal, _line));
        }
    }
}
