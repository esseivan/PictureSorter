using ESNLib.Tools;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Size = System.Drawing.Size;

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

        private const bool previewImage = false;

        private const string saveFileName = "pictureSelector.pssave";
        public string SelectedFolder = string.Empty;

        public Form1()
        {
            InitializeComponent();

            SelectedColorControl = panel1;

            imageList = new ImageList { ImageSize = new Size(64, 64) };

            treeView1.ImageList = previewImage ? imageList : null;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            ChooseFile();
        }

        private void ChooseFile()
        {
            var dialog = new CommonOpenFileDialog("Sélectionner une image")
            {
                IsFolderPicker = false
            };
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                string folder = Path.GetDirectoryName(dialog.FileName);

                LoadImages(folder);
            }
        }

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
        private void SaveToFile(bool backup = false)
        {
            if (!Directory.Exists(SelectedFolder))
                return;

            if (imageInfoCache.Count == 0)
                return;

            string savePath = Path.Combine(SelectedFolder, saveFileName);
            if (File.Exists(savePath))
                File.SetAttributes(savePath, FileAttributes.Normal);

            SettingsManager.SaveTo(savePath, imageInfoCache, backup, true);

            File.SetAttributes(savePath, FileAttributes.Hidden);

            Console.WriteLine("Saved !");
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

        #region Image management

        /// <summary>
        /// Load images in the specified directory
        /// </summary>
        /// <param name="directoryPath"></param>
        private void LoadImages(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

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

            UpdateFromSave(); // Update selection from save
            SaveToFile(); // Save. Maybe more images, maybe no save yet

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
            treeView1.Focus();
        }

        /// <summary>
        /// Change the selection state of the selected image
        /// </summary>
        private void ToggleSelectedImage()
        {
            if (null == selectedImageInfo)
                return;

            selectedImageInfo.ToggleSelection();

            UpdateIsSelectedBackground();

            // Update picturebox image (border)
            pictureBox1.Image = selectedImageInfo.ReapplyBorder(pictureBox1.Image);
            GC.Collect();

            SaveToFile();
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
            ChooseFile();
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
                    ToggleSelectedImage();
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

        private void voirLaideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string helpStr =
                "1. Aller sous Fichier -> Ouvrir pour choisir un DOSSIER contenant les images à sélectionner\n\n"
                + "2. Sélectionner les images en utilisant les flèches Haut et Bas et la barre Espace\n\n"
                + "3. Aller sous Fichier -> Exporter pour choisir un DOSSIER où copier les images sélectionnées";
            System.Windows.MessageBox.Show(
                helpStr,
                "Aide",
                MessageBoxButton.OK,
                MessageBoxImage.Question
            );
        }

        private void toutCocherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Backup current save
            SaveToFile(true);

            foreach (ImageInfo imageInfo in imageInfoCache.Values)
            {
                imageInfo.IsSelected = true;
            }
        }

        private void toutDécocherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Backup current save
            SaveToFile(true);

            foreach (ImageInfo imageInfo in imageInfoCache.Values)
            {
                imageInfo.IsSelected = false;
            }
        }

        private void exporterLesImaesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelectedImages();
        }

        private void ExportSelectedImages()
        {
            var selectedImages = imageInfoCache.Where((x) => x.Value.IsSelected);

            if (0 == selectedImages.Count())
            {
                MessageBox.Show(
                    "Pas d'image sélectionnée trouvée",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            string folderSavePath = Path.Combine(
                Path.GetDirectoryName(SelectedFolder),
                Path.GetFileName(SelectedFolder) + " tri "
            );

            int counter = 1;
            while (Directory.Exists(folderSavePath + counter))
            {
                counter++;
            }
            folderSavePath += counter;
            Directory.CreateDirectory(folderSavePath);

            Console.WriteLine($"Saving to '{folderSavePath}'");

            foreach (var file in selectedImages)
            {
                string srcPath = Path.Combine(SelectedFolder, file.Key);
                string destPath = Path.Combine(folderSavePath, file.Key);
                File.Copy(srcPath, destPath, false);
            }

            Process.Start(folderSavePath);
        }

        private void ouvrirDossierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }
    }
}
