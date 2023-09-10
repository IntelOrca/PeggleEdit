using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace IntelOrca.PeggleEdit.Tools.Pack
{
    /// <summary>
    /// An image stored in the .pak file.
    /// Keep the original buffer unless replaced with a new image.
    /// But allow conversion to .png for editor preview.
    /// </summary>
    public sealed class PakImage
    {
        private Image _image;
        private bool _imageLoaded;

        public string FileName { get; private set; }
        public byte[] Data { get; }

        public PakImage(string path)
            : this(Path.GetFileName(path), File.ReadAllBytes(path))
        {
        }

        public PakImage(string fileName, Image image)
        {
            FileName = fileName;
            Data = GetImageData(fileName, image);
            _image = image;
        }

        public PakImage(string fileName, byte[] data)
        {
            FileName = fileName;
            Data = data;
        }

        public Image Image
        {
            get
            {
                if (!_imageLoaded)
                {
                    try
                    {
                        if (FileName.EndsWith(".jp2", System.StringComparison.OrdinalIgnoreCase))
                        {
                            _image = J2K.ConvertJPEG2(Data);
                        }
                        else
                        {
                            _image = Image.FromStream(new MemoryStream(Data));
                        }
                    }
                    catch
                    {
                    }
                    _imageLoaded = true;
                }
                return _image;
            }
        }

        private static byte[] GetImageData(string fileName, Image image)
        {
            ImageFormat format;
            if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                format = ImageFormat.Png;
            }
            else if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            {
                format = ImageFormat.Jpeg;
            }
            else
            {
                throw new ArgumentException($"{nameof(fileName)} must be a supported extension.", nameof(fileName));
            }

            var ms = new MemoryStream();
            image.Save(ms, format);
            return ms.ToArray();
        }
    }
}
