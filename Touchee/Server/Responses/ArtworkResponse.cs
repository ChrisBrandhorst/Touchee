using System;
using System.Drawing;
using System.IO;

namespace Touchee.Server.Responses {

    public class ArtworkResponse : ToucheeResponse {

        public string Url { get; set; }
        public string Data { get; set; }

        public ArtworkResponse(string url, Image image) {
            this.Url = url;
            if (image != null) {
                using (MemoryStream ms = new MemoryStream()) {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();
                    this.Data = Convert.ToBase64String(imageBytes);
                }
            }

        }

    }

}
