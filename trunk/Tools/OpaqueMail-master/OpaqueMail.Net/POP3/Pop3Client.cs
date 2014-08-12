/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 * 
 * This file is based upon the work done by Bert Johnson in OpaqueMail at 
 * http://opaquemail.org/
 * 
 * Parts of the file is hence Copyright © Bert Johnson (http://bertjohnson.net/) 
 * of Bkip Inc. (http://bkip.com/)
 * 
 * The library OpaqueMail was forked to fix bugs, and to make sure it was 
 * compatible with older frameworks than 4.5
 * 
 * OpaqueMail is licensed as MIT License (http://mit-license.org/).
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail.Net
{
    public class Pop3Client : IDisposable
    {
        private bool LastCapabilitiesCheckAuthenticationState = false;
        private byte[] InternalBuffer = new byte[Constants.LARGEBUFFERSIZE];
        private TcpClient Pop3TcpClient;
        private Stream Pop3Stream;
        private String SessionWelcomeMessage = "";
        private string ServerExpirationPolicy = "";
        private string ServerImplementation = "";
        private int ServerLoginDelay = 0;
        private bool SessionIsAuthenticated = false;

        public string LastCommandIssued;
        public bool LastCommandResult = false;
        public string LastErrorMessage;
        public int Port;
        public string APOPSharedSecret = "";
        public NetworkCredential Credentials;
        public bool EnableSsl;
        public ReadOnlyMailMessageProcessingFlags ProcessingFlags = ReadOnlyMailMessageProcessingFlags.IncludeRawHeaders | ReadOnlyMailMessageProcessingFlags.IncludeRawBody;
        public HashSet<string> Capabilities = new HashSet<string>();
        public string Host;

        public bool IsAuthenticated
        {
            get
            {
                if (IsConnected)
                    return SessionIsAuthenticated;
                else
                    return false;
            }
        }

        public bool IsConnected
        {
            get
            {
                if (Pop3TcpClient != null)
                    return Pop3TcpClient.Connected;
                else
                    return false;
            }
        }

        public int ReadTimeout
        {
            get
            {
                if (Pop3Stream != null)
                    return Pop3Stream.ReadTimeout;
                else
                    return -1;
            }
            set
            {
                if (Pop3Stream != null)
                    Pop3Stream.ReadTimeout = value;
            }
        }

        public string WelcomeMessage
        {
            get { return SessionWelcomeMessage; }
        }

        public int WriteTimeout
        {
            get
            {
                if (Pop3Stream != null)
                    return Pop3Stream.WriteTimeout;
                else
                    return -1;
            }
            set
            {
                if (Pop3Stream != null)
                    Pop3Stream.WriteTimeout = value;
            }
        }

        public Pop3Client(string host, int port, string userName, string password, bool enableSSL)
        {
            Host = host;
            Port = port;
            Credentials = new NetworkCredential(userName, password);
            EnableSsl = enableSSL;
        }

        public void Dispose()
        {
            if (Pop3TcpClient != null)
            {
                if (Pop3TcpClient.Connected)
                {
                    SendCommand("QUIT\r\n");
                    string response = ReadData();
                }
                Pop3TcpClient.Close();
                Pop3TcpClient = null;
            }
            if (Pop3Stream != null)
                Pop3Stream.Dispose();
        }

        ~Pop3Client()
        {
            if (Pop3Stream != null)
                Pop3Stream.Dispose();
            if (Pop3TcpClient != null)
                Pop3TcpClient.Close();
        }

        public bool Authenticate()
        {
            string response = "";

            // If an APOP shared secret has been established between the client and server, require that authentication.
            if (!string.IsNullOrEmpty(APOPSharedSecret))
            {
                string[] welcomeMessageComponents = WelcomeMessage.Split(' ');
                SendCommand("APOP " + Credentials.UserName + " " + Functions.MD5(Credentials.Password + welcomeMessageComponents[welcomeMessageComponents.Length - 1] + APOPSharedSecret) + "\r\n");
                response = ReadData();

                SessionIsAuthenticated = LastCommandResult;
                return LastCommandResult;
            }
            else
            {
                SendCommand("USER " + Credentials.UserName + "\r\n");
                response = ReadData();
                if (!LastCommandResult)
                    return false;

                SendCommand("PASS " + Credentials.Password + "\r\n");
                response = ReadData();

                SessionIsAuthenticated = LastCommandResult;
                return LastCommandResult;
            }
        }

        public bool Connect()
        {
            try
            {
                Pop3TcpClient = new TcpClient();
                Pop3TcpClient.Connect(Host, Port);
                Pop3Stream = Pop3TcpClient.GetStream();

                if (EnableSsl)
                    StartTLS();

                // Remember the welcome message.
                SessionWelcomeMessage = ReadData();
                if (!LastCommandResult)
                    return false;

                return true;
            }
            catch
            {
                if (Pop3Stream != null)
                    Pop3Stream.Dispose();
                if (Pop3TcpClient != null)
                    Pop3TcpClient.Close();

                return false;
            }
        }

        public bool DeleteMessage(int index)
        {
            return Task.Run(() => DeleteMessageAsync(index)).Result;
        }

        public bool DeleteMessageUid(string uid)
        {
            return Task.Run(() => DeleteMessageAsync(uid)).Result;
        }

        public async Task<bool> DeleteMessageAsync(int index)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the DELE command.");

            await SendCommandAsync("DELE " + index.ToString() + "\r\n");
            string response = await ReadDataAsync();
            return LastCommandResult;
        }

        public async Task<bool> DeleteMessageAsync(string uid)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the DELE command.");

            await SendCommandAsync("DELE " + uid + "\r\n");
            string response = await ReadDataAsync();
            return LastCommandResult;
        }

        public bool DeleteMessages(int[] indices)
        {
            return Task.Run(() => DeleteMessagesAsync(indices)).Result;
        }

        public async Task<bool> DeleteMessagesAsync(int[] indices)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the DELE command.");

            bool returnValue = true;
            foreach (int index in indices)
            {
                await SendCommandAsync("DELE " + index.ToString() + "\r\n");
                string response = await ReadDataAsync();
                returnValue &= LastCommandResult;
            }

            return returnValue;
        }

        public bool DeleteMessages(string[] uids)
        {
            return Task.Run(() => DeleteMessagesAsync(uids)).Result;
        }

        public async Task<bool> DeleteMessagesAsync(string[] uids)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the DELE command.");

            bool returnValue = true;
            foreach (string uid in uids)
            {
                await SendCommandAsync("DELE " + uid + "\r\n");
                string response = await ReadDataAsync();
                returnValue &= LastCommandResult;
            }

            return returnValue;
        }

        public string[] GetCapabilities()
        {
            return Task.Run(() => GetCapabilitiesAsync()).Result;
        }

        public async Task<string[]> GetCapabilitiesAsync()
        {
            // If we've logged in or out since last checking capabilities, ignore the cache.
            if (LastCapabilitiesCheckAuthenticationState == IsAuthenticated)
            {
                // Send pre-cached capabilities if they exist.
                if (Capabilities.Count > 0)
                    return Capabilities.ToArray();
            }

            LastCapabilitiesCheckAuthenticationState = IsAuthenticated;

            await SendCommandAsync("CAPA\r\n");
            string response = await ReadDataAsync();

            if (LastCommandResult)
            {
                ServerSupportsTop = false;
                ServerSupportsUIDL = false;

                // Ignore the first and last line of the response.
                string[] capabilitiesLines = response.Replace("\r", "").Split('\n');
                string[] capabilitiesList = new string[capabilitiesLines.Length - 1];

                // Look for known capabilities to populate Pop3Client properties.
                for (int i = 1; i < capabilitiesLines.Length; i++)
                {
                    if (capabilitiesLines[i].StartsWith("EXPIRE "))
                        ServerExpirationPolicy = capabilitiesLines[i].Substring(7);
                    else if (capabilitiesLines[i].StartsWith("IMPLEMENTATION "))
                        ServerImplementation = capabilitiesLines[i].Substring(15);
                    else if (capabilitiesLines[i].StartsWith("LOGIN-DELAY "))
                        int.TryParse(capabilitiesLines[i].Substring(12), out ServerLoginDelay);
                    else if (capabilitiesLines[i].StartsWith("LOGIN-DELAY "))
                        int.TryParse(capabilitiesLines[i].Substring(12), out ServerLoginDelay);
                    else
                    {
                        switch (capabilitiesLines[i])
                        {
                            case "SASL":
                                ServerSupportsSASL = true;
                                break;
                            case "STLS":
                                ServerSupportsSTLS = true;
                                break;
                            case "TOP":
                                ServerSupportsTop = true;
                                break;
                            case "UIDL":
                                ServerSupportsUIDL = true;
                                break;
                        }
                    }

                    capabilitiesList[i - 1] = capabilitiesLines[i];
                    Capabilities.Add(capabilitiesLines[i]);
                }

                return capabilitiesList;
            }
            else
                return new string[] { };
        }

        public ReadOnlyMailMessage GetMessage(int index)
        {
            return Task.Run(() => GetMessageAsync(index)).Result;
        }

        public ReadOnlyMailMessage GetMessageUid(string uid)
        {
            return Task.Run(() => GetMessageUidAsync(uid)).Result;
        }

        public ReadOnlyMailMessage GetMessage(int index, bool headersOnly)
        {
            return Task.Run(() => GetMessageAsync(index, headersOnly)).Result;
        }

        public ReadOnlyMailMessage GetMessageUid(string uid, bool headersOnly)
        {
            return Task.Run(() => GetMessageUidAsync(uid, headersOnly)).Result;
        }

        public async Task<ReadOnlyMailMessage> GetMessageAsync(int index)
        {
            return await GetMessageHelper(index, "", false);
        }

        public async Task<ReadOnlyMailMessage> GetMessageUidAsync(string uid)
        {
            return await GetMessageHelper(-1, uid, false);
        }

        public async Task<ReadOnlyMailMessage> GetMessageAsync(int index, bool headersOnly)
        {
            return await GetMessageHelper(index, "", headersOnly);
        }

        public async Task<ReadOnlyMailMessage> GetMessageUidAsync(string uid, bool headersOnly)
        {
            return await GetMessageHelper(-1, uid, headersOnly);
        }

        public int GetMessageCount()
        {
            return Task.Run(() => GetMessageCountAsync()).Result;
        }

        public async Task<int> GetMessageCountAsync()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the STAT command.");

            await SendCommandAsync("STAT\r\n");
            string response = await ReadDataAsync();

            // Handle POP3 server errors.
            if (!LastCommandResult)
                return -1;

            string[] responseParts = response.Split(' ');
            int numMessages = 0;
            int.TryParse(responseParts[0], out numMessages);

            return numMessages;
        }

        public List<ReadOnlyMailMessage> GetMessages()
        {
            return Task.Run(() => GetMessagesAsync()).Result;
        }

        public List<ReadOnlyMailMessage> GetMessages(int count)
        {
            return Task.Run(() => GetMessagesAsync(count)).Result;
        }

        public List<ReadOnlyMailMessage> GetMessages(int count, bool headersOnly)
        {
            return Task.Run(() => GetMessagesAsync(count, headersOnly)).Result;
        }

        public List<ReadOnlyMailMessage> GetMessages(int count, int startIndex, bool reverseOrder)
        {
            return Task.Run(() => GetMessagesAsync(count, startIndex, reverseOrder)).Result;
        }

        public List<ReadOnlyMailMessage> GetMessages(int count, int startIndex, bool reverseOrder, bool headersOnly)
        {
            return Task.Run(() => GetMessagesAsync(count, startIndex, reverseOrder, headersOnly)).Result;
        }

        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync()
        {
            return await GetMessagesAsync(25, 1, false, false);
        }

        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(int count)
        {
            return await GetMessagesAsync(count, 1, false, false);
        }

        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(int count, bool headersOnly)
        {
            return await GetMessagesAsync(count, 1, false, headersOnly);
        }

        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(int count, int startIndex, bool reverseOrder)
        {
            return await GetMessagesAsync(count, startIndex, reverseOrder, false);
        }

        public async Task<List<ReadOnlyMailMessage>> GetMessagesAsync(int count, int startIndex, bool reverseOrder, bool headersOnly)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the TOP or RETR commands.");

            string response;
            List<ReadOnlyMailMessage> messages = new List<ReadOnlyMailMessage>();
            Dictionary<int, string> uidls = new Dictionary<int, string>();

            // Try to retrieve a list of unique IDs using the UIDL command.
            if (ServerSupportsUIDL != false)
            {
                await SendCommandAsync("UIDL\r\n");
                response = (await ReadDataAsync("\r\n.\r\n")).Replace("\r", "");
                string[] responseLines = response.Split('\n');
                if (responseLines.Length > 0)
                {
                    foreach (string responseLine in responseLines)
                    {
                        string[] responseParts = responseLine.Split(new char[] { ' ' }, 2);
                        if (!uidls.ContainsKey(int.Parse(responseParts[0])))
                            uidls.Add(int.Parse(responseParts[0]), responseParts[1]);
                    }
                }
                else
                    ServerSupportsUIDL = false;
            }
 
            // Retrieve the current message count.
            int numMessages = GetMessageCount();

            if (numMessages > 0)
            {
                response = "";
                int messagesReturned = 0;

                int loopStartIndex = reverseOrder ? numMessages + 1 - startIndex : startIndex;
                int loopIterateCount = reverseOrder ? -1 : 1;
                int loopIterations = 0;
                for (int i = loopStartIndex; loopIterations < numMessages; i += loopIterateCount)
                {
                    ReadOnlyMailMessage message = await GetMessageHelper(i, "", headersOnly);
                    if (message != null)
                    {
                        message.Index = i;
                        if (ServerSupportsUIDL == true)
                        {
                            if (uidls.ContainsKey(i))
                                message.Pop3Uidl = uidls[i];
                        }
                        messages.Add(message);
                        messagesReturned++;
                    }

                    if (messagesReturned >= count || messagesReturned >= numMessages)
                        break;
                    else
                        loopIterations++;
                }
            }

            return messages;
        }

        public string GetUidl(int index)
        {
            return Task.Run(() => GetUidlAsync(index)).Result;
        }

        public async Task<string> GetUidlAsync(int index)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the UIDL command.");

            await SendCommandAsync("UIDL " + index.ToString() + "\r\n");
            string response = await ReadDataAsync();

            // Validate the response is in the correct format.
            if (LastCommandResult){
                string[] responseSections = response.Split(new char[] {' '}, 2);
                if (responseSections.Length > 1)
                    return responseSections[1];
                else
                    return "";
            }
            else
            {
                ServerSupportsUIDL = false;
                return "";
            }
        }

        public void LogOut()
        {
            SendCommand("QUIT\r\n");
            string response = ReadData();
            SessionIsAuthenticated = false;
        }

        public bool NoOp()
        {
            return Task.Run(() => NoOpAsync()).Result;
        }

        public async Task<bool> NoOpAsync()
        {
            await SendCommandAsync("NOOP\r\n");
            string response = await ReadDataAsync();

            return LastCommandResult;
        }

        public string ReadData()
        {
            return ReadData("");
        }

        public string ReadData(string endMarker)
        {
            string response = "";

            LastCommandResult = false;
            bool receivingMessage = true, firstResponse = true;
            while (receivingMessage)
            {
                int bytesRead = Pop3Stream.Read(InternalBuffer, 0, Constants.LARGEBUFFERSIZE);
                response += Encoding.UTF8.GetString(InternalBuffer, 0, bytesRead);

                // Deal with bad commands and responses with errors.
                if (firstResponse && response.StartsWith("-"))
                {
                    LastErrorMessage = response.Substring(0, response.Length - 2).Substring(response.IndexOf(" ") + 1);
                    return "";
                }

                // Check if the last sequence received ends with a line break, possibly indicating an end of message.
                if (response.EndsWith("\r\n"))
                {
                    if (endMarker.Length > 0)
                    {
                        if (response.EndsWith(endMarker))
                        {
                            receivingMessage = false;

                            // Strip start +OK message.
                            if (response.StartsWith("+OK\r\n"))
                                response = response.Substring(5);
                            else
                                response = response.Substring(4);

                            // Strip end POP3 padding.
                            response = response.Substring(0, response.Length - endMarker.Length);
                        }
                    }
                    else
                    {
                        // Check if the message includes a POP3 "OK" signature, signifying the message is complete.
                        // Eliminate POP3 message padding.
                        if (response.StartsWith("+OK\r\n"))
                        {
                            receivingMessage = false;
                            response = response.Substring(5);
                        }
                        else if (response.StartsWith("+OK"))
                        {
                            receivingMessage = false;
                            response = response.Substring(4, response.Length - 6);
                        }
                    }
                }

                firstResponse = false;
            }

            LastCommandResult = true;
            return response;
        }
        
        public async Task<string> ReadDataAsync()
        {
            return await ReadDataAsync("");
        }

        public async Task<string> ReadDataAsync(string endMarker)
        {
            StringBuilder builder = new StringBuilder();

            LastCommandResult = false;
            bool receivingMessage = true, firstResponse = true;
            while (receivingMessage)
            {
                int bytesRead = await Pop3Stream.ReadAsync(InternalBuffer, 0, Constants.LARGEBUFFERSIZE);
                string response = Encoding.UTF8.GetString(InternalBuffer, 0, bytesRead);

                builder.Append(response);

                // Deal with bad commands and responses with errors.
                if (firstResponse && response.StartsWith("-"))
                {
                    LastErrorMessage = response.Substring(0, response.Length - 2).Substring(response.IndexOf(" ") + 1);
                    return "";
                }

                // Check if the last sequence received ends with a line break, possibly indicating an end of message.
                if (builder.ToString(builder.Length - 2, 2) == "\r\n")
                {
                    if (endMarker.Length > 0)
                    {
                        if (builder.ToString(builder.Length - endMarker.Length, endMarker.Length) == endMarker)
                        {
                            receivingMessage = false;

                            // Strip start +OK message.
                            if (builder.ToString(0, 5) == "+OK\r\n")
                                builder = new StringBuilder(builder.ToString(5, builder.Length - 5));
                            else
                                builder = new StringBuilder(builder.ToString(4, builder.Length - 4));

                            // Strip end POP3 padding.
                            builder = new StringBuilder(builder.ToString(0, builder.Length - endMarker.Length));
                        }
                    }
                    else
                    {
                        // Check if the message includes a POP3 "OK" signature, signifying the message is complete.
                        // Eliminate POP3 message padding.
                        if (builder.ToString(0, 5) == "+OK\r\n")
                        {
                            receivingMessage = false;
                            builder = new StringBuilder(builder.ToString(5, builder.Length - 5));
                        }
                        else if (builder.ToString(0, 3) == "+OK")
                        {
                            receivingMessage = false;
                            builder = new StringBuilder(builder.ToString(4, builder.Length - 6));
                        }
                    }
                }

                firstResponse = false;
            }

            LastCommandResult = true;
            return builder.ToString();
        }

        public bool Reset()
        {
            return Task.Run(() => ResetAsync()).Result;
        }

        public async Task<bool> ResetAsync()
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the RSET command.");

            await SendCommandAsync("RSET\r\n");
            string response = await ReadDataAsync();

            return LastCommandResult;
        }

        public void SendCommand(string command)
        {
            LastCommandIssued = command;
            Functions.SendStreamString(Pop3Stream, InternalBuffer, command);
        }

        public async Task SendCommandAsync(string command)
        {
            LastCommandIssued = command;
            await Functions.SendStreamStringAsync(Pop3Stream, InternalBuffer, command);
        }

        public void StartTLS()
        {
            if (!(Pop3Stream is SslStream))
                Pop3Stream = new SslStream(Pop3TcpClient.GetStream());
            if (!((SslStream)Pop3Stream).IsAuthenticated)
                ((SslStream)Pop3Stream).AuthenticateAsClient(Host);
        }

        private async Task<ReadOnlyMailMessage> GetMessageHelper(int index, string uid, bool headersOnly)
        {
            // Protect against commands being called out of order.
            if (!IsAuthenticated)
                throw new Pop3Exception("Must be connected to the server and authenticated prior to calling the RETR command.");

            bool processed = false;
            string response = "";

            // Determine whether we're using the index number or UID string.
            string messageID = index > -1 ? index.ToString() : uid;

            // If retrieving headers only, first try the POP3 TOP command.
            if (headersOnly && (ServerSupportsTop != false))
            {
                await SendCommandAsync("TOP " + messageID + " 0\r\n");
                response = await ReadDataAsync("\r\n.\r\n");

                if (LastCommandResult)
                    processed = true;
                ServerSupportsTop = processed;
            }
            if (!processed)
            {
                await SendCommandAsync("RETR " + messageID + "\r\n");
                response = await ReadDataAsync("\r\n.\r\n");
            }

            if (LastCommandResult && response.Length > 0)
            {
                ReadOnlyMailMessage message = new ReadOnlyMailMessage(response, ProcessingFlags);

                if (string.IsNullOrEmpty(uid) && ServerSupportsUIDL != null)
                {
                    message.Index = index;
                    message.Pop3Uidl = await GetUidlAsync(index);
                }
                else
                    message.Pop3Uidl = uid;

                return message;
            }

            // If unable to find or parse the message, return null.
            return null;
        }

        private bool ServerSupportsPipelining = false;
        private bool? ServerSupportsTop = null;
        private bool ServerSupportsSASL = false;
        private bool ServerSupportsSTLS = false;
        private bool? ServerSupportsUIDL = null;

        public string ExpirationPolicy
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerExpirationPolicy;
            }
        }

        public string Implementation
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerImplementation;
            }
        }

        public int LoginDelay
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerLoginDelay;
            }
        }

        public bool SupportsPipelining
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerSupportsPipelining;
            }
        }

        public bool SupportsSASL
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerSupportsSASL;
            }
        }

        public bool SupportsSTLS
        {
            get
            {
                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerSupportsSTLS;
            }
        }

        public bool SupportsTop
        {
            get
            {
                // Check if we've explicitly found this to be true.
                if (ServerSupportsTop != null)
                    return ServerSupportsTop == true ? true : false;

                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerSupportsTop == true ? true : false;
            }
        }

        public bool SupportsUIDL
        {
            get
            {
                // Check if we've explicitly found this to be true.
                if (ServerSupportsUIDL != null)
                    return ServerSupportsUIDL == true ? true : false;

                // If we haven't already retrieved the server's capabilities, populate those now.
                if (Capabilities.Count < 1)
                    GetCapabilities();

                return ServerSupportsUIDL == true ? true : false;
            }
        }
    }

    public class Pop3Exception : Exception
    {
        public Pop3Exception() : base() { }
        public Pop3Exception(string message) : base(message) { }
        public Pop3Exception(string message, Exception innerException) : base(message, innerException) { }
    }
}
