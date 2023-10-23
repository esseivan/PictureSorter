﻿using ESNLib.Tools;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using System.Drawing.Imaging;
using System.Text;
using System.Drawing;

namespace PictureSorter
{
    public partial class frmMain : Form
    {
        /// <summary>
        /// Keep track of the images informations in the selected directory
        /// </summary>
        private readonly Dictionary<string, ImageInfo> imageInfoCache =
            new Dictionary<string, ImageInfo>();

        /// <summary>
        /// The currently selected image from the treeview
        /// </summary>
        private ImageInfo selectedImageInfo = null;

        /// <summary>
        /// Manage the caching of images
        /// </summary>
        private readonly CacheManager cacheManager = new CacheManager();

        /// <summary>
        /// The control that changes it's background color according to the selected status
        /// </summary>
        private readonly Control selectedColorControl = null;

        /// <summary>
        /// The extensions of the image files available
        /// </summary>
        private string[] filters => AppSettingsManager.Instance.FileExtensionsFilter;

        /// <summary>
        /// The name of the saved progress file
        /// </summary>
        private string saveFileName => AppSettingsManager.Instance.AppSaveFileName;

        /// <summary>
        /// The currently selected folder
        /// </summary>
        public string SelectedFolder = string.Empty;

        /// <summary>
        /// Define wether the form is initialised
        /// </summary>
        private readonly bool IsFormInitialised = false;

        /// <summary>
        /// Current version of the app
        /// </summary>
        public string VersionLabel
        {
            get
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    Version ver = System
                        .Deployment
                        .Application
                        .ApplicationDeployment
                        .CurrentDeployment
                        .CurrentVersion;
                    return string.Format(
                        "Product Name: {4}, Version: {0}.{1}.{2}.{3}",
                        ver.Major,
                        ver.Minor,
                        ver.Build,
                        ver.Revision,
                        Assembly.GetEntryAssembly().GetName().Name
                    );
                }
                else
                {
                    var ver = Assembly.GetExecutingAssembly().GetName().Version;
                    return string.Format(
                        "Product Name: {4}, Version: {0}.{1}.{2}.{3}",
                        ver.Major,
                        ver.Minor,
                        ver.Build,
                        ver.Revision,
                        Assembly.GetEntryAssembly().GetName().Name
                    );
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public frmMain()
        {
            // Set language before components init
            SetLanguage();

            InitializeComponent();

            Logger appLogger = new Logger
            {
                FilePath = Logger.GetDefaultLogPath("ESN", "PictureSorter", "log.txt"),
                FilenameMode = Logger.FilenamesModes.FileName_DateSuffix,
                WriteMode = Logger.WriteModes.Append,
            };
            if (!appLogger.Enable())
            {
                MessageBox.Show(
                    "Unable to enable the logger...",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            appLogger.Write("Application ready !", Logger.LogLevels.Info);

            IsFormInitialised = true;

            selectedColorControl = panel1;
        }

        /// <summary>
        /// Called when app is shown
        /// </summary>
        private void Form1_Shown(object sender, EventArgs e)
        {
            ChooseFile();
        }

        /// <summary>
        /// Select the specified language. Can only be called before the init
        /// </summary>
        private void SetLanguage()
        {
            if (IsFormInitialised)
                throw new InvalidOperationException(
                    $"Unable to call {nameof(SetLanguage)} after initialisation"
                );

            Console.WriteLine("Language : " + Properties.Settings.Default.LanguageStr);
            Logger.Instance.Write("Language : " + Properties.Settings.Default.LanguageStr);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(
                Properties.Settings.Default.LanguageStr
            );
        }

        private void ChangeLanguage(string languageStr)
        {
            // Verify that the languageStr is valid. Otherwise a exception will be thrown
            _ = CultureInfo.GetCultureInfo(languageStr);
            Properties.Settings.Default.LanguageStr = languageStr;
            Properties.Settings.Default.Save();

            Logger.Instance.Write("Language change requested...");

            // Restart app
            Application.Restart();
            Environment.Exit(0);
        }

        #region Selection and File management

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
        /// Choose a file and select the parent folder
        /// </summary>
        private void ChooseFile()
        {
            var dialog = new CommonOpenFileDialog(Properties.strings.SelectPictureStr)
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

        /// <summary>
        /// Choose a folder
        /// </summary>
        private void ChooseFolder()
        {
            var dialog = new CommonOpenFileDialog(Properties.strings.SelectFolderStr)
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
        /// Save the progress into a predetermined save file in the folder
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

            SettingsManager.SaveTo(
                savePath,
                imageInfoCache,
                backup: backup
                    ? SettingsManager.BackupMode.dotBak
                    : SettingsManager.BackupMode.None,
                indent: true,
                hide: true,
                zipFile: false
            );

            Logger.Instance.Write($"Progres saved to '${savePath}'");
            Console.WriteLine("Saved !");
        }

        /// <summary>
        /// Sync the selection with the save in the folder
        /// </summary>
        private void UpdateFromSave()
        {
            string savePath = Path.Combine(SelectedFolder, saveFileName);

            if (!File.Exists(savePath))
                return;

            Logger.Instance.Write($"Loading progress '{savePath}'");

            Dictionary<string, ImageInfo> loadedData;
            try
            {
                bool loadSuccess = SettingsManager.LoadFrom(
                    path: savePath,
                    output: out loadedData,
                    zipFile: false
                );

                if (!loadSuccess)
                    return;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write("Failed to load progress...", Logger.LogLevels.Error);
                MessageBox.Show(
                    "Error loading progress : \n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Apply only the IsSelected property
            foreach (var dataItem in loadedData)
            {
                if (imageInfoCache.ContainsKey(dataItem.Key))
                {
                    imageInfoCache[dataItem.Key].IsSelected = dataItem.Value.IsSelected;
                }
            }
            Logger.Instance.Write("Progress loaded");
        }

        #endregion

        #region Image management

        private DateTime GetJpegDate(string filePath)
        {
            var directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(filePath);

            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    if (tag.Name == "Date/Time Original")
                    {
                        if (string.IsNullOrEmpty(tag.Description))
                            continue;
                        string d = tag.Description.Split(' ')[0].Replace(":", "-");
                        string t = tag.Description.Split(' ')[1];
                        return DateTime.Parse($"{d} {t}");
                    }
                }
            }

            throw new InvalidOperationException($"Date not found in {filePath}");
        }

        /// <summary>
        /// Sort the images according the the date taken
        /// </summary>
        public List<string> RenameImageFilesByDateTaken(string directoryPath)
        {
            // Load images
            List<string> imageFiles = new List<string>();
            foreach (string filter in filters)
            {
                imageFiles.AddRange(Directory.GetFiles(directoryPath, filter));
            }

            List<ImageDateInfo> imageList = new List<ImageDateInfo>();

            foreach (string filePath in imageFiles)
            {
                ImageDateInfo info = new ImageDateInfo { FilePath = filePath };
                try
                {
                    DateTime dt = GetJpegDate(filePath);
                    info.DateTaken = dt;
                    string fileName = $"IMG_{dt:yyyyMMdd_HHmmss}.{Path.GetExtension(filePath)}"; // In case of renaming all files
                    string destPath = Path.Combine(directoryPath, fileName);

                    Logger.Instance.Write($"Renaming '{filePath}' to '{destPath}'...");
                    File.Move(filePath, destPath);
                    int counter = 0;

                    Console.WriteLine(fileName);
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., if the file does not have EXIF information)
                    Logger.Instance.Write(
                        $"Unable to retrieve exif for '{filePath}' : {ex.Message}",
                        Logger.LogLevels.Error
                    );
                    info.DateTaken = DateTime.MinValue;
                }
                imageList.Add(info);
            }

            // Sort the list by Date Taken in ascending order
            imageList = imageList.OrderBy(img => img.DateTaken).ToList();

            // Extract the file paths from the sorted ImageInfo list
            sortedImageFiles = imageList.Select(img => img.FilePath).ToList();

            return sortedImageFiles;
        }

        internal class ImageDateInfo
        {
            public string FilePath { get; set; }
            public DateTime DateTaken { get; set; }
        }

        /// <summary>
        /// Rename a whole directory images according the the date taken of the picture
        /// </summary>
        private void RenameDirectoryContent(string directoryPath)
        {
            Logger.Instance.Write($"Renaming directory content '{directoryPath}'");

            Logger.Instance.Write($"Processing...");
            Cursor.Current = Cursors.WaitCursor;
            RenameImageFilesByDateTaken(directoryPath);
            Cursor.Current = Cursors.Default;
            Logger.Instance.Write($"Processing complete");
        }

        /// <summary>
        /// Load images from the specified directory
        /// </summary>
        /// <param name="directoryPath"></param>
        private void LoadImages(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            Logger.Instance.Write($"Loading directory '{directoryPath}'");

            SelectedFolder = directoryPath;

            cacheManager.Clear();
            imageInfoCache.Clear();
            treeView1.Nodes.Clear();

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

                // Add the treeview node
                treeView1.Nodes.Add(node);
                imageIndex++;
            }

            UpdateFromSave(); // Update selection from save
            SaveToFile(); // Save. Maybe more images, maybe no save yet

            if (imageInfoCache.Count > 0)
                treeView1.SelectedNode = treeView1.Nodes[0];

            treeView1.Focus();
            Logger.Instance.Write($"Directory loading complete !");
        }

        /// <summary>
        /// Update the backround color to inform if the current image is selected or not
        /// </summary>
        public void UpdateIsSelectedBackground()
        {
            if (null == selectedColorControl)
            {
                return;
            }

            if (null == selectedImageInfo)
            {
                return;
            }

            selectedColorControl.BackColor = selectedImageInfo.IsSelected
                ? ImageInfo.colorSelected
                : ImageInfo.colorNotSelected;
        }

        /// <summary>
        /// Export the selected images into a predefined folder
        /// </summary>
        private void ExportSelectedImages()
        {
            var selectedImages = imageInfoCache.Where((x) => x.Value.IsSelected);

            if (0 == selectedImages.Count())
            {
                MessageBox.Show(
                    Properties.strings.NoPictureCheckedErrorStr,
                    Properties.strings.ErrorStr,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            Logger.Instance.Write($"Exporting {selectedImages.Count()} images...");

            string folderSavePath = Path.Combine(
                Path.GetDirectoryName(SelectedFolder),
                Path.GetFileName(SelectedFolder) + $" {Properties.strings.sortStr} "
            );

            int counter = 1;
            while (Directory.Exists(folderSavePath + counter))
            {
                counter++;
            }
            folderSavePath += counter;
            Directory.CreateDirectory(folderSavePath);
            Logger.Instance.Write($"Folder choosen : {folderSavePath}");

            Logger.Instance.Write($"Saving to '{folderSavePath}'");
            Console.WriteLine($"Saving to '{folderSavePath}'");

            foreach (var file in selectedImages)
            {
                string srcPath = Path.Combine(SelectedFolder, file.Key);
                string destPath = Path.Combine(folderSavePath, file.Key);
                File.Copy(srcPath, destPath, false);
            }

            Logger.Instance.Write("Exporting complete !");
            Process.Start(folderSavePath);
        }

        #endregion

        #region Cache management

        /// <summary>
        /// Cache images after 10ms to be sure the UI is updated
        /// </summary>
        private async void CacheAsync()
        {
            await Task.Delay(10);

            PrepareCache(selectedImageInfo.Index, imageInfoCache.Count);
        }

        /// <summary>
        /// Prepare for caching
        /// </summary>
        /// <param name="index">The currently seleted index</param>
        /// <param name="maxIndex">Number of images in the list</param>
        private void PrepareCache(int index, int maxIndex)
        {
            if (0 == CacheManager.CACHE_MAX_SIZE) // no caching
                return;

            // Caching priority :
            //  1. Present image (always)
            //  2. Next image (in top to bottom direction)
            //  3. Previous image (in top to bottom direction)
            // The manager will decache the oldest images

            List<ImageInfo> toCache = new List<ImageInfo>();
            if ((index > 0) && CacheManager.CACHE_MAX_SIZE >= 3) // only if 3 or more
            {
                Logger.Instance.Write("Caching previous");
                toCache.Add(imageInfoCache.ElementAt(index - 1).Value);
            }

            Logger.Instance.Write($"Caching current, index={index}");
            toCache.Add(imageInfoCache.ElementAt(index).Value);

            if (((index + 1) < maxIndex) && CacheManager.CACHE_MAX_SIZE >= 2) // only if 2 or more
            {
                Logger.Instance.Write("Caching next");
                toCache.Add(imageInfoCache.ElementAt(index + 1).Value);
            }

            cacheManager.Add(toCache);
        }
        #endregion

        #region Events

        /// <summary>
        /// Quitter
        /// </summary>
        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Instance.Write("Application Closing...");
            Close();
        }

        /// <summary>
        /// Help tooltip
        /// </summary>
        private void voirLaideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                Properties.strings.HelpStr,
                "Aide",
                MessageBoxButtons.OK,
                MessageBoxIcon.Question
            );
        }

        /// <summary>
        /// Select all
        /// </summary>
        private void toutCocherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Backup current save
            SaveToFile(true);

            foreach (ImageInfo imageInfo in imageInfoCache.Values)
            {
                imageInfo.IsSelected = true;
            }
        }

        /// <summary>
        /// Unselect all
        /// </summary>
        private void toutDécocherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Backup current save
            SaveToFile(true);

            foreach (ImageInfo imageInfo in imageInfoCache.Values)
            {
                imageInfo.IsSelected = false;
            }
        }

        /// <summary>
        /// Export selection
        /// </summary>
        private void exporterLesImaesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelectedImages();
        }

        private void ouvrirDossierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        /// <summary>
        /// Image selected. Set into the picturebox and save into static variable
        /// </summary>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedFilePath = e.Node.FullPath;
            selectedImageInfo = imageInfoCache[selectedFilePath];

            // Set the picturebox image. Generally cached
            pictureBox1.Image = selectedImageInfo.GetImageAndCache();

            UpdateIsSelectedBackground();

            CacheAsync();
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
        /// Save progres. Automatically made so not really usefull
        /// </summary>
        private void sauvegarderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveToFile();
        }

        /// <summary>
        /// Open a file
        /// </summary>
        private void ouvrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChooseFile();
        }

        /// <summary>
        /// When a key is pressed. Global listener
        /// </summary>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // The button is made so that it cannot be focused,
            // so no worries to double toggle the selection
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

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                $"{VersionLabel}\nMade by EsseivaN\nhttps://github.com/esseivan/PictureSorter",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void frenchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLanguage("fr");
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLanguage("en");
        }

        private void renommerDossierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog(Properties.strings.SelectPictureStr)
            {
                IsFolderPicker = false
            };
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                string folder = Path.GetDirectoryName(dialog.FileName);

                RenameDirectoryContent(folder);
            }
        }

        #endregion
    }
}