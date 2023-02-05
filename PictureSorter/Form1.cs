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

        bool previewImage = false;

        string filepath = @"C:\Users\nicol\Pictures\Aletsch 2022 2";

        //string filepath = @"C:\Users\nicol\Pictures\Screenshots";

        public Form1()
        {
            InitializeComponent();

            imageList = new ImageList { ImageSize = new Size(64, 64) };

            treeView1.ImageList = previewImage ? imageList : null;
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
                if (previewImage)
                {
                    imageList.Images.Add(imageInfo.GetThumbnail());
                    GC.Collect(); // Image cannot be disposed, so the GC will collect it
                }
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
            private static int cacheSize = 10;
            private static Queue<ImageInfo> imagesCached = new Queue<ImageInfo>(cacheSize);

            public Image CachedImage { get; set; }
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
                if (null == CachedImage)
                {
                    CachedImage = Image.FromFile(FullPath);
                    AddCache();
                }

                return CachedImage;
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

            public void AddCache()
            {
                // Decache image
                if (imagesCached.Count >= cacheSize)
                {
                    ImageInfo imageInfo = imagesCached.Dequeue();
                    imageInfo.CachedImage = null;
                    GC.Collect();
                    Console.WriteLine("Dequeued. Count is " + imagesCached.Count);
                }

                // cache image
                imagesCached.Enqueue(this);
                Console.WriteLine("Enqueued. Count is " + imagesCached.Count);
            }
        }
    }
}
