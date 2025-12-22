namespace glox.Enums;
enum SpecialChar: int
{
    BREAKLINE = '\n',
    NULL = '\0',
    SPACE = ' ',
    ENDLINE = ';',

    // Parens and Braces
    LEFT_PAREN  = '(',
    RIGHT_PAREN = ')',
    LEFT_BRACE  = '{',
    RIGHT_BRACE = '}',

    // Pontuation
    COMMA       = ',',
    DOT         = '.',
    SEMICOLON   = ';',

    // Operators
    PLUS        = '+',
    MINUS       = '-',
    STAR        = '*',

     // lógic / relacional operators
    BANG        = '!',
    EQUAL       = '=',
    LESS        = '<',
    GREATER     = '>',
}