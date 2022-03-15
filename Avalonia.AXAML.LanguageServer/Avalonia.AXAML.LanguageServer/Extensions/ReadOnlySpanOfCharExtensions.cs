using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Avalonia.AXAML.LanguageServer.Extensions
{
    public static class ReadOnlySpanOfCharExtensions
    {
        public static int PositionToOffset(this ReadOnlySpan<char> data, Position position)
        {
            return data.PositionToOffset(position.Line, position.Character);
        }

        public static Position OffsetToPosition(this ReadOnlySpan<char> data, int offset)
        {
            int character = 0;
            int line = 0;

            if (offset != 0 && data.Length < offset)
            {
                offset = data.Length - 1;
            }

            void NewLine()
            {
                line++;
                character = 0;
            }

            int i = 0;
            for (; i < offset; i++)
            {
                if (data[i] == '\n')
                {
                    NewLine();
                }
                else if (data[i] == '\r')
                {
                    bool hasRn = i + 1 < data.Length && data[i + 1] == '\n';
                    if (hasRn)
                    {
                        i++;
                    }
                    NewLine();
                }
                else
                {
                    character++;
                }
            }

            return new Position(line, character);
        }

        public static int PositionToOffset(this ReadOnlySpan<char> data, int line, int character)
        {
            int position = 0;
            for (int i = 0; i < line; i++)
            {
                position = data.FindNextLine(position);
            }
            position += character;
            return position;
        }

        private static int FindNextLine(this ReadOnlySpan<char> data, int position)
        {
            while (position < data.Length)
            {
                if (data[position] == '\n')
                {
                    position++;
                    return position;
                }
                else if (data[position] == '\r')
                {
                    bool foundRN = false;
                    if (data.Length > position + 1)
                    {
                        if (data[position + 1] == '\n')
                        {
                            foundRN = true;
                        }
                    }

                    if (foundRN)
                    {
                        position += 2;
                        return position;
                    }
                    else
                    {
                        position += 1;
                        return position;
                    }
                }
                else
                {
                    position += 1;
                }
            }

            return position;
        }
    }
}
