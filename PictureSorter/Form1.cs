using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PictureSorter
{
    public partial class Form1 : Form
    {
        private ImageList imageList;
        private Dictionary<string, Image> imageCache = new Dictionary<string, Image>();

        string filepath = @"C:\Users\nicol\Pictures\Screenshots";

        public Form1()
        {
            InitializeComponent();

            imageList = new ImageList();
            imageList.ImageSize = new Size(64, 64);
            treeView1.ImageList = imageList;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadImages();
        }

        private void LoadImages()
        {
            string[] imageFiles = Directory.GetFiles(filepath, "*.png");
            imageCache.Clear();

            int imageIndex = 0;
            foreach (string imageFile in imageFiles)
            {
                string imageFileName = Path.GetFileNameWithoutExtension(imageFile);
                Image image = Image.FromFile(imageFile);
                imageCache[imageFileName] = image;

                imageList.Images.Add(image);

                TreeNode node = new TreeNode();
                node.ImageIndex = imageIndex;
                node.SelectedImageIndex = imageIndex;
                node.Text = imageFileName;

                treeView1.Nodes.Add(node);
                imageIndex++;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedFilePath = e.Node.FullPath;
            pictureBox1.Image = imageCache[selectedFilePath];
        }
    }
}
