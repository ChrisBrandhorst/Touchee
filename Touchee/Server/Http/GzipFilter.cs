using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

using Nancy;

namespace Touchee.Server.Http {


    /// <summary>
    /// A JSON gzip compression filter, which could easily be adapted to any pattern needed.  This uses a custom AfterFilter 
    /// type which is just a fancy wrapper of Action<NancyContext>.  It's useful for convention based loading of filters
    /// </summary>
    public class GzipCompressionFilter : AfterFilter {

        protected override void Handle(NancyContext context) {
            //if ((ctx.Response.ContentType == "application/json") && ctx.Request.Headers.AcceptEncoding.Any(x => x.Contains("gzip"))) {
            //    var jsonData = new MemoryStream();
            //    ctx.Response.Contents.Invoke(jsonData);
            //    jsonData.Position = 0;
            //    if (jsonData.Length < 4096) {
            //        ctx.Response.Contents = s => {
            //            jsonData.CopyTo(s);
            //            s.Flush();
            //        };
            //    }
            //    else {
            //        ctx.Response.Headers["Content-Encoding"] = "gzip";
            //        ctx.Response.Contents = s => {
            //            var gzip = new GZipStream(s, CompressionMode.Compress, true);
            //            jsonData.CopyTo(gzip);
            //            gzip.Close();
            //        };
            //    }
            //}

            if (!RequestIsGzipCompatible(context.Request)) {
                return;
            }

            if (context.Response.StatusCode != HttpStatusCode.OK) {
                return;
            }

            if (!ResponseIsCompatibleMimeType(context.Response)) {
                return;
            }

            if (ContentLengthIsTooSmall(context.Response)) {
                return;
            }

            CompressResponse(context.Response);
        }

        static void CompressResponse(Response response) {
            response.Headers["Content-Encoding"] = "gzip";

            var contents = response.Contents;
            response.Contents = responseStream => {
                using (var compression = new GZipStream(responseStream, CompressionMode.Compress)) {
                    contents(compression);
                }
            };
        }

        static bool ContentLengthIsTooSmall(Response response) {
            string contentLength;
            if (response.Headers.TryGetValue("Content-Length", out contentLength)) {
                var length = long.Parse(contentLength);
                if (length < 4096) {
                    return true;
                }
            }
            return false;
        }

        public static List<string> ValidMimes = new List<string> {
            "text/css",
            "text/html",
            "text/plain",
            "application/xml",
            "application/json",
            "application/xaml+xml",
            "application/x-javascript"
        };

        static bool ResponseIsCompatibleMimeType(Response response) {
            return ValidMimes.Any(x => response.ContentType.StartsWith(x));
        }

        static bool RequestIsGzipCompatible(Request request) {
            return request.Headers.AcceptEncoding.Any(x => x.Contains("gzip"));
        }


    }

    public abstract class AfterFilter {
        public static implicit operator Action<NancyContext>(AfterFilter filter) {
            return filter.Handle;
        }
        protected abstract void Handle(NancyContext ctx);
    }

}
