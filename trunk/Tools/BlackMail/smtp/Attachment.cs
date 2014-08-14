/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using sys = System.Net.Mail;
using System.Net.Mime;

namespace BlackMail.smtp
{
    /*
     * wraps an attachment
     */
    public class Attachment
    {
        #region [ -- ctors -- ]

        public Attachment(string fileName)
        {
            RawBytes = File.ReadAllBytes(fileName);
            ContentType = new ContentType();
            ContentType.Name = Path.GetFileName(fileName);
        }

        public Attachment(string fileName, string mediaType)
        {
            RawBytes = File.ReadAllBytes(fileName);
            ContentType = new ContentType(mediaType);
            ContentType.Name = Path.GetFileName(fileName);
        }

        public Attachment(string fileName, ContentType contentType)
        {
            if (contentType == null)
                throw new Exception("contentType was null");

            RawBytes = File.ReadAllBytes(fileName);
            ContentType = contentType;
            ContentType.Name = Path.GetFileName(fileName);
        }

        public Attachment(Stream contentStream, string name)
        {
            if (contentStream == null)
                throw new Exception("contentStream was null");

            using (BinaryReader reader = new BinaryReader(contentStream))
            {
                contentStream.Position = 0;
                RawBytes = reader.ReadBytes((int)contentStream.Length);
            }

            ContentType = new ContentType();
            ContentType.Name = name;
        }

        public Attachment(Stream contentStream, ContentType contentType)
        {
            if (contentStream == null)
                throw new Exception("contentStream was null");

            using (BinaryReader reader = new BinaryReader(contentStream))
            {
                contentStream.Position = 0;
                RawBytes = reader.ReadBytes((int)contentStream.Length);
            }

            ContentType = contentType;
        }

        public Attachment(Stream contentStream, string name, string mediaType)
        {
            if (contentStream == null)
                throw new Exception("contentStream was null");

            using (BinaryReader reader = new BinaryReader(contentStream))
            {
                contentStream.Position = 0;
                RawBytes = reader.ReadBytes((int)contentStream.Length);
            }

            ContentType = new ContentType(mediaType);
            ContentType.Name = name;
        }

        public Attachment(byte[] contentBytes, string name)
        {
            RawBytes = contentBytes;
            ContentType = new ContentType();
            ContentType.Name = name;
        }

        public Attachment(byte[] contentBytes, ContentType contentType)
        {
            RawBytes = contentBytes;
            ContentType = contentType;
        }

        public Attachment(byte[] contentBytes, string name, string mediaType)
        {
            RawBytes = contentBytes;
            ContentType = new ContentType(mediaType);
            ContentType.Name = name;
        }

        #endregion

        /*
         * returns raw bytes of attachment
         */
        internal byte[] RawBytes
        {
            get;
            private set;
        }

        /*
         * returns content type
         */
        public ContentType ContentType
        {
            get;
            private set;
        }
    }
}
