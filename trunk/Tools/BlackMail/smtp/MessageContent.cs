/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Text;
using System.Net.Mime;
using sys = System.Net.Mail;

namespace BlackMail.smtp
{
    /*
     * class for encoding message
     */
    internal class MessageContent
    {
        /*
         * maximum chars per line
         */
        private const int MAX_CHARS_PER_LINE = 76;

        private static readonly string[] quotedPrintableChars = {
            "=00", "=01", "=02", "=03", "=04", "=05", "=06", "=07", "=08", "\t", "=0A", "=0B", 
            "=0C", "=0D", "=0E", "=0F", "=10", "=11", "=12", "=13", "=14", "=15", "=16", "=17", 
            "=18", "=19", "=1A", "=1B", "=1C", "=1D", "=1E", "=1F", " ", "!", "\"", "#", "$", 
            "%", "&", "'", "(", ")", "*", "+", ",", "-", ".", "/", "0", "1", "2", "3", "4", 
            "5", "6", "7", "8", "9", ":", ";", "<", "=3D", ">", "?", "@", "A", "B", "C", "D", 
            "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", 
            "U", "V", "W", "X", "Y", "Z", "[", "\\", "]", "^", "_", "`", "a", "b", "c", "d", 
            "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", 
            "u", "v", "w", "x", "y", "z", "{", "|", "}", "~", "=7F", "=80", "=81", "=82", 
            "=83", "=84", "=85", "=86", "=87", "=88", "=89", "=8A", "=8B", "=8C", "=8D", "=8E", 
            "=8F", "=90", "=91", "=92", "=93", "=94", "=95", "=96", "=97", "=98","=99", "=9A", 
            "=9B", "=9C", "=9D", "=9E", "=9F", "=A0", "=A1", "=A2", "=A3", "=A4", "=A5", "=A6", 
            "=A7", "=A8", "=A9", "=AA", "=AB", "=AC", "=AD", "=AE", "=AF", "=B0", "=B1", "=B2", 
            "=B3", "=B4", "=B5", "=B6", "=B7", "=B8", "=B9", "=BA", "=BB", "=BC", "=BD", "=BE", 
            "=BF", "=C0", "=C1", "=C2", "=C3", "=C4", "=C5", "=C6", "=C7", "=C8", "=C9", "=CA", 
            "=CB", "=CC", "=CD", "=CE", "=CF", "=D0", "=D1", "=D2", "=D3", "=D4", "=D5", "=D6", 
            "=D7", "=D8", "=D9", "=DA", "=DB", "=DC", "=DD", "=DE", "=DF", "=E0", "=E1", "=E2", 
            "=E3", "=E4", "=E5", "=E6", "=E7", "=E8", "=E9", "=EA", "=EB", "=EC", "=ED", "=EE", 
            "=EF", "=F0", "=F1", "=F2", "=F3", "=F4", "=F5", "=F6", "=F7", "=F8", "=F9", "=FA", 
            "=FB", "=FC", "=FD", "=FE", "=FF" };

        /*
         * ctor
         */
        internal MessageContent(
            byte[] body, 
            ContentType contentType, 
            TransferEncoding transferEncoding, 
            bool encodeBody)
        {
            if (encodeBody)
            {
                switch (transferEncoding)
                {
                    case TransferEncoding.SevenBit:
                        Body = body;
                        break;
                    case TransferEncoding.QuotedPrintable:
                        Body = Encoding.ASCII.GetBytes(ToQuotedPrintable(body, false));
                        break;
                    case TransferEncoding.Base64:
                        Body = Encoding.ASCII.GetBytes(Convert.ToBase64String(body, Base64FormattingOptions.InsertLineBreaks));
                        break;
                    default:
                        throw new Exception("bad transfer encoding");
                }
            }
            else
                Body = body;

            TransferEncoding = transferEncoding;
            ContentType = contentType;
        }

        /*
         * content type
         */
        internal ContentType ContentType { get; private set; }

        /*
         * transfer encoding
         */
        internal TransferEncoding TransferEncoding { get; private set; }

        /*
         * content
         */
        internal byte[] Body { get; private set; }

