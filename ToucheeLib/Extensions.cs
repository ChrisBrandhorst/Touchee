using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

namespace Touchee {

    public static class Extensions {

        public static string StripDiacritics(this string input) {
            string normalized = (input ?? "").Normalize(NormalizationForm.FormKD);
            Encoding removal = Encoding.GetEncoding(Encoding.ASCII.CodePage,
                                                    new EncoderReplacementFallback(""),
                                                    new DecoderReplacementFallback(""));
            byte[] bytes = removal.GetBytes(normalized);
            return Encoding.ASCII.GetString(bytes);
        }

        public static string StripPrefixes(this string input) {
            return Regex.Replace(input, @"^((the|de|een|a|radio)\s|[^\w]*)", "", RegexOptions.IgnoreCase);
        }

        public static bool Matches(this string input, string query) {
            return input.StripDiacritics().ToLower().Contains(query.ToLower());
        }

        public static bool Is(this string input, string query) {
            return input.StripDiacritics().ToLower() == query.StripDiacritics().ToLower();
        }

        public static string ToUnderscore(this string input) {
            return Regex.Replace(
                input,
                "([A-Z])([^A-Z]|$)",
                "_$1$2",
                RegexOptions.Compiled
            ).Trim(' ', '_').ToLower();
        }

        public static string ToCamelCase(this string input, bool firstCapital = true) {
            var camel = Regex.Replace(
                input,
                String.Format("({0})([a-z])", firstCapital ? "^|_" : "_"),
                new MatchEvaluator(ToCamelCaseMatchEvaluator)
            );
            return firstCapital ? camel : FirstToLower(camel);
        }

        public static string FirstToLower(this string input) {
            if (String.IsNullOrEmpty(input)) return input;
            char[] arr = input.ToCharArray();
            arr[0] = char.ToLower(arr[0]);
            return new string(arr);
        }

        public static string FirstToUpper(this string input) {
            if (String.IsNullOrEmpty(input)) return input;
            char[] arr = input.ToCharArray();
            arr[0] = char.ToUpper(arr[0]);
            return new string(arr);
        }

        static string ToCamelCaseMatchEvaluator(Match m) {
            return m.Captures[0].Value.Trim('_').ToUpper();
        }

        public static bool FirstIsAlpha(this string input) {
            return Regex.IsMatch(input, "^[a-z]", RegexOptions.IgnoreCase);
        }

        public static int CompareToCustom(this string one, string other) {
            var oneIsNull = one == null;
            var otherIsNull = other == null;
            int c;

            if (!oneIsNull && !otherIsNull) {
                c = (!Util.FirstIsAlpha(one)).CompareTo(!Util.FirstIsAlpha(other));
                if (c == 0)
                    c = one.CompareTo(other);
            }
            else
                c = oneIsNull ? 1 : -1;

            return c;
        }

        public static string ToTitleCase(this string input) {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        public static bool HasMethod(this object objectToCheck, string methodName) {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName) != null;
        }

        public static double TimeStamp(this DateTime dateTime) {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dateTime.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds;
        }

        public static string ToStringShort(this TimeSpan timeSpan) {
            return timeSpan.ToString(timeSpan.Hours > 0 ? @"%h\:mm\h" : @"%m\:ss");
        }

        /// <summary>
        /// Return unique Int64 value for input string
        /// 
        /// Author: Composition4
        /// Source: http://www.codeproject.com/KB/library/String_To_64bit_Int.aspx
        /// License: COPL
        /// </summary>
        /// <param name="strText">Input string</param>
        /// <returns>64-bits hash output</returns>
        static SHA256CryptoServiceProvider _crypt = new SHA256CryptoServiceProvider();
        public static string GetInt64HashCode(this string strText) {
            Int64 hashCode = 0;
            if (!string.IsNullOrEmpty(strText)) {
                //Unicode Encode Covering all characterset
                byte[] byteContents = Encoding.Unicode.GetBytes(strText);
                byte[] hashText;
                lock(_crypt)
                    hashText = _crypt.ComputeHash(byteContents);
                //32Byte hashText separate
                //hashCodeStart = 0~7  8Byte
                //hashCodeMedium = 8~23  8Byte
                //hashCodeEnd = 24~31  8Byte
                //and Fold
                Int64 hashCodeStart = BitConverter.ToInt64(hashText, 0);
                Int64 hashCodeMedium = BitConverter.ToInt64(hashText, 8);
                Int64 hashCodeEnd = BitConverter.ToInt64(hashText, 24);
                hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            }
            return hashCode.ToString("x16");
        }



