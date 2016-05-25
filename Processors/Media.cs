
using ImageProcessor.Web.Processors;

namespace Ullo.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    using ImageProcessor.Imaging;
    using ImageProcessor.Processors;
    using ImageProcessor.Web.Helpers;

    public class Media : IWebGraphicsProcessor
    {

        private static readonly Regex QueryRegex = new Regex(@"(media)=(xs|sm|md|lg|xl)", RegexOptions.Compiled);

        public Media()
        {
            this.Processor = new ImageProcessor.Processors.Resize();
        }

        public Regex RegexPattern
        {
            get
            {
                return QueryRegex;
            }
        }

        public int SortOrder { get; private set; }

        public IGraphicsProcessor Processor { get; private set; }

        public int MatchRegexIndex(string queryString)
        {
            this.SortOrder = int.MaxValue;
            Match match = this.RegexPattern.Match(queryString);

            if (match.Success)
            {
                this.SortOrder = match.Index;
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(queryString);

                Size size = this.ParseSize(queryCollection);

                ResizeLayer resizeLayer = new ResizeLayer(size)
                {
                    ResizeMode = ResizeMode.Max,
                    AnchorPosition = AnchorPosition.Center,
                    Upscale = false,
                    CenterCoordinates = new float[] { }
                };

                this.Processor.DynamicParameter = resizeLayer;

                string restrictions;
                this.Processor.Settings.TryGetValue("RestrictTo", out restrictions);
                ((ImageProcessor.Processors.Resize)this.Processor).RestrictedSizes = this.ParseRestrictions(
                    restrictions);
            }

            return this.SortOrder;
        }

        private Size ParseSize(NameValueCollection queryCollection)
        {
            string media = queryCollection["media"];

            Size size = new Size();

            if (media != null)
            {
                switch (media)
                {
                    case "xs":
                        size = new Size(Settings.MediaXs, 0);
                        break;
                    case "sm":
                        size = new Size(Settings.MediaSm, 0);
                        break;
                    case "md":
                        size = new Size(Settings.MediaMd, 0);
                        break;
                    case "lg":
                        size = new Size(Settings.MediaLg, 0);
                        break;
                    case "xl":
                        size = new Size(Settings.MediaXl, 0);
                        break;
                }
            }

            return size;
        }

        private List<Size> ParseRestrictions(string input)
        {
            List<Size> sizes = new List<Size>();

            if (!string.IsNullOrWhiteSpace(input))
            {
                sizes.AddRange(input.Split(',').Select(q => this.ParseSize(HttpUtility.ParseQueryString(q))));
            }

            return sizes;
        }
    }
}
