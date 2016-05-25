using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Ullo.Models
{
    public class Picture {
        public enum assetTypeEnum
        {
            Unknown = 0,
            Picture = 1,
        };
        [Key]
        [ScaffoldColumn(false)]
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        [StringLength(1024)]
        public string Route { get; set; }
        public DateTime Created { get; set; }
        public assetTypeEnum AssetType { get; set; }
        public virtual string Key
        {
            get
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string camelCase = Regex.Replace(textInfo.ToTitleCase(this.Name), @"\s+", "");
                return camelCase.First().ToString().ToLower() + String.Join("", camelCase.Skip(1));
            }
        }
        public static Picture getPictureWithInfos(string originalFilename, string mimeType, FileInfo fileInfo)
        {
            Picture picture = new Picture();
            picture.Name = Path.GetFileNameWithoutExtension(originalFilename);
            picture.Created = fileInfo.CreationTimeUtc;
            picture.Route = Path.GetFileName(fileInfo.FullName); //unreachable file
            picture.Guid = new Guid(picture.Route.Replace("BodyPart_", ""));
            string[] imageFormats = new string[] { ".jpg", ".png", ".jpeg" };
            if (imageFormats.Any(item => originalFilename.ToLower().EndsWith(item, StringComparison.OrdinalIgnoreCase)))
            {
                if (mimeType.Contains("image"))
                {
                    picture.AssetType = assetTypeEnum.Picture;
                }
            }            
            return picture;
        }
        public static Picture getPictureFromBase64(string pictureBase64)
        {
            Picture picture = new Picture();
            picture.Guid = Guid.NewGuid();
            picture.Name = picture.Guid.ToString();
            picture.Created = DateTime.Now;
            picture.Route = String.Format("{0}{1}.jpg", Settings.UploadFolder, picture.Name);
            picture.AssetType = assetTypeEnum.Picture;
            try
            {
                byte[] imageBytes = Convert.FromBase64String(pictureBase64.Replace("data:image/jpeg;base64,", ""));
                using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    Image image = Image.FromStream(ms, true);
                    image = DoCrop(image);
                    string savePath = HttpContext.Current.Server.MapPath(picture.Route);
                    ImageCodecInfo codecInfo = GetEncoderInfo("image/jpeg");
                    // Create an Encoder object based on the GUID
                    Encoder encoder = Encoder.Quality;
                    // Create an EncoderParameters object.
                    EncoderParameters parameters = new EncoderParameters(1);
                    // Save the bitmap as a JPEG file with quality level 90.
                    EncoderParameter parameter = new EncoderParameter(encoder, 90L);
                    parameters.Param[0] = parameter;
                    image.Save(savePath, codecInfo, parameters);
                }                
            } catch 
            {
                picture = null;
            }
            return picture;
        }
        public static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        public static Image DoCrop(Image source)
        {
            Bitmap newImage = null;
            Image image = source;
            try
            {
                int iw = image.Width;
                int ih = image.Height;
                double ir = iw / (double)ih;

                double tw = 750;
                double th = 375;
                double tr = tw / th;

                double sw, sh, sx, sy;
                if (ir > tr)
                {
                    sh = ih;
                    sw = ih * tr;
                } else
                {
                    sw = iw;
                    sh = iw / tr;
                }

                sx = (iw - sw) / 2;
                sy = (ih - sh) / 2;

                RectangleF sourceRectF = new RectangleF(Convert.ToSingle(sx), Convert.ToSingle(sy), Convert.ToSingle(sw), Convert.ToSingle(sh));
                Rectangle sourceRect = Rectangle.Round(sourceRectF);

                int width = (int)Math.Min(sw, tw);
                int height = (int)(Math.Min(sw, tw) / tr);

                newImage = new Bitmap(width, height);
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
                            new Rectangle(0, 0, width, height),
                            sourceRect,
                            GraphicsUnit.Pixel);
                    }
                }
                image.Dispose();
                image = newImage;

            } catch
            {
                if (newImage != null)
                {
                    newImage.Dispose();
                }
            }

            return image;
        }

        public static Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }
        public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
    }
}