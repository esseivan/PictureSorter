using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PictureSorter
{
    public partial class Form1 : Form
    {
        private readonly ImageList imageList;
        private readonly Dictionary<string, ImageInfo> imageCache =
            new Dictionary<string, ImageInfo>();
        ImageInfo selectedImageInfo = null;

        readonly string[] filters = new string[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" };

        //string filepath = @"C:\Users\nicol\Pictures\Aletsch 2022 2";

        string filepath = @"C:\Users\nicol\Pictures\Screenshots";

        public Form1()
        {
            InitializeComponent();

            imageList = new ImageList { ImageSize = new Size(64, 64) };
            treeView1.ImageList = imageList;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadImages();
        }

        private void LoadImages()
        {
            List<string> imageFiles = new List<string>();
            foreach (string filter in filters)
            {
                imageFiles.AddRange(Directory.GetFiles(filepath, filter));
            }

            imageCache.Clear();

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
                imageCache[imageFileName] = imageInfo;

                // Add the image to the imagelist for the treeview
                imageList.Images.Add(imageInfo.GetThumbnail());
                GC.Collect(); // Image cannot be disposed, so the GC will collect it

                // Add the treeview node
                treeView1.Nodes.Add(node);
                imageIndex++;
            }
        }

        /// <summary>
        /// Image selected. Set into the picturebox and save into static variable
        /// </summary>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedFilePath = e.Node.FullPath;
            selectedImageInfo = imageCache[selectedFilePath];
            // Set the picturebox image
            pictureBox1.Image = selectedImageInfo.ReadImage();
        }

        /// <summary>
        /// Selection button clicked
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            selectedImageInfo.ToggleSelection();
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
                    Node.BackColor = value ? Color.LightGreen : Color.Transparent;
                }
            }

            public TreeNode Node { get; set; }

            public Image ReadImage()
            {
                return Image.FromFile(FullPath);
            }

            public Image GetThumbnail()
            {
                Image image = Image.FromFile(FullPath);
                Image thumb = ImageTools.resizeImage(image, 64, 64, true);
                return thumb;
            }

            public void ToggleSelection()
            {
                IsSelected = !IsSelected;
            }
        }

        private abstract class ImageTools
        {
            //set the resolution, 72 is usually good enough for displaying images on monitors
            static float imageResolution = 72;

            //set the compression level. higher compression = better quality = bigger images
            static long compressionLevel = 80L;

            static int margin = 4;

            static Color borderColor = Color.Black;

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

            public static Image resizeImage(Image image, int maxWidth, int maxHeight, bool padImage)
            {
                int newWidth;
                int newHeight;

                //first we check if the image needs rotating (eg phone held vertical when taking a picture for example)
                image = FixRotation(image);

                //apply the padding to make a square image
                if (padImage == true)
                {
                    image = applyPaddingToImage(image, Color.Transparent);
                }

                //check if the with or height of the image exceeds the maximum specified, if so calculate the new dimensions
                int marginMaxWidth = maxWidth - 2 * margin;
                int marginMaxHeight = maxHeight - 2 * margin;
                if (image.Width > marginMaxWidth || image.Height > marginMaxHeight)
                {
                    double ratioX = (double)marginMaxWidth / image.Width;
                    double ratioY = (double)marginMaxHeight / image.Height;
                    double ratio = Math.Min(ratioX, ratioY);

                    newWidth = (int)(image.Width * ratio);
                    newHeight = (int)(image.Height * ratio);
                }
                else
                {
                    newWidth = image.Width;
                    newHeight = image.Height;
                }

                //start the resize with a new image
                Bitmap newImage = new Bitmap(newWidth, newHeight);

                //set the new resolution
                newImage.SetResolution(imageResolution, imageResolution);

                //start the resizing
                using (var graphics = Graphics.FromImage(newImage))
                {
                    //set some encoding specs
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    //Pen penBlack = new Pen(Color.Black, margin);
                    //graphics.DrawRectangle(penBlack, 0, 0, maxWidth - margin, maxHeight - margin);
                    graphics.DrawImage(image, margin, margin, newWidth, newHeight);
                }

                //save the image to a memorystream to apply the compression level
                using (MemoryStream ms = new MemoryStream())
                {
                    EncoderParameters encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(
                        System.Drawing.Imaging.Encoder.Quality,
                        compressionLevel
                    );

                    newImage.Save(ms, getEncoderInfo("image/jpeg"), encoderParameters);

                    //save the image as byte array here if you want the return type to be a Byte Array instead of Image
                    //byte[] imageAsByteArray = ms.ToArray();
                }

                //return the image
                return newImage;
            }

            //=== image padding
            public static Image applyPaddingToImage(Image image, Color backColor)
            {
                //get the maximum size of the image dimensions
                int maxSize = Math.Max(image.Height, image.Width);
                Size squareSize = new Size(maxSize, maxSize);
                Size borderSize = new Size(
                    Math.Min(maxSize, image.Width),
                    Math.Min(maxSize, image.Height)
                );

                //create a new square image
                Bitmap squareImage = new Bitmap(squareSize.Width, squareSize.Height);

                using (Graphics graphics = Graphics.FromImage(squareImage))
                {
                    //fill the new square with a color
                    graphics.FillRectangle(
                        new SolidBrush(backColor),
                        0,
                        0,
                        squareSize.Width,
                        squareSize.Height
                    );

                    //put the original image on top of the new square
                    graphics.DrawImage(
                        image,
                        (squareSize.Width / 2) - (image.Width / 2),
                        (squareSize.Height / 2) - (image.Height / 2),
                        image.Width,
                        image.Height
                    );

                    // Make the border
                    graphics.DrawRectangle(
                        new Pen(borderColor, margin),
                        (squareSize.Width / 2) - (borderSize.Width / 2),
                        (squareSize.Height / 2) - (borderSize.Height / 2),
                        borderSize.Width,
                        borderSize.Height
                    );
                }

                //return the image
                return squareImage;
            }

            //=== get encoder info
            private static ImageCodecInfo getEncoderInfo(string mimeType)
            {
                ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

                for (int j = 0; j < encoders.Length; ++j)
                {
                    if (encoders[j].MimeType.ToLower() == mimeType.ToLower())
                    {
                        return encoders[j];
                    }
                }

                return null;
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

            //== convert image to base64
            public static string convertImageToBase64(Image image)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    //convert the image to byte array
                    image.Save(ms, ImageFormat.Jpeg);
                    byte[] bin = ms.ToArray();

                    //convert byte array to base64 string
                    return Convert.ToBase64String(bin);
                }
            }
        }
    }
}
