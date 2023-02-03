using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PictureSorter
{
    public partial class Form1 : Form
    {
        private List<Image> images;

        public Form1()
        {
            InitializeComponent();

            images = LoadImagesFromFolder("C:\\Users\\nicol\\Pictures\\Screenshots");
            PopulateListView(images);
        }

        private void PopulateListView(List<Image> images)
        {
            listView1.View = View.LargeIcon;
            listView1.LargeImageList = new ImageList();
            //listView1.LargeImageList.ImageSize = new Size(64, 64);
            listView1.TileSize = new Size(64, 64);

            int imageIndex = 0;
            foreach (Image image in images)
            {
                listView1.LargeImageList.Images.Add(image);
                ListViewItem item = new ListViewItem();
                item.ImageIndex = imageIndex;
                item.Text = "Image " + (imageIndex + 1);

                listView1.Items.Add(item);
                imageIndex++;
            }

            listView1.OwnerDraw = false;
            //listView1.DrawItem += ListView1_DrawItem;
        }

        private void ListView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.Graphics.InterpolationMode = System
                .Drawing
                .Drawing2D
                .InterpolationMode
                .HighQualityBilinear;
            e.Graphics.DrawImage(listView1.LargeImageList.Images[e.Item.ImageIndex], e.Bounds);
        }

        static List<Image> LoadImagesFromFolder(string folderPath)
        {
            List<Image> images = new List<Image>();
            string[] filePaths = Directory.GetFiles(folderPath, "*.*");
            foreach (string filePath in filePaths)
            {
                try
                {
                    images.Add(Image.FromFile(filePath));
                }
                catch (OutOfMemoryException)
                {
                    // Some file types, such as non-image files, may cause exceptions to be thrown.
                    // You can handle these exceptions and continue processing other files if desired.
                }
            }
            return images;
        }
    }
}
