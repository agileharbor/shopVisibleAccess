﻿//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.IO;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace ShopVisibleAccess.Services
{
    public class CustomTextMessageEncoder : MessageEncoder
    {
        private CustomTextMessageEncoderFactory factory;
        private XmlWriterSettings writerSettings;
        private string contentType;
        
        public CustomTextMessageEncoder(CustomTextMessageEncoderFactory factory)
        {
            this.factory = factory;
            
            this.writerSettings = new XmlWriterSettings();
            this.writerSettings.Encoding = Encoding.GetEncoding(factory.CharSet);
            this.contentType = string.Format("{0}; charset={1}", 
                this.factory.MediaType, this.writerSettings.Encoding.HeaderName);
        }

        public override string ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        public override string MediaType
        {
            get 
            {
                return this.factory.MediaType;
            }
        }

        public override MessageVersion MessageVersion
        {
            get 
            {
                return this.factory.MessageVersion;
            }
        }

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {   
            byte[] msgContents = new byte[buffer.Count];
            Array.Copy(buffer.Array, buffer.Offset, msgContents, 0, msgContents.Length);
            bufferManager.ReturnBuffer(buffer.Array);

            MemoryStream stream = new MemoryStream(msgContents);
            return this.ReadMessage(stream, int.MaxValue);
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            XmlReader reader = XmlReader.Create(stream);
            return Message.CreateMessage(reader, maxSizeOfHeaders, this.MessageVersion);
        }

        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
	        using( var stream = new MemoryStream() )
	        {
		        using( var writer = XmlWriter.Create( stream, this.writerSettings ) )
			        message.WriteMessage( writer );

		        var messageBytes = stream.GetBuffer();
		        var messageLength = ( int )stream.Position;

		        var totalLength = messageLength + messageOffset;
		        var totalBytes = bufferManager.TakeBuffer( totalLength );
		        Array.Copy( messageBytes, 0, totalBytes, messageOffset, messageLength );

		        var byteArray = new ArraySegment< byte >( totalBytes, messageOffset, messageLength );
		        return byteArray;
	        }
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            using( var writer = XmlWriter.Create(stream, this.writerSettings))
				message.WriteMessage(writer);
        }
    }
}
