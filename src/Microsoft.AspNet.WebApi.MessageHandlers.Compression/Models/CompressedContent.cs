﻿namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression.Models
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Interfaces;

    /// <summary>
    /// Represents compressed HTTP content.
    /// </summary>
    public class CompressedContent : HttpContent
    {
        /// <summary>
        /// The content
        /// </summary>
        private readonly HttpContent content;

        /// <summary>
        /// The compressor
        /// </summary>
        private readonly ICompressor compressor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedContent"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="compressor">The compressor.</param>
        public CompressedContent(HttpContent content, ICompressor compressor)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (compressor == null)
            {
                throw new ArgumentNullException("compressor");
            }

            this.content = content;
            this.compressor = compressor;

            this.AddHeaders();
        }

        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <param name="length">The length in bytes of the HTTP content.</param>
        /// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="length" /> is a valid length; otherwise, false.</returns>
        protected override bool TryComputeLength(out long length)
        {
            length = -1;

            return false;
        }

        /// <summary>
        /// serialize to stream as an asynchronous operation.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
        /// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (content)
            {
                var contentStream = await content.ReadAsStreamAsync();

                await compressor.Compress(contentStream, stream);
            }
        }

        /// <summary>
        /// Adds the headers.
        /// </summary>
        private void AddHeaders()
        {
            foreach (var header in content.Headers)
            {
                this.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            this.Headers.ContentEncoding.Add(compressor.EncodingType);
        }
    }
}