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
        public static readonly Color colorSelected = Color.LightGreen,
            colorNotSelected = Color.Salmon;

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

        public Image ReadImage()
        {
            Image outputImage = Image.FromFile(FullPath);
            outputImage = ImageTools.FixRotation(outputImage);
            outputImage = ImageTools.applyBorderToImage(
                outputImage,
                IsSelected ? colorSelected : colorNotSelected,
                40
            );
            return outputImage;
        }

        public Image ReapplyBorder(Image image)
        {
            image = ImageTools.reapplyBorderToImage(
                image,
                IsSelected ? colorSelected : colorNotSelected,
                40
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
    }
}
