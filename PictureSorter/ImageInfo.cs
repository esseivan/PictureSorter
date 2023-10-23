using ESNLib.Tools;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace PictureSorter
{
    /// <summary>
    /// Contains information about an image
    /// </summary>
    public class ImageInfo
    {
        public static readonly Color colorSelected = Color.LightGreen,
            colorNotSelected = Color.Salmon;

        [JsonIgnore]
        public Image CachedImage { get; set; }

        [JsonIgnore]
        public string FullPath { get; set; }

        [JsonIgnore]
        public int Index { get; set; }
        private bool _isSelected = true;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (null != Node)
                    Node.BackColor = value ? colorSelected : Color.Transparent;
            }
        }

        /// <summary>
        /// The time where the image was taken
        /// </summary>
        public DateTime DateTimeTaken { get; set; } = DateTime.MinValue;

        [JsonIgnore]
        public TreeNode Node { get; set; }

        public Image GetImageAndCache()
        {
            if (null != CachedImage)
                return CachedImage;

            Cache();
            return CachedImage;
        }

        /// <summary>
        /// Read the image from the file
        /// </summary>
        private Image ReadImage()
        {
            Image outputImage = Image.FromFile(FullPath);
            outputImage = ImageTools.FixRotation(outputImage);
            outputImage = ImageTools.applyBorderToImage(
                outputImage,
                IsSelected ? colorSelected : colorNotSelected,
                0.01f // 1 %
            );
            GC.Collect();
            return outputImage;
        }

        public Image ReapplyBorder(Image image)
        {
            image = ImageTools.reapplyBorderToImage(
                image,
                IsSelected ? colorSelected : colorNotSelected,
                0.01f // 1 %
            );
            return image;
        }

        public void ToggleSelection()
        {
            IsSelected = !IsSelected;
        }

        public void Cache()
        {
            if (null != CachedImage)
                return;

            Logger.Instance.Write($"[CacheManager] [Caching]   {FullPath}");
            Console.WriteLine($"'{FullPath}' cached");
            CachedImage = ReadImage();
        }

        public void Decache()
        {
            if (null == CachedImage)
            {
                throw new InvalidOperationException(
                    "Image was not cached. This is a cause of a major error in the programm !"
                );
            }

            Logger.Instance.Write($"[CacheManager] [Decaching] {FullPath}");
            Console.WriteLine($"'{FullPath}' decached");
            CachedImage = null;
        }
    }
}
