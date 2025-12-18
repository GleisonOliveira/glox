using glox.Enums;
using glox.Exceptions;
using glox.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        private readonly ImmutableDictionary<char, TokenType> _simpleTypes = ImmutableDictionary.CreateRange([
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

        ]);

        private readonly ImmutableDictionary<char, (char match, TokenType trueValue, TokenType falseValue)> _combinationTypes = ImmutableDictionary.CreateRange([
            KeyValuePair.Create('!', ('=', TokenType.BANG_EQUAL, TokenType.BANG)),
            KeyValuePair.Create('=', ('=', TokenType.EQUAL_EQUAL, TokenType.EQUAL)),
            KeyValuePair.Create('<', ('=', TokenType.LESS_EQUAL, TokenType.LESS)),
            KeyValuePair.Create('>', ('=', TokenType.GREATER_EQUAL, TokenType.GREATER)),
        ]);

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
                default:
                    errors.Add($"Invalid character '{c}' at line '{_line}'");
                    break;
            }
        }

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

        /// <summary>
        /// Match the token 
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
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
        /// Change to next char
        /// </summary>
        /// <returns></returns>
        private char Next()
        {
            _current++;

            return _source[_current];
        }

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

        private char Peek()
        {
            if (IsAtEnd()) return '\0';

            return _source[_current];
        }
    }
}