        /// <summary>
        /// Checks if the given image is valid (i.e. can be written to a MemoryStream)
        /// </summary>
        /// <param name="image">The image to check</param>
        /// <returns>Boolean</returns>
        public static bool IsValid(this Image image) {
            using (var stream = new MemoryStream()) {
                try {
                    image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (Exception) {
                    return false;
                }
            }
            return true;
        }


        public static Image Resize(this Image src, Size bounds, ResizeMode resizeMode = ResizeMode.Stretch) {

            int srcX, srcY, srcWidth, srcHeight;
            int targetX, targetY, targetWidth, targetHeight;

            var widthRatio = ((float)bounds.Width / (float)src.Width);
            var heightRatio = ((float)bounds.Height / (float)src.Height);
            float ratio;

            switch (resizeMode) {

                case ResizeMode.Contain:
                case ResizeMode.ContainAndShrink:
                case ResizeMode.ContainAndFill:
                    srcX = srcY = 0;
                    srcWidth = src.Width;
                    srcHeight = src.Height;
                    ratio = heightRatio < widthRatio ? heightRatio : widthRatio;
                    targetWidth = (int)Math.Round(srcWidth * ratio);
                    targetHeight = (int)Math.Round(srcHeight * ratio);
                    if (resizeMode == ResizeMode.ContainAndShrink)
                        bounds = new Size(targetWidth, targetHeight);
                    targetX = (bounds.Width - targetWidth) / 2;
                    targetY = (bounds.Height - targetHeight) / 2;
                    break;

                case ResizeMode.Cover:
                    targetX = targetY = 0;
                    targetWidth = bounds.Width;
                    targetHeight = bounds.Height;
                    ratio = heightRatio > widthRatio ? heightRatio : widthRatio;
                    srcWidth = (int)Math.Round(targetWidth / ratio);
                    srcHeight = (int)Math.Round(targetHeight / ratio);
                    srcX = (src.Width - srcWidth) / 2;
                    srcY = (src.Height - srcHeight) / 2;
                    break;

                case ResizeMode.Stretch:
                case ResizeMode.CoverAndGrow:
                default:
                    srcX = targetX = srcY = targetY = 0;
                    srcWidth = src.Width;
                    srcHeight = src.Height;
                    if (resizeMode == ResizeMode.CoverAndGrow) {
                        ratio = heightRatio > widthRatio ? heightRatio : widthRatio;
                        bounds = new Size((int)Math.Round(src.Width * ratio), (int)Math.Round(src.Height * ratio));
                    }
                    targetWidth = bounds.Width;
                    targetHeight = bounds.Height;
                    break;
            }


            var b = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
            var g = Graphics.FromImage(b as Image);
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.InterpolationMode = InterpolationMode.Low;
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Fill background if requested
            if (resizeMode == ResizeMode.ContainAndFill) {
                var pixel = ((Bitmap)src).GetPixel(0, 0);
                if (pixel.A < byte.MaxValue)
                    pixel = Color.White;
                g.FillRectangle(new SolidBrush(pixel), 0, 0, bounds.Width, bounds.Height);
            }

            g.DrawImage(src,
                new Rectangle(targetX, targetY, targetWidth, targetHeight),
                new Rectangle(srcX, srcY, srcWidth, srcHeight),
                GraphicsUnit.Pixel
            );
            g.Dispose();

            return (Image)b;
        }



        public static string ToBase64(this Image src) {
            using (MemoryStream ms = new MemoryStream()) {
                // Convert Image to byte[]
                src.Save(ms, src.RawFormat);
                byte[] imageBytes = ms.ToArray();
                // Convert byte[] to Base64 String
                return Convert.ToBase64String(imageBytes); ;
            }
        }

        /// <summary>
        /// Gets an array containing the path segments that make up the specified URI. The returned path segments do not contain any slashes.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string[] GetSegments(this Uri uri, bool withSlashes = false) {
            return withSlashes ? uri.Segments : uri.Segments.Where(s => s != "/").Select(s => s.TrimEnd(new char[] { '/' })).ToArray();
        }


        /// <summary>
        /// Shuffles a list
        /// </summary>
        public static void Shuffle<T>(this IList<T> list) {
            var rng = new Random();
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


    }

}


namespace System.Drawing {

    /// <summary>
    /// Resize modes
    /// </summary>
    public enum ResizeMode {
                /// <summary>
        /// Scale the image as small as possible while ensuring both the dimensions are greater than or equal to the corresponding dimensions of the given bounds.
        /// The resulting image size is equal to the given bounds.
        /// </summary>
        Cover,
        /// <summary>
        /// Scale the image as small as possible while ensuring both the dimensions are greater than or equal to the corresponding dimensions of the given bounds.
        /// The resulting image size is equal to the scaled image, which may be larger in one single dimension than the given bounds.
        /// </summary>
        CoverAndGrow,
        /// <summary>
        /// Scales the image as large as possible while ensuring both the dimensions are less than or equal to the corresponding dimensions of the given bounds.
        /// The resulting image size is equal to the given bounds.
        /// </summary>
        Contain,
        /// <summary>
        /// Scales the image as large as possible while ensuring both the dimensions are less than or equal to the corresponding dimensions of the given bounds.
        /// The resulting image size is equal to the scaled image, which may be smaller in one single dimension than the given bounds.
        /// </summary>
        ContainAndShrink,
        /// <summary>
        /// Scales the image as large as possible while ensuring both the dimensions are less than or equal to the corresponding dimensions of the given bounds.
        /// The resulting image size is equal to the given bounds, whilst filling the resulting gaps with the color of the top left pixel.
        /// </summary>
        ContainAndFill,
        /// <summary>
        /// Stretches the original image to the given bounds.
        /// </summary>
        Stretch
    }

}