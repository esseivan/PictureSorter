using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PictureSorter
{
    public partial class Form1 : Form
    {
        private readonly ImageList imageList;
        private readonly Dictionary<string, ImageInfo> imageInfoCache =
            new Dictionary<string, ImageInfo>();
        ImageInfo selectedImageInfo = null;
        public Control SelectedColorControl = null;

        readonly string[] filters = new string[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" };

        bool previewImage = false;

        string filepath = @"C:\Users\nicol\Pictures\Aletsch 2018";

        //string filepath = @"C:\Users\nicol\Pictures\Screenshots";

        public static readonly Color colorSelected = Color.LightGreen,
            colorNotSelected = Color.Salmon;

        public Form1()
        {
            InitializeComponent();

            SelectedColorControl = panel1;

            imageList = new ImageList { ImageSize = new Size(64, 64) };

            treeView1.ImageList = previewImage ? imageList : null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadImages(filepath);
        }

        private void LoadImages(string directoryPath)
        {
            List<string> imageFiles = new List<string>();
            foreach (string filter in filters)
            {
                imageFiles.AddRange(Directory.GetFiles(directoryPath, filter));
            }

            imageInfoCache.Clear();

            int imageIndex = 0;
            foreach (string imageFullPath in imageFiles)
            {
                // Full path to the image
                string imageFileName = Path.GetFileNameWithoutExtension(imageFullPath);

                // New treeview node
                TreeNode node = new TreeNode
                {
                    ImageIndex = imageIndex,
                    SelectedImageIndex = imageIndex,
                    Text = imageFileName,
                };
                node.Bounds.Inflate(0, 10);

                // Save image info
                ImageInfo imageInfo = new ImageInfo
                {
                    FullPath = imageFullPath,
                    Index = imageIndex,
                    Node = node,
                    IsSelected = true
                };
                // Cache the image info
                imageInfoCache[imageFileName] = imageInfo;

                // Add the image to the imagelist for the treeview
                if (previewImage)
                {
                    imageList.Images.Add(imageInfo.GetThumbnail());
                    GC.Collect(); // Image cannot be disposed, so the GC will collect it
                }
                // Add the treeview node
                treeView1.Nodes.Add(node);
                imageIndex++;
            }

            if (imageInfoCache.Count > 0)
                treeView1.SelectedNode = treeView1.Nodes[0];

            treeView1.Focus();
        }

        /// <summary>
        /// Image selected. Set into the picturebox and save into static variable
        /// </summary>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedFilePath = e.Node.FullPath;
            selectedImageInfo = imageInfoCache[selectedFilePath];
            // Set the picturebox image
            pictureBox1.Image = selectedImageInfo.ReadImage();
            GC.Collect();

            UpdateIsSelectedBackground();
        }

        /// <summary>
        /// Selection button clicked
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            ToggleSelectedImage();
        }

        private void ToggleSelectedImage()
        {
            selectedImageInfo.ToggleSelection();

            UpdateIsSelectedBackground();

            // Update picturebox image (border)
            pictureBox1.Image = selectedImageInfo.ReapplyBorder(pictureBox1.Image);
            GC.Collect();
        }

        public void UpdateIsSelectedBackground()
        {
            if (null == SelectedColorControl)
            {
                return;
            }

            if (null == selectedImageInfo)
            {
                return;
            }

            SelectedColorControl.BackColor = selectedImageInfo.IsSelected
                ? colorSelected
                : colorNotSelected;
        }

        /// <summary>
        /// Contains information about an image
        /// </summary>
        private class ImageInfo
        {
            public string FullPath { get; set; }
            public int Index { get; set; }
            private bool _isSelected = true;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    Node.BackColor = value ? colorSelected : Color.Transparent;
                }
            }

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

        private abstract class ImageTools
        {
            public static Image FixRotation(Image image)
            {
                foreach (var prop in image.PropertyItems)
                {
                    if (prop.Id == 0x0112)
                    {
                        int orientationValue = image.GetPropertyItem(prop.Id).Value[0];
                        RotateFlipType rotateFlipType = getRotateFlipType(orientationValue);
                        image.RotateFlip(rotateFlipType);
                        break;
                    }
                }

                return image;
            }

            //=== determine image rotation
            private static RotateFlipType getRotateFlipType(int rotateValue)
            {
                RotateFlipType flipType = RotateFlipType.RotateNoneFlipNone;

                switch (rotateValue)
                {
                    case 1:
                        flipType = RotateFlipType.RotateNoneFlipNone;
                        break;
                    case 2:
                        flipType = RotateFlipType.RotateNoneFlipX;
                        break;
                    case 3:
                        flipType = RotateFlipType.Rotate180FlipNone;
                        break;
                    case 4:
                        flipType = RotateFlipType.Rotate180FlipX;
                        break;
                    case 5:
                        flipType = RotateFlipType.Rotate90FlipX;
                        break;
                    case 6:
                        flipType = RotateFlipType.Rotate90FlipNone;
                        break;
                    case 7:
                        flipType = RotateFlipType.Rotate270FlipX;
                        break;
                    case 8:
                        flipType = RotateFlipType.Rotate270FlipNone;
                        break;
                    default:
                        flipType = RotateFlipType.RotateNoneFlipNone;
                        break;
                }

                return flipType;
            }

            //=== image border
            public static Image reapplyBorderToImage(Image image, Color borderColor, int borderSize)
            {
                if (borderSize < 0)
                    throw new ArgumentOutOfRangeException(
                        "borderSize",
                        "The border size must be greater or equal to 0"
                    );

                //create a new square image
                Bitmap newImage = new Bitmap(image.Width, image.Height);

                using (Graphics graphics = Graphics.FromImage(newImage))
                {
                    //fill the new square with a color for the border
                    graphics.FillRectangle(
                        new SolidBrush(borderColor),
                        0,
                        0,
                        newImage.Width,
                        newImage.Height
                    );

                    //put the original image on top of the new square
                    Rectangle srcRect = new Rectangle(
                        borderSize,
                        borderSize,
                        image.Width - 2 * borderSize,
                        image.Height - 2 * borderSize
                    );
                    graphics.DrawImage(image, borderSize, borderSize, srcRect, GraphicsUnit.Pixel);
                }

                //return the image
                return newImage;
            }

            //=== image border
            public static Image applyBorderToImage(Image image, Color borderColor, int borderSize)
            {
                if (borderSize < 0)
                    throw new ArgumentOutOfRangeException(
                        "borderSize",
                        "The border size must be greater or equal to 0"
                    );

                //create a new square image
                Bitmap newImage = new Bitmap(
                    image.Width + 2 * borderSize,
                    image.Height + 2 * borderSize
                );

                using (Graphics graphics = Graphics.FromImage(newImage))
                {
                    //fill the new square with a color for the border
                    graphics.FillRectangle(
                        new SolidBrush(borderColor),
                        0,
                        0,
                        newImage.Width,
                        newImage.Height
                    );

                    //put the original image on top of the new square
                    graphics.DrawImage(image, borderSize, borderSize, image.Width, image.Height);
                }

                //return the image
                return newImage;
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine($"treeView1_KeyDown Key pressed {e.KeyCode}");
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine($"Form1_KeyDown Key pressed {e.KeyCode}");
            e.SuppressKeyPress = true;
            switch (e.KeyCode)
            {
                case Keys.A:
                    if (Keys.Control == e.Modifiers)
                        treeView1.SelectedNode = null;
                    break;
                case Keys.Space:
                case Keys.Enter:
                    button1.PerformClick();
                    break;
                default:
                    e.SuppressKeyPress = false;
                    break;
            }
        }
    }
}
