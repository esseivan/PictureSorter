﻿using System;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace PictureSorter
{
    /// <summary>
    /// Contains information about an image
    /// </summary>
    public class ImageInfo
    {
        private static CacheManager cacheManager = new CacheManager();

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

        public Image GetThumbnail()
        {
            Image image = ReadImage();
            Image thumb = image.GetThumbnailImage(64, 64, () => false, IntPtr.Zero);
            return thumb;
        }

        public void ToggleSelection()
        {
            IsSelected = !IsSelected;
        }

        public void Cache()
        {
            if (null != CachedImage)
                return;

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

            Console.WriteLine($"'{FullPath}' decached");
            CachedImage = null;
        }
    }
}
