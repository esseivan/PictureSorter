using ESNLib.Tools;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Timers;
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

        private string saveFileName = "pictureSelector.pssave";
        public string SelectedFolder = string.Empty;

        // 1min auto save timer
        //public System.Timers.Timer tmrAutoUpdate = new Timer(1000 * 60 * 1);

        public Form1()
        {
            InitializeComponent();

            //tmrAutoUpdate.AutoReset = true;
            //tmrAutoUpdate.Elapsed += TmrAutoUpdate_Elapsed;
            //tmrAutoUpdate.Start();

            SelectedColorControl = panel1;

            imageList = new ImageList { ImageSize = new Size(64, 64) };

            treeView1.ImageList = previewImage ? imageList : null;
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void ChooseFolder()
        {
            var dialog = new CommonOpenFileDialog("Sélectionner le dossier contenant les images")
            {
                IsFolderPicker = true
            };
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                LoadImages(dialog.FileName);
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void SaveToFile()
        {
            if (!Directory.Exists(SelectedFolder))
                return;

            if (imageInfoCache.Count == 0)
                return;

            string savePath = Path.Combine(SelectedFolder, saveFileName);
            if (File.Exists(savePath))
                File.SetAttributes(savePath, FileAttributes.Normal);

            SettingsManager.SaveTo(savePath, imageInfoCache, false, true);

            File.SetAttributes(savePath, FileAttributes.Hidden);
        }

        /// <summary>
        ///
        /// </summary>
        private void UpdateFromSave()
        {
            string savePath = Path.Combine(SelectedFolder, saveFileName);

            if (!File.Exists(savePath))
                return;

            if (!SettingsManager.LoadFrom(savePath, out Dictionary<string, ImageInfo> loadedData))
                return;

            // Apply only the IsSelected property
            foreach (var dataItem in loadedData)
            {
                if (imageInfoCache.ContainsKey(dataItem.Key))
                {
                    imageInfoCache[dataItem.Key].IsSelected = dataItem.Value.IsSelected;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TmrAutoUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            SaveToFile();
        }

        #region Image management

        /// <summary>
        /// Load images in the specified directory
        /// </summary>
        /// <param name="directoryPath"></param>
        private void LoadImages(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            // Restart auto save timer
            //tmrAutoUpdate.Stop();
            //tmrAutoUpdate.Start();

            SelectedFolder = directoryPath;

            imageInfoCache.Clear();

            List<string> imageFiles = new List<string>();
            foreach (string filter in filters)
            {
                imageFiles.AddRange(Directory.GetFiles(directoryPath, filter));
            }

            int imageIndex = 0;
            foreach (string imageFullPath in imageFiles)
            {
                // Full path to the image
                string imageFileName = Path.GetFileName(imageFullPath);

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

            UpdateFromSave();

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

        /// <summary>
        ///
        /// </summary>
        private void ToggleSelectedImage()
        {
            selectedImageInfo.ToggleSelection();

            UpdateIsSelectedBackground();

            // Update picturebox image (border)
            pictureBox1.Image = selectedImageInfo.ReapplyBorder(pictureBox1.Image);
            GC.Collect();
        }

        /// <summary>
        ///
        /// </summary>
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
                ? ImageInfo.colorSelected
                : ImageInfo.colorNotSelected;
        }

        /// <summary>
        ///
        /// </summary>
        private void sauvegarderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveToFile();
        }

        private void ouvrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            switch (e.KeyCode)
            {
                case Keys.Space:
                case Keys.Enter:
                    button1.PerformClick();
                    break;
                default:
                    e.SuppressKeyPress = false;
                    break;
            }
        }

        #endregion

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
