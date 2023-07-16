using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace sms.Controllers.BoldReports
{
    public class CompressionHelper
    {
        IHttpContextAccessor _context;
        public CompressionHelper(IHttpContextAccessor context)
        {
            _context = context;
        }

        public  byte[] Compress(byte[] data, bool useGZipCompression)
        {
            CompressionLevel compressionLevel = CompressionLevel.Optimal;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                if (useGZipCompression)
                {
                    using (GZipStream gZipStream = new GZipStream(memoryStream, compressionLevel, true))
                    {
                        gZipStream.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    using (DeflateStream dZipStream = new DeflateStream(memoryStream, compressionLevel, true))
                    {
                        dZipStream.Write(data, 0, data.Length);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public  bool IsCompressionSupported()
        {
            string? AcceptEncoding = _context.HttpContext?.Request.Headers["Accept-Encoding"].ToString();

            return ((!string.IsNullOrEmpty(AcceptEncoding) &&
                    (AcceptEncoding.Contains("gzip") || AcceptEncoding.Contains("deflate"))));
        }
    }
}
