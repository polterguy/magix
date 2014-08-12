/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;

namespace BlackMail
{
    /*
     * wraps smtp client with s/mime and encryption support
     */
    public class SmtpClient
    {
        private string _host;
        private int _port;
        private bool _ssl;

        public ICredentialsByHost Credentials { get; set; }

        public SmtpClient(string host, int port, bool ssl)
        {
            _host = host;
            _port = port;
            _ssl = ssl;
        }

        /*
         * sends an email message
         */
        public void Send(MailMessage msg)
        {
            using (TcpClient tcp = new TcpClient())
            {
                tcp.Connect(_host, _port);
                using (Stream stream = tcp.GetStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.AutoFlush = true;
                            SendEhlo(reader, writer);
                            if (_ssl)
                                SendMessageSsl(msg, stream, reader, writer);
                            else
                                SendMessage(msg, reader, writer);
                        }
                    }
                }
            }
        }

        /*
         * opens a seesion by sending the EHLO command
         */
        private void SendEhlo(StreamReader reader, StreamWriter writer)
        {
            string welcomeLine = reader.ReadLine();
            if (!welcomeLine.StartsWith("220"))
            {
                writer.WriteLine("QUIT");
                throw new Exception("couldn't connect to remote server '" + _host + "'. 'EHLO' returned; '" + welcomeLine + "'");
            }
            writer.WriteLine("EHLO " + _host);
            string response = "";
            while (true)
            {
                string buffer = reader.ReadLine();
                response += buffer + "\r\n";
                if (buffer[3] != '-')
                    break; // seen the last line ...
            }
            if (!response.StartsWith("2"))
            {
                writer.WriteLine("QUIT");
                throw new Exception("couldn't handshake to remote server '" + _host + "'. 'EHLO' returned; '" + response + "'");
            }
        }

        /*
         * send message over SSL
         */
        private void SendMessageSsl(MailMessage msg, Stream stream, StreamReader reader, StreamWriter writer)
        {
            writer.WriteLine("STARTTLS");
            string response = reader.ReadLine();
            if (!response.StartsWith("2"))
                throw new Exception("couldn't start SSL protection with '" + _host + "'.  server returned; '" + response + "'");

            using (SslStream sslStream = new SslStream(stream))
            {
                sslStream.AuthenticateAsClient(_host);
                using (StreamReader sslReader = new StreamReader(sslStream))
                {
                    using (StreamWriter sslWriter = new StreamWriter(sslStream))
                    {
                        sslWriter.AutoFlush = true;
                        SendMessage(msg, sslReader, sslWriter);
                    }
                }
            }
        }

        /*
         * send message implementation
         */
        private void SendMessage(MailMessage msg, StreamReader reader, StreamWriter writer)
        {
            if (Credentials != null)
                AuthenticateCredentials(reader, writer);

            WriteFrom(msg, reader, writer);
            WriteRecipients(msg, reader, writer);
            WriteCarbonCopy(msg, reader, writer);
            WriteBlindCarbonCopy(msg, reader, writer);

            WriteMessage(msg, reader, writer);

            writer.WriteLine("QUIT");
        }

        private void AuthenticateCredentials(StreamReader reader, StreamWriter writer)
        {
            NetworkCredential cred = (NetworkCredential)Credentials;

            writer.WriteLine("AUTH LOGIN");
            string response = reader.ReadLine();
            if (!response.StartsWith("3"))
                throw new Exception("couldn't authenticate with server '" + _host + "'.  server returned; '" + response + "'");

            writer.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(cred.UserName), Base64FormattingOptions.None));
            response = reader.ReadLine();
            if (!response.StartsWith("3"))
                throw new Exception("couldn't authenticate with server '" + _host + "'.  server returned '" + response + "'");

            writer.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(cred.Password), Base64FormattingOptions.None));
            response = reader.ReadLine();
            if (!response.StartsWith("2"))
                throw new Exception("couldn't authenticate with server '" + _host + "'.  server returned; '" + response + "'");
        }

        /*
         * writes FROM to the smtp stream
         */
        private void WriteFrom(MailMessage msg, StreamReader reader, StreamWriter writer)
        {
            writer.WriteLine("MAIL FROM:<" + msg.From + ">");
            string response = reader.ReadLine();
            if (!response.StartsWith("2"))
                throw new Exception("communicating error with server '" + _host + "'.  MAIL FROM returned; '" + response + "'");
        }

        /*
         * writes TO to the smtp stream
         */
        private void WriteRecipients(MailMessage msg, StreamReader reader, StreamWriter writer)
        {
            foreach (string idx in msg.To)
            {
                writer.WriteLine("RCPT TO:<" + idx + ">");
                string response = reader.ReadLine();
                if (!response.StartsWith("250"))
                {
                    if (response.StartsWith("550"))
                        throw new Exception("no such email address exist, server returned; '" + response + "'");
                    throw new Exception("communicating error with server '" + _host + "'.  MAIL FROM returned; '" + response + "'");
                }
            }
        }

        /*
         * writes CC to the smtp stream
         */
        private void WriteCarbonCopy(MailMessage msg, StreamReader reader, StreamWriter writer)
        {
            foreach (string idx in msg.Cc)
            {
                writer.WriteLine("RCPT TO:<" + idx + ">");
                string response = reader.ReadLine();
                if (!response.StartsWith("250"))
                {
                    if (response.StartsWith("550"))
                        throw new Exception("no such email address exist, server returned; '" + response + "'");
                    throw new Exception("communicating error with server '" + _host + "'.  MAIL FROM returned; '" + response + "'");
                }
            }
        }

        /*
         * writes BCC to the smtp stream
         */
        private void WriteBlindCarbonCopy(MailMessage msg, StreamReader reader, StreamWriter writer)
        {
            foreach (string idx in msg.Bcc)
            {
                writer.WriteLine("RCPT TO:<" + idx + ">");
                string response = reader.ReadLine();
                if (!response.StartsWith("250"))
                {
                    if (response.StartsWith("550"))
                        throw new Exception("no such email address exist, server returned; '" + response + "'");
                    throw new Exception("communicating error with server '" + _host + "'.  MAIL FROM returned; '" + response + "'");
                }
            }
        }

        /*
         * writes DATA of email message
         */
        private void WriteMessage(MailMessage msg, StreamReader reader, StreamWriter writer)
        {
            writer.WriteLine("DATA");
            string response = reader.ReadLine();
            if (!response.StartsWith("354"))
                throw new Exception("error while communicating with server '" + _host + "'.  DATA returned; '" + response + "'");

            // writing headers
            writer.WriteLine("Subject: " + EncodeHeader(msg.Subject));
            foreach (string idx in msg.To)
            {
                writer.WriteLine("TO: " + EncodeHeader(idx));
            }
            foreach (string idx in msg.Cc)
            {
                writer.WriteLine("CC: " + EncodeHeader(idx));
            }
            foreach (string idx in msg.Bcc)
            {
                writer.WriteLine("BCC: " + EncodeHeader(idx));
            }

            // writing body
            writer.WriteLine(msg.Body);

            // eof
            writer.Write("\r\n.\r\n");
            response = reader.ReadLine();
            if (!response.StartsWith("2"))
                throw new Exception("error while communicating with server '" + _host + "'.  message after DATA returned; '" + response + "'");
        }

        /*
         * encodes header as BASE64 if extended characters are found
         */
        public static string EncodeHeader(string header)
        {
            bool extCharFound = false;
            foreach (char headerCharacter in header.ToCharArray())
            {
                if (headerCharacter > 127)
                    extCharFound = true;
            }

            if (extCharFound)
                return "=?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(header), Base64FormattingOptions.None) + "?=";
            else
                return header;
        }
    }
}