        /*
         * converts bytes to quoted printable
         */
        public static string ToQuotedPrintable(byte[] bytes, bool encodeNewlines)
        {
            StringBuilder returnValue = new StringBuilder();

            int currentColumn = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                if (IsNewline(bytes, i) && !encodeNewlines)
                {
                    if (i > 0 && IsWhitespace(bytes[i - 1]))
                    {
                        returnValue.Length -= 1;

                        if (bytes[i - 1] == ' ')
                        {
                            returnValue.Append("=20");
                        }
                        else if (bytes[i - 1] == '\t')
                        {
                            returnValue.Append("=09");
                        }
                    }

                    i++;

                    returnValue.Append("\r\n");
                    currentColumn = 0;
                }
                else if (currentColumn >= MAX_CHARS_PER_LINE - 4)
                {
                    returnValue.Append("=\r\n");
                    currentColumn = 0;
                    i--;
                }
                else if (IsWhitespace(bytes[i]))
                {
                    returnValue.Append(quotedPrintableChars[bytes[i]]);
                    currentColumn += quotedPrintableChars[bytes[i]].Length;
                }
                else // non-whitespace and non-linefeed
                {
                    int bytesTillWhitespace;

                    int chars = CharactersUntilNextWhitespace(bytes, i, encodeNewlines, out bytesTillWhitespace);

                    if (chars > MAX_CHARS_PER_LINE - 4)
                    {
                        if (currentColumn != 0)
                        {
                            returnValue.Append("=\r\n");
                            currentColumn = 0;
                        }

                        for (int offset = 0; offset < bytesTillWhitespace; offset++)
                        {
                            returnValue.Append(quotedPrintableChars[bytes[i + offset]]);
                            currentColumn += quotedPrintableChars[bytes[i + offset]].Length;

                            if (currentColumn >= MAX_CHARS_PER_LINE - 4)
                            {
                                returnValue.Append("=\r\n");
                                currentColumn = 0;
                            }
                        }

                        if (bytesTillWhitespace > 0)
                        {
                            i += bytesTillWhitespace - 1;
                        }
                    }
                    else if (chars + currentColumn >= MAX_CHARS_PER_LINE - 4)
                    {
                        returnValue.Append("=\r\n");
                        currentColumn = 0;

                        for (int offset = 0; offset < bytesTillWhitespace; offset++)
                        {
                            returnValue.Append(quotedPrintableChars[bytes[i + offset]]);
                            currentColumn += quotedPrintableChars[bytes[i + offset]].Length;
                        }

                        if (bytesTillWhitespace > 0)
                        {
                            i += bytesTillWhitespace - 1;
                        }
                    }
                    else
                    {
                        for (int offset = 0; offset < bytesTillWhitespace; offset++)
                        {
                            returnValue.Append(quotedPrintableChars[bytes[i + offset]]);
                            currentColumn += quotedPrintableChars[bytes[i + offset]].Length;
                        }

                        if (bytesTillWhitespace > 0)
                        {
                            i += bytesTillWhitespace - 1;
                        }
                    }
                }
            }
            return returnValue.ToString();
        }

        /*
         * helper for determining cr/lf
         */
        private static bool IsNewline(byte[] bytes, int currentPosition)
        {
            return currentPosition < bytes.Length - 1
               && bytes[currentPosition] == '\r'
               && bytes[currentPosition + 1] == '\n';
        }

        /*
         * helper for determining whitespace
         */
        private static bool IsWhitespace(byte character)
        {
            return (character == '\t' || character == ' ');
        }

        /*
         * returns no characters before next whitespace
         */
        private static int CharactersUntilNextWhitespace(
            byte[] bytes, 
            int currentPosition, 
            bool encodeNewlines, 
            out int bytesRead)
        {
            int returnValue = 0;
            bytesRead = 0;

            while (currentPosition < bytes.Length &&
                !IsWhitespace(bytes[currentPosition])
                && (encodeNewlines || !IsNewline(bytes, currentPosition))
                && returnValue <= MAX_CHARS_PER_LINE)
            {
                bytesRead++;
                returnValue += quotedPrintableChars[bytes[currentPosition]].Length;

                currentPosition++;
            }

            return returnValue;
        }

        internal static string GetTransferEncodingName(TransferEncoding encoding)
        {
            switch (encoding)
            {
                case TransferEncoding.SevenBit:
                    return "7bit";
                case TransferEncoding.QuotedPrintable:
                    return "quoted-printable";
                case TransferEncoding.Base64:
                    return "base64";
                default:
                    throw new Exception("encoding didn't exist");
            }
        }
    }
}
