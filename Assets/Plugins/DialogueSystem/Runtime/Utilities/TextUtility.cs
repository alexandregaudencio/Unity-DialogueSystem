using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System;

public static class TextUtility
{
    public static bool IsWhitespace(this char character)
    {
        switch (character)
        {
            case '\u0020':
            case '\u00A0':
            case '\u1680':
            case '\u2000':
            case '\u2001':
            case '\u2002':
            case '\u2003':
            case '\u2004':
            case '\u2005':
            case '\u2006':
            case '\u2007':
            case '\u2008':
            case '\u2009':
            case '\u200A':
            case '\u202F':
            case '\u205F':
            case '\u3000':
            case '\u2028':
            case '\u2029':
            case '\u0009':
            case '\u000A':
            case '\u000B':
            case '\u000C':
            case '\u000D':
            case '\u0085':
                {
                    return true;
                }

            default:
                {
                    return false;
                }
        }
    }

    // While unnecessary for this project, I've used the method seen here: https://stackoverflow.com/a/37368176
    // Benchmarks: https://stackoverflow.com/a/37347881
    public static string RemoveWhitespaces(this string text)
    {
        int textLength = text.Length;

        char[] textCharacters = text.ToCharArray();

        int currentWhitespacelessTextLength = 0;

        for (int currentCharacterIndex = 0; currentCharacterIndex < textLength; ++currentCharacterIndex)
        {
            char currentTextCharacter = textCharacters[currentCharacterIndex];

            if (currentTextCharacter.IsWhitespace())
            {
                continue;
            }

            textCharacters[currentWhitespacelessTextLength++] = currentTextCharacter;
        }

        return new string(textCharacters, 0, currentWhitespacelessTextLength);
    }

    // See here for alternatives: https://stackoverflow.com/questions/3210393/how-do-i-remove-all-non-alphanumeric-characters-from-a-string-except-dash
    public static string RemoveSpecialCharacters(this string text)
    {
        int textLength = text.Length;

        char[] textCharacters = text.ToCharArray();

        int currentWhitespacelessTextLength = 0;

        for (int currentCharacterIndex = 0; currentCharacterIndex < textLength; ++currentCharacterIndex)
        {
            char currentTextCharacter = textCharacters[currentCharacterIndex];

            if (!char.IsLetterOrDigit(currentTextCharacter) && !currentTextCharacter.IsWhitespace())
            {
                continue;
            }

            textCharacters[currentWhitespacelessTextLength++] = currentTextCharacter;
        }

        return new string(textCharacters, 0, currentWhitespacelessTextLength);
    }

    /// <summary>
    ///     /// return string with "..." at end of the string.
    /// </summary>
    /// <param name="stringValue"></param>
    ///// <param name="maxLength">max characters in the final string.</param>
    /// <returns>return a new string with 30 characters max and "..." at end of the string.</returns>
    public static string DialogueNameRangeFormat(this string stringValue, int maxLength = 30)
    {
        string newValue = stringValue;
        if (newValue.Length > 30)
        {
            newValue = stringValue.Substring(0, maxLength) + "...";
            return newValue;
        }
        //dialogueNameTextElement.text = newValue;
        return newValue;
    }

    //public static string DialogueTextFormat(this string stringValue, int maxLength = 30)
    //{
    //    //formatar evitando com que o texto começe com espaço ou caracteres especiais.
    //    string textSanitized = stringValue.SanitizeFileName();
    //    if (!string.IsNullOrEmpty(textSanitized) || textSanitized.Length > maxLength)
    //    {
    //        textSanitized = textSanitized.Substring(0, maxLength);

    //    }
    //    return stringValue;
    //}

    public static string SanitizeFileName(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "_";
            throw new ArgumentException("File name cannot be null or empty", nameof(value));
        }

        // Lista de caracteres inválidos no nome do arquivo
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string invalidCharsPattern = new string(invalidChars);
        Regex invalidCharsRegex = new Regex($"[{Regex.Escape(invalidCharsPattern)}]");

        // Substituir caracteres inválidos por underscore
        string sanitizedFileName = invalidCharsRegex.Replace(value, "_");
        sanitizedFileName = sanitizedFileName.Replace(" ", "_");

        // Remover espaços no início e no final do nome do arquivo
        sanitizedFileName = sanitizedFileName.Trim();

        // Lista de nomes reservados
        string[] reservedNames = { "CON", "PRN", "AUX", "NUL",
                                   "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                                   "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };

        // Verificar se o nome é um dos nomes reservados
        if (reservedNames.Contains(sanitizedFileName.ToUpper()))
        {
            sanitizedFileName = '_' + sanitizedFileName;
        }

        // Limitar o comprimento do nome do arquivo se necessário
        int maxFileNameLength = 255; // Geralmente o máximo é 255 caracteres para nomes de arquivos
        if (sanitizedFileName.Length > maxFileNameLength)
        {
            sanitizedFileName = sanitizedFileName.Substring(0, maxFileNameLength);
        }

        if (sanitizedFileName.Length == 0) sanitizedFileName = "_";

        return sanitizedFileName;
    }

}