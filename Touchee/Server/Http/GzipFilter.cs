using System;
using System.Linq;
using System.IO;
using System.IO.Compression;

using Nancy;

namespace Touchee.Server.Http {


    /// <summary>
    /// A JSON gzip compression filter, which could easily be adapted to any pattern needed.  This uses a custom AfterFilter 
    /// type which is just a fancy wrapper of Action<NancyContext>.  It's useful for convention based loading of filters
    /// </summary>
    public class GzipCompressionFilter : AfterFilter {
        protected override void Handle(NancyContext ctx) {
            if ((ctx.Response.ContentType == "application/json") && ctx.Request.Headers.AcceptEncoding.Any(
                x => x.Contains("gzip"))) {
                var jsonData = new MemoryStream();
                ctx.Response.Contents.Invoke(jsonData);
                jsonData.Position = 0;
                if (jsonData.Length < 4096) {
                    ctx.Response.Contents = s => {
                        jsonData.CopyTo(s);
                        s.Flush();
                    };
                }
                else {
                    ctx.Response.Headers["Content-Encoding"] = "gzip";
                    ctx.Response.Contents = s => {
                        var gzip = new GZipStream(s, CompressionMode.Compress, true);
                        jsonData.CopyTo(gzip);
                        gzip.Close();
                    };
                }
            }
        }
    }

    public abstract class AfterFilter {
        public static implicit operator Action<NancyContext>(AfterFilter filter) {
            return filter.Handle;
        }

        protected abstract void Handle(NancyContext ctx);
    }

}
