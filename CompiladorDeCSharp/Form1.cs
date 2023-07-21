using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace CompiladorDeCSharp
{
    public partial class Form1 : Form
    {

        // Classe Token que representa um token de linguagem de programação.
        public class Token
        {
            public string Lexeme { get; set; }
            public string Type { get; set; }
            public int Line { get; set; }

            // Construtor para criar um novo token.
            public Token(string lexeme, string type, int line)
            {
                Lexeme = lexeme;
                Type = type;
                Line = line;
            }
        }

        // Classe Lexer que analisa um código-fonte e produz uma lista de tokens.
        public class Lexer
        {
            private string code;  // O código-fonte a ser analisado.
            private int currentPosition;  // A posição atual no código-fonte.
            private int currentLine;  // A linha atual no código-fonte.
            private List<Token> tokens;  // A lista de tokens produzidos.

            // Um dicionário mapeando lexemas para seus tipos de token correspondentes.
            private static readonly Dictionary<string, string> TokenTypes = new Dictionary<string, string>
    {
        //Tipos
        { "int", "INT" },
        { "float", "FLOAT" },
        { "string", "STRING" },
        { "bool", "BOOL" },
        { "char", "CHAR" },
        { "byte", "BYTE" },
        { "decimal", "DECIMAL" },
        { "double", "DOUBLE" },
        { "long", "LONG" },
        { "short", "SHORT" },

        //Estruturas
        { "if", "IF" },
        { "else", "ELSE" },
        { "while", "WHILE" },
        { "for", "FOR" },
        { "foreach", "FOREACH" },
        { "do", "DO" },

        //Booleanos
        { "true", "TRUE" },
        { "false", "FALSE" },

        //Operadores aritmeticos
        { "=", "ASSIGN" },
        { "+", "PLUS" },
        { "-", "MINUS" },
        { "*", "MULTIPLY" },
        { "/", "DIVIDE" },

        //Operadores de comparação
        { "<", "LESS" },
        { ">", "GREATER" },
        { "<=", "LESS_EQUAL" },
        { ">=", "GREATER_EQUAL" },
        { "==", "EQUAL" },
        { "!=", "NOT_EQUAL" },

        //Operadores logicos
        { "&&", "AND" },
        { "||", "OR" },
        { "!", "NOT" },

        //Operadores composto
        { "++", "INCREMENT" },
        { "--", "DECREMENT" },
        { "+=", "PLUS_ASSIGN" },
        { "-=", "MINUS_ASSIGN" },
        { "*=", "MULTIPLY_ASSIGN" },
        { "/=", "DIVIDE_ASSIGN" },
        { "%=", "MODULO_ASSIGN" },

        //Pontuação
        { "(", "LPAREN" },
        { ")", "RPAREN" },
        { "{", "LBRACE" },
        { "}", "RBRACE" },
        { ";", "SEMICOLON" },
        { ".", "DOT" },
        { ",", "COMMA" },

        //Modificadores de acesso
        { "public", "PUBLIC" },
        { "private", "PRIVATE" },
        { "protected", "PROTECTED" },

        //Identificadores
        { "namespace", "NAMESPACE" },
        { "Class", "CLASS" },
        { "using", "USING" },
        { "in", "IN" }
    };

            // Construtor que recebe o código-fonte a ser analisado.
            public Lexer(string code)
            {
                this.code = code;
                currentPosition = 0;
                currentLine = 1;
                tokens = new List<Token>();
            }

            // Método para obter a lista de tokens.
            public List<Token> GetTokens()
            {
                while (currentPosition < code.Length)
                {
                    SkipWhitespace();

                    if (currentPosition >= code.Length)
                        break;

                    if (MatchIdentifier())
                        continue;

                    if (MatchNumber())
                        continue;

                    if (MatchString())
                        continue;

                    if (MatchChar())
                        continue;

                    if (MatchCommentSingleLine())
                        continue;

                    if (MatchCommentMultiLine())
                        continue;

                    if (MatchSymbol())
                        continue;

                    // Se chegou aqui, encontrou um token inválido
                    string invalidToken = code.Substring(currentPosition, 1);
                    Console.WriteLine($"Token inválido: {invalidToken} na linha {currentLine}");
                    currentPosition++;
                }

                return tokens;
            }

            // Método para ignorar espaços em branco e linhas novas.
            private void SkipWhitespace()
            {
                while (currentPosition < code.Length && char.IsWhiteSpace(code[currentPosition]))
                {
                    if (code[currentPosition] == '\n')
                    {
                        currentLine++;
                    }

                    currentPosition++;
                }
            }

            private bool MatchIdentifier()
            {
                string pattern = @"^[a-zA-Z_][a-zA-Z0-9_]*";
                Match match = Regex.Match(code.Substring(currentPosition), pattern);

                if (match.Success)
                {
                    string lexeme = match.Value;
                    string type = TokenTypes.ContainsKey(lexeme) ? TokenTypes[lexeme] : "IDENTIFIER";
                    tokens.Add(new Token(lexeme, type, currentLine));
                    currentPosition += lexeme.Length;
                    return true;
                }

                return false;
            }

            private bool MatchNumber()
            {
                string pattern = @"^\d+(\.\d+)?f?";
                Match match = Regex.Match(code.Substring(currentPosition), pattern);

                if (match.Success)
                {
                    string lexeme = match.Value;
                    tokens.Add(new Token(lexeme, "NUMBER", currentLine));
                    currentPosition += lexeme.Length;
                    return true;
                }

                return false;
            }


            private bool MatchString()
            {
                string pattern = @"^""[^""]*""";
                Match match = Regex.Match(code.Substring(currentPosition), pattern);

                if (match.Success)
                {
                    string lexeme = match.Value;
                    tokens.Add(new Token(lexeme, "STRING", currentLine));
                    currentPosition += lexeme.Length;
                    return true;
                }

                return false;
            }

            private bool MatchChar()
            {
                string pattern = @"^'[^']+'";
                Match match = Regex.Match(code.Substring(currentPosition), pattern);

                if (match.Success)
                {
                    string lexeme = match.Value;
                    tokens.Add(new Token(lexeme, "CHAR", currentLine));
                    currentPosition += lexeme.Length;
                    return true;
                }

                return false;
            }


            private bool MatchCommentSingleLine()
            {
                if (code.Substring(currentPosition).StartsWith("//"))
                {
                    int endPosition = code.IndexOf('\n', currentPosition);
                    if (endPosition == -1)
                        endPosition = code.Length;

                    string lexeme = code.Substring(currentPosition, endPosition - currentPosition);
                    tokens.Add(new Token(lexeme, "COMMENT_SINGLELINE", currentLine));
                    currentPosition = endPosition;
                    return true;
                }

                return false;
            }

            private bool MatchCommentMultiLine()
            {
                if (code.Substring(currentPosition).StartsWith("/*"))
                {
                    int endPosition = code.IndexOf("*/", currentPosition, StringComparison.Ordinal);
                    if (endPosition == -1)
                    {
                        endPosition = code.Length;
                    }

                    string lexeme = code.Substring(currentPosition, endPosition - currentPosition);
                    tokens.Add(new Token(lexeme, "COMMENT_MULTILINE", currentLine));
                    currentPosition = endPosition + 2;
                    return true;
                }

                return false;
            }

            private bool MatchSymbol()
            {
                string currentChar = code.Substring(currentPosition, 1);

                if (TokenTypes.ContainsKey(currentChar))
                {
                    string lexeme = currentChar;
                    int lexemeLength = 1;

                    if (currentChar == "+" || currentChar == "-" || currentChar == "*" || currentChar == "/" || currentChar == "%" || currentChar == "=" || currentChar == "<" || currentChar == ">" || currentChar == "!" || currentChar == "&" || currentChar == "|")
                    {
                        int nextPosition = currentPosition + 1;
                        if (nextPosition < code.Length && code[nextPosition] == '=')
                        {
                            lexeme += code[nextPosition];
                            lexemeLength = 2;
                        }
                        else if ((currentChar == "+" || currentChar == "-") && nextPosition < code.Length && code[nextPosition] == currentChar[0])
                        {
                            lexeme += code[nextPosition];
                            lexemeLength = 2;
                        }
                        else if ((currentChar == "<" || currentChar == ">") && nextPosition < code.Length && code[nextPosition] == '=')
                        {
                            lexeme += code[nextPosition];
                            lexemeLength = 2;
                        }
                        else if (currentChar == "&" && nextPosition < code.Length && code[nextPosition] == '&')
                        {
                            lexeme += code[nextPosition];
                            lexemeLength = 2;
                        }
                        else if (currentChar == "|" && nextPosition < code.Length && code[nextPosition] == '|')
                        {
                            lexeme += code[nextPosition];
                            lexemeLength = 2;
                        }
                    }

                    tokens.Add(new Token(lexeme, TokenTypes[lexeme], currentLine));
                    currentPosition += lexemeLength;
                    return true;
                }

                return false;
            }
        }

        // Classe SymbolTable que mantém um registro de todas as variáveis declaradas e seus tipos.
        public class SymbolTable
        {
            private Dictionary<string, string> table = new Dictionary<string, string>();

            // Método para adicionar uma nova variável à tabela de símbolos.
            public void Add(string name, string type)
            {
                if (table.ContainsKey(name))
                {
                    throw new Exception($"Variavel '{name}' já foi declarada no escopo.");
                }

                table[name] = type;
            }

            // Método para obter o tipo de uma variável.
            public string Get(string name)
            {
                if (!table.ContainsKey(name))
                {
                    return null;
                }

                return table[name];
            }

            // Método para imprimir a tabela
            public void Print()
            {
                foreach (var entry in table)
                {
                    Console.WriteLine($"Variable: {entry.Key}, Type: {entry.Value}");
                }
            }
        }

        public class Parser
        {
            private List<Token> tokens = new List<Token>();
            private int currentTokenIndex;
            private Token currentToken = null;

            private SymbolTable globalTable = new SymbolTable();
            private Stack<SymbolTable> localTables = new Stack<SymbolTable>();

            public void Parse(List<Token> tokens)
            {
                this.tokens = tokens;
                currentTokenIndex = 0;
                currentToken = tokens[currentTokenIndex];

                try
                {
                    ParseProgram();
                    Console.WriteLine("Análise concluída com sucesso!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro: " + ex.Message);
                }
            }

            private void MatchExpecting(string expectedType)
            {
                // Ignora tokens de comentário
                while (currentToken.Type == "COMMENT_SINGLELINE" || currentToken.Type == "COMMENT_MULTILINE")
                {
                    currentTokenIndex++;
                    if (currentTokenIndex < tokens.Count)
                        currentToken = tokens[currentTokenIndex];
                }

                Match(expectedType);
            }

            private void Match(string expectedType)
            {
                if (currentToken.Type == expectedType)
                {
                    currentTokenIndex++;
                    if (currentTokenIndex < tokens.Count)
                        currentToken = tokens[currentTokenIndex];
                }
                else
                {
                    ThrowSyntaxError(expectedType, currentToken.Type);
                }
            }

            private void ThrowSyntaxError(string expectedType, string actualType)
            {
                throw new Exception($"esperado '{expectedType}', encontrado '{actualType}', linha {currentToken.Line}");
            }

            // Analisa o programa principal
            private void ParseProgram()
            {
                while (currentTokenIndex < tokens.Count)
                {
                    if (currentToken.Type == "NAMESPACE")
                    {
                        Match("NAMESPACE");
                        Match("IDENTIFIER");
                        Match("LBRACE");
                        while (currentToken.Type != "RBRACE")
                        {
                            ParseClassDeclaration();
                        }
                        Match("RBRACE");
                    }
                    else
                    {
                        ParseUsingDeclaration();
                    }
                }
            }

            // Analisa uma declaração 'using'
            private void ParseUsingDeclaration()
            {
                Match("USING");
                Match("IDENTIFIER");
                Match("SEMICOLON");
            }

            // Analisa a declaração de uma classe
            private void ParseClassDeclaration()
            {
                ParseAccessModifier();
                Match("CLASS");
                Match("IDENTIFIER");
                Match("LBRACE");
                while (currentToken.Type != "RBRACE")
                {
                    ParseClassMember();
                }
                Match("RBRACE");
            }

            // Analisa o modificador de acesso (public, private, protected)
            private void ParseAccessModifier()
            {
                // Ignora tokens de comentário
                while (currentToken.Type == "COMMENT_SINGLELINE" || currentToken.Type == "COMMENT_MULTILINE")
                {
                    currentTokenIndex++;
                    if (currentTokenIndex < tokens.Count)
                        currentToken = tokens[currentTokenIndex];
                }

                if (currentToken.Type == "PUBLIC")
                {
                    Match("PUBLIC");
                }
                else if (currentToken.Type == "PRIVATE")
                {
                    Match("PRIVATE");
                }
                else if (currentToken.Type == "PROTECTED")
                {
                    Match("PROTECTED");
                }
                else
                {
                    ThrowSyntaxError("ACCESS MODIFIER", currentToken.Type);
                }
            }

            // Analisa um membro de classe (variável ou método)
            private void ParseClassMember()
            {
                ParseAccessModifier();
                string type = ParseType();
                string name = currentToken.Lexeme;
                Match("IDENTIFIER");
                if (currentToken.Type == "LPAREN")
                {
                    ParseMethodDeclaration();
                }
                else
                {
                    globalTable.Add(name, type);
                    Match("SEMICOLON");
                }
            }

            // Analisa a declaração de um método
            private void ParseMethodDeclaration()
            {
                Match("LPAREN");
                localTables.Push(new SymbolTable()); // Entra em um novo escopo
                if (currentToken.Type != "RPAREN")
                {
                    ParseParameterList();
                }
                Match("RPAREN");
                Match("LBRACE");
                ParseStatementList();
                Match("RBRACE");
                localTables.Pop(); // Sai do escopo
            }

            // Analisa a lista de parâmetros de um método
            private void ParseParameterList()
            {
                do
                {
                    string type = ParseType();
                    string name = currentToken.Lexeme;
                    Match("IDENTIFIER");
                    localTables.Peek().Add(name, type);
                    if (currentToken.Type != "COMMA")
                    {
                        break;
                    }
                    Match("COMMA");
                } while (true);
            }

            // Analisa o tipo de dado
            private string ParseType()
            {
                if (currentToken.Type == "INT" || currentToken.Type == "FLOAT" || currentToken.Type == "STRING" || currentToken.Type == "BOOL" || currentToken.Type == "CHAR" || currentToken.Type == "BYTE" || currentToken.Type == "DECIMAL" || currentToken.Type == "DOUBLE" || currentToken.Type == "LONG" || currentToken.Type == "SHORT")
                {
                    string type = currentToken.Type;
                    Match(currentToken.Type);
                    return type;
                }
                else
                {
                    ThrowSyntaxError("TYPE", currentToken.Type);
                    return null; // Este código nunca será alcançado, mas o compilador não sabe disso.
                }
            }

            // Analisa uma lista de declarações de comandos (statements)
            private void ParseStatementList()
            {
                while (currentToken.Type != "RBRACE")
                {
                    // Ignora tokens de comentário
                    if (currentToken.Type == "COMMENT_SINGLELINE" || currentToken.Type == "COMMENT_MULTILINE")
                    {
                        currentTokenIndex++;
                        if (currentTokenIndex < tokens.Count)
                            currentToken = tokens[currentTokenIndex];
                        continue;
                    }

                    ParseStatement();
                }
            }

            // Analisa um fator em uma expressão
            private string ParseFactor()
            {
                string factorType = "";
                if (currentToken.Type == "IDENTIFIER")
                {
                    string identifier = currentToken.Lexeme;
                    Match("IDENTIFIER");
                    factorType = GetCurrentScopeTable().Get(identifier);
                    if (factorType == null)
                    {
                        throw new Exception($"Variavel '{identifier}' ainda não foi declarada.");
                    }
                }
                else if (currentToken.Type == "NUMBER")
                {
                    // Se o número terminar com 'f', é um float, caso contrário, é um int
                    factorType = currentToken.Lexeme.EndsWith('f') ? "FLOAT" : "INT";
                    Match("NUMBER");
                }
                else if (currentToken.Type == "STRING")
                {
                    Match("STRING");
                    factorType = "STRING";
                }
                else if (currentToken.Type == "LPAREN")
                {
                    Match("LPAREN");
                    factorType = ParseExpression();
                    Match("RPAREN");
                }
                else
                {
                    ThrowSyntaxError("IDENTIFIER, NUMBER, STRING ou LPAREN", currentToken.Type);
                }

                return factorType;
            }

            // Analisa um comando (statement)
            private void ParseStatement()
            {
                if (currentToken.Type == "IDENTIFIER")
                {
                    ParseAssignmentStatement();
                }
                else if (currentToken.Type == "DO")
                {
                    ParseDoWhileStatement();
                }
                else if (currentToken.Type == "FOREACH")
                {
                    ParseForeachStatement();
                }
                else if (currentToken.Type == "IF")
                {
                    ParseIfStatement();
                }
                else if (currentToken.Type == "WHILE")
                {
                    ParseWhileStatement();
                }
                else if (currentToken.Type == "FOR")
                {
                    ParseForStatement();
                }
                else if (currentToken.Type == "INT" || currentToken.Type == "FLOAT" || currentToken.Type == "STRING" || currentToken.Type == "BOOL")
                {
                    string type = ParseType();
                    do
                    {
                        string name = currentToken.Lexeme;
                        Match("IDENTIFIER");
                        localTables.Peek().Add(name, type);

                        if (currentToken.Type == "ASSIGN")
                        {
                            Match("ASSIGN");
                            string expressionType = ParseExpression();
                            if (type != expressionType)
                            {
                                throw new Exception($"Incompatibilidade de tipo: não é possível atribuir {expressionType} para {type}.");
                            }
                        }

                        if (currentToken.Type != "COMMA")
                        {
                            break;
                        }

                        Match("COMMA");
                    } while (true);

                    Match("SEMICOLON");
                }
                else
                {
                    ThrowSyntaxError("IDENTIFIER, IF, WHILE, FOR ou TIPO", currentToken.Type);
                }
            }

            // Analisa um comando de atribuição
            private void ParseAssignmentStatement()
            {
                do
                {
                    string name = currentToken.Lexeme;
                    Match("IDENTIFIER");
                    Match("ASSIGN");

                    string expressionType = ParseExpression();
                    string variableType = GetCurrentScopeTable().Get(name);
                    if (variableType != expressionType)
                    {
                        throw new Exception($"Incompatibilidade de tipo: não é possível atribuir {expressionType} para {variableType}.");
                    }

                    if (currentToken.Type != "COMMA")
                    {
                        break;
                    }

                    Match("COMMA");
                } while (true);

                Match("SEMICOLON");
            }

            // Retorna a tabela de símbolos do escopo atual
            private SymbolTable GetCurrentScopeTable()
            {
                if (localTables.Count > 0)
                    return localTables.Peek();
                else
                    return globalTable;
            }

            // Analisa um comando if
            private void ParseIfStatement()
            {
                Match("IF");
                Match("LPAREN");
                ParseExpression();
                Match("RPAREN");
                Match("LBRACE");
                ParseStatementList();
                Match("RBRACE");

                if (currentToken.Type == "ELSE")
                {
                    Match("ELSE");
                    Match("LBRACE");
                    ParseStatementList();
                    Match("RBRACE");
                }
            }

            // Analisa um comando while
            private void ParseWhileStatement()
            {
                Match("WHILE");
                Match("LPAREN");
                ParseExpression();
                Match("RPAREN");
                Match("LBRACE");
                ParseStatementList();
                Match("RBRACE");
            }

            // Analisa um comando for
            private void ParseForStatement()
            {
                Match("FOR");
                Match("LPAREN");

                // Inicialização da variável
                string initType = ParseType();
                string initName = currentToken.Lexeme;
                Match("IDENTIFIER");
                localTables.Peek().Add(initName, initType);
                Match("ASSIGN");
                string initExpressionType = ParseExpression();
                if (initType != initExpressionType)
                {
                    throw new Exception($"Incompatibilidade de tipo: não é possível atribuir {initExpressionType} para {initType}.");
                }
                Match("SEMICOLON");

                // Condição
                ParseExpression();
                Match("SEMICOLON");

                // Operação aritmética
                string varName = currentToken.Lexeme;
                Match("IDENTIFIER");
                string varType = GetCurrentScopeTable().Get(varName);
                if (varType == null)
                {
                    throw new Exception($"Variavel '{varName}' ainda não foi declarada.");
                }
                if (currentToken.Type == "PLUS_ASSIGN" || currentToken.Type == "MINUS_ASSIGN" ||
                    currentToken.Type == "MULTIPLY_ASSIGN" || currentToken.Type == "DIVIDE_ASSIGN" ||
                    currentToken.Type == "MODULO_ASSIGN" || currentToken.Type == "INCREMENT" ||
                    currentToken.Type == "DECREMENT")
                {
                    Match(currentToken.Type);
                }
                else
                {
                    ThrowSyntaxError("ARITHMETIC OPERATOR", currentToken.Type);
                }
                if (currentToken.Type == "NUMBER")
                {
                    Match("NUMBER");
                }
                else
                {
                    ThrowSyntaxError("NUMBER", currentToken.Type);
                }
                Match("RPAREN");

                // Corpo do loop
                Match("LBRACE");
                ParseStatementList();
                Match("RBRACE");
            }

            // Analisa um comando do-while
            private void ParseDoWhileStatement()
            {
                Match("DO");
                Match("LBRACE");
                ParseStatementList();
                Match("RBRACE");
                Match("WHILE");
                Match("LPAREN");
                ParseExpression();
                Match("RPAREN");
                Match("SEMICOLON");
            }

            // Analisa um comando foreach
            private void ParseForeachStatement()
            {
                Match("FOREACH");
                Match("LPAREN");
                ParseType();
                Match("IDENTIFIER");
                Match("IN");
                ParseExpression();
                Match("RPAREN");
                Match("LBRACE");
                ParseStatementList();
                Match("RBRACE");
            }

            // Analisa uma expressão
            private string ParseExpression()
            {
                string expressionType = ParseLogicalOrExpression();
                while (currentToken.Type == "AND" || currentToken.Type == "OR")
                {
                    string operatorType = currentToken.Type;
                    MatchExpecting(operatorType);
                    string rightType = ParseLogicalOrExpression();

                    if (expressionType != "BOOL" || rightType != "BOOL")
                    {
                        throw new Exception($"Incompatibilidade, operadores logicos só podem ser aplicados a valores booleanos.");
                    }

                    // Executa a operação lógica
                    if (operatorType == "AND")
                        expressionType = "BOOL";
                }

                return expressionType;
            }

            // Analisa uma expressão lógica OR
            private string ParseLogicalOrExpression()
            {
                string expressionType = ParseLogicalAndExpression();
                while (currentToken.Type == "OR")
                {
                    string operatorType = currentToken.Type;
                    MatchExpecting(operatorType);
                    string rightType = ParseLogicalAndExpression();

                    if (expressionType != "BOOL" || rightType != "BOOL")
                    {
                        throw new Exception($"Incompatibilidade, operadores logicos só podem ser aplicados a valores booleanos.");
                    }

                    // Executa a operação lógica
                    expressionType = "BOOL";
                }

                return expressionType;
            }

            // Analisa uma expressão lógica AND
            private string ParseLogicalAndExpression()
            {
                string expressionType = ParseEqualityExpression();
                while (currentToken.Type == "AND")
                {
                    string operatorType = currentToken.Type;
                    MatchExpecting(operatorType);
                    string rightType = ParseEqualityExpression();

                    if (expressionType != "BOOL" || rightType != "BOOL")
                    {
                        throw new Exception($"Incompatibilidade, operadores logicos só podem ser aplicados a valores booleanos.");
                    }

                    // Executa a operação lógica
                    expressionType = "BOOL";
                }

                return expressionType;
            }

            // Analisa uma expressão de igualdade (==, !=)
            private string ParseEqualityExpression()
            {
                string expressionType = ParseRelationalExpression();
                while (currentToken.Type == "EQUAL" || currentToken.Type == "NOT_EQUAL")
                {
                    string operatorType = currentToken.Type;
                    MatchExpecting(operatorType);
                    string rightType = ParseRelationalExpression();

                    if (expressionType != rightType)
                    {
                        throw new Exception($"Incompatibilidade, operadores de igualdade só podem ser aplicados a operadores do mesmo tipo.");
                    }

                    // Executa a comparação de igualdade
                    expressionType = "BOOL";
                }

                return expressionType;
            }

            // Analisa uma expressão relacional (<, >, <=, >=)
            private string ParseRelationalExpression()
            {
                string expressionType = ParseAdditiveExpression();
                while (currentToken.Type == "LESS" || currentToken.Type == "GREATER" ||
                       currentToken.Type == "LESS_EQUAL" || currentToken.Type == "GREATER_EQUAL")
                {
                    string operatorType = currentToken.Type;
                    MatchExpecting(operatorType);
                    string rightType = ParseAdditiveExpression();

                    if (expressionType != rightType)
                    {
                        throw new Exception($"Incompatibilidade, operadores relacionais só podem ser aplicados a operandos do mesmo tipo.");
                    }

                    // Executa a comparação relacional
                    expressionType = "BOOL";
                }

                return expressionType;
            }

            // Analisa uma expressão aditiva (+, -)
            private string ParseAdditiveExpression()
            {
                string expressionType = ParseMultiplicativeExpression();
                while (currentToken.Type == "PLUS" || currentToken.Type == "MINUS")
                {
                    string operatorType = currentToken.Type;
                    MatchExpecting(operatorType);
                    string rightType = ParseMultiplicativeExpression();

                    if (expressionType != rightType)
                    {
                        throw new Exception($"Incompatibilidade, operadores aritimeticos só podem ser aplicados a operandos do mesmo tipo.");
                    }

                    // Executa a operação aritmética
                    expressionType = expressionType;
                }

                return expressionType;
            }

            // Analisa uma expressão multiplicativa (*, /)
            private string ParseMultiplicativeExpression()
            {
                string expressionType = ParseFactor();
                while (currentToken.Type == "MULTIPLY" || currentToken.Type == "DIVIDE")
                {
                    string operatorType = currentToken.Type;
                    MatchExpecting(operatorType);
                    string rightType = ParseFactor();

                    if (expressionType != rightType)
                    {
                        throw new Exception($"Incompatibilidade, operadores aritimeticos só podem ser aplicados a operandos do mesmo tipo.");
                    }

                    // Executa a operação aritmética
                    expressionType = expressionType;
                }

                return expressionType;
            }

            // Analisa um termo (fator)
            private string ParseTerm()
            {
                string termType = ParseFactor();
                while (currentToken.Type == "MULTIPLY" || currentToken.Type == "DIVIDE")
                {
                    Match(currentToken.Type);
                    string factorType = ParseFactor();
                    if (termType != factorType)
                    {
                        throw new Exception($"Incompatibilidade de tipo: não é possível executar operação aritmética entre {termType} e {factorType}.");
                    }
                }

                return termType;
            }
        }



        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Código fonte
            string input = inputTxt.Text;

            Lexer lexer = new Lexer(input);
            List<Token> tokens = lexer.GetTokens();

            Parser parser = new Parser();
            parser.Parse(tokens);

            outputTxt.Text += "Lexema:\t\tTipo:\t\tLinha:\n\n";

            foreach (Token token in tokens)
                outputTxt.AppendText(token.Lexeme + "\t\t" + token.Type + "\t\t" + token.Line +"\n");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            inputTxt.Clear();
            outputTxt.Clear();
        }

        private void inputTxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void resultado_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Arquivos de texto|*.txt|Todos os arquivos|*.*";
            saveFileDialog1.Title = "Salvar Arquivo de Texto";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        // Grava o conteúdo do RichTextBox no arquivo
                        sw.Write(inputTxt.Text);
                    }

                    MessageBox.Show("Arquivo salvo com sucesso!", "Salvar Arquivo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao salvar o arquivo: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
