
using ImageProcessor;
using ImageProcessor.Common.Exceptions;
using ImageProcessor.Imaging;
using ImageProcessor.Processors;
using ImageProcessor.Web.Helpers;
using ImageProcessor.Web.Processors;
using ImageProcessor.Web.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Web;

namespace Ullo.Processors
{
    /// <summary>
    /// Crops an image to the given directions.
    /// </summary>
    public class Cropper : IWebGraphicsProcessor
    {
        /// <summary>
        /// The regular expression to search strings for.
        /// </summary>
        private static readonly Regex QueryRegex = new Regex(@"\bcropper\b[=]", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="Cropper"/> class.
        /// </summary>
        public Cropper()
        {
            this.Processor = new CropperProcessor();
        }

        /// <summary>
        /// Gets the regular expression to search strings for.
        /// </summary>
        public Regex RegexPattern
        {
            get
            {
                return QueryRegex;
            }
        }

        /// <summary>
        /// Gets the order in which this processor is to be used in a chain.
        /// </summary>
        public int SortOrder { get; private set; }

        /// <summary>
        /// Gets the associated graphics processor.
        /// </summary>
        public IGraphicsProcessor Processor { get; private set; }

        /// <summary>
        /// The position in the original string where the first character of the captured substring was found.
        /// </summary>
        /// <param name="queryString">
        /// The query string to search.
        /// </param>
        /// <returns>
        /// The zero-based starting position in the original string where the captured substring was found.
        /// </returns>
        public int MatchRegexIndex(string queryString)
        {
            this.SortOrder = int.MaxValue;
            Match match = this.RegexPattern.Match(queryString);

            if (match.Success)
            {
                this.SortOrder = match.Index;
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(queryString);
                float[] coordinates = QueryParamParser.Instance.ParseValue<float[]>(queryCollection["cropper"]);

                // Default CropMode.Pixels will be returned.
                CropMode cropMode = QueryParamParser.Instance.ParseValue<CropMode>(queryCollection["cropmode"]);
                CropLayer cropLayer = new CropLayer(coordinates[0], coordinates[1], coordinates[2], coordinates[3], cropMode);
                this.Processor.DynamicParameter = cropLayer;
            }

            return this.SortOrder;
        }

    }

    public class CropperProcessor : IGraphicsProcessor
    {
        public CropperProcessor()
        {
            this.Settings = new Dictionary<string, string>();
        }
        public dynamic DynamicParameter
        {
            get;
            set;
        }
        public Dictionary<string, string> Settings
        {
            get;
            set;
        }
        public Image ProcessImage(ImageFactory factory)
        {
            Bitmap newImage = null;
            Image image = factory.Image;
            try
            {
                CropLayer cropLayer = this.DynamicParameter;

                int iw = image.Width;
                int ih = image.Height;
                double ir = iw / (double)ih;

                double tw = cropLayer.Right;
                double th = cropLayer.Right / cropLayer.Bottom;
                double tr = tw / th;

                double sw, sh, sx, sy;
                if (ir > tr)
                {
                    sh = ih;
                    sw = ih * tr;
                }
                else
                {
                    sw = iw;
                    sh = iw / tr;
                }

                sx = (iw - sw) / 2 + iw * (cropLayer.Left - 0.5);
                sy = (ih - sh) / 2 + ih * (cropLayer.Top - 0.5);
                sx = Math.Max(0, sx);
                sy = Math.Max(0, sy);
                sx = Math.Min(iw - sw, sx);
                sy = Math.Min(ih - sh, sy);

                RectangleF rectangleF = new RectangleF(Convert.ToSingle(sx), Convert.ToSingle(sy), Convert.ToSingle(sw), Convert.ToSingle(sh));
                Rectangle rectangle = Rectangle.Round(rectangleF);

                if (rectangle.X < iw && rectangle.Y < ih)
                {
                    if (rectangle.Width > (iw - rectangle.X))
                    {
                        rectangle.Width = iw - rectangle.X;
                    }

                    if (rectangle.Height > (ih - rectangle.Y))
                    {
                        rectangle.Height = ih - rectangle.Y;
                    }

                    newImage = new Bitmap(rectangle.Width, rectangle.Height);
                    newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                    using (Graphics graphics = Graphics.FromImage(newImage))
                    {
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;

                        // An unwanted border appears when using InterpolationMode.HighQualityBicubic to resize the image
                        // as the algorithm appears to be pulling averaging detail from surrounding pixels beyond the edge 
                        // of the image. Using the ImageAttributes class to specify that the pixels beyond are simply mirror 
                        // images of the pixels within solves this problem.
                        using (ImageAttributes wrapMode = new ImageAttributes())
                        {
                            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                            graphics.DrawImage(
                                image,
                                new Rectangle(0, 0, (int)tw, (int)(tw / tr)),
                                rectangle.X,
                                rectangle.Y,
                                rectangle.Width,
                                rectangle.Height,
                                GraphicsUnit.Pixel,
                                wrapMode);
                        }
                    }

                    // Reassign the image.
                    image.Dispose();
                    image = newImage;
                }
            }
            catch (Exception ex)
            {
                if (newImage != null)
                {
                    newImage.Dispose();
                }

                throw new ImageProcessingException("Error processing image with " + this.GetType().Name, ex);
            }

            return image;
        }

    }
}

