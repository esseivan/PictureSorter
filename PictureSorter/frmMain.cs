using ESNLib.Tools;
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
using System.Text.RegularExpressions;

namespace PictureSorter
{
    public partial class frmMain : Form
    {
        private bool INDENT_SAVE_FILE => AppSettingsManager.Instance.IndentSaveFile;
        private bool HIDE_SAVE_FILE => AppSettingsManager.Instance.HideSaveFile;

        private string LOCK_FILENAME => ".psorter.lock";

        private readonly string RUNTIME_ID;

        private Stream _lockfileStream = null;

        /// <summary>
        /// Keep track of the images informations in the selected directory
        /// </summary>
        private readonly Dictionary<string, ImageInfo> imageInfoCache =
            new Dictionary<string, ImageInfo>();

        /// <summary>
        /// Keep track of the images informations in the selected directory
        /// </summary>
        private readonly Dictionary<int, string> imageInfoIndices = new Dictionary<int, string>();

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
        /// The name of the saved progress/config file
        /// </summary>
        private string saveFileName => AppSettingsManager.Instance.AppSaveFileName;

        /// <summary>
        /// The currently selected folder
        /// </summary>
        public string SelectedFolder = string.Empty;

        /// <summary>
        /// Path to the current lock file
        /// </summary>
        private string lockFilePath = string.Empty;

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

        public bool WriteLog(string str) => Logger.Instance.Write(str);

        public bool WriteLog(string str, Logger.LogLevels level) =>
            Logger.Instance.Write(str, level);

        /// <summary>
        /// Constructor
        /// </summary>
        public frmMain()
        {
            // Set language before components init
            SetLanguage();

            InitializeComponent();

            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // Generate random ID for this instance. Used for lock file
            Random rnd = new Random();
            RUNTIME_ID = rnd.Next().ToString();

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
            appLogger.Write($"Runtime ID : {RUNTIME_ID}", Logger.LogLevels.Info);

            IsFormInitialised = true;

            selectedColorControl = panel1;
        }

        /// <summary>
        /// Called when app is shown
        /// </summary>
        private void Form1_Shown(object sender, EventArgs e)
        {
            //ChooseFile();
            ChooseFolder();
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
            WriteLog("Language : " + Properties.Settings.Default.LanguageStr);

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

            WriteLog("Language change requested...");

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
                indent: INDENT_SAVE_FILE,
                hide: HIDE_SAVE_FILE,
                zipFile: false
            );

            WriteLog($"Progres saved to '${savePath}'");
            Console.WriteLine("Saved !");
        }

        /// <summary>
        /// Create the lock file
        /// </summary>
        /// <returns>True if success</returns>
        private bool _createLockFile()
        {
            try
            {
                File.WriteAllText(lockFilePath, RUNTIME_ID);
                File.SetAttributes(lockFilePath, FileAttributes.Hidden);
                _lockfileStream = File.Open(lockFilePath, FileMode.Open);
            }
            catch (Exception ex)
            {
                WriteLog("Unable to create lock file...", Logger.LogLevels.Fatal);
                WriteLog(ex.Message, Logger.LogLevels.Fatal);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Try to lock the folder by creating a .lock file. If already present, ask the user
        /// </summary>
        /// <returns>If the folder can be open (no lock file present or user forced)</returns>
        private bool LockFolder()
        {
            WriteLog("Locking folder...");
            if (File.Exists(lockFilePath))
            {
                WriteLog("Lock file already present...", Logger.LogLevels.Warn);
                // Already open from another app, or not properly exited the app
                // So asking the user if he want to open anyway
                DialogResult result = MessageBox.Show(
                    Properties.strings.warningLockedStr,
                    "Warning",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning
                );
                if (result != DialogResult.Yes)
                {
                    // Indicate not available
                    return false;
                }

                // User want to try and take control of the folder
                try
                {
                    File.Delete(lockFilePath);
                }
                catch (Exception ex)
                {
                    WriteLog("Failed to delete lock file...", Logger.LogLevels.Error);
                    WriteLog(ex.Message, Logger.LogLevels.Error);
                    MessageBox.Show(
                        Properties.strings.errorLockDeleteFailed,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return false;
                }
            }

            return _createLockFile();
        }

        /// <summary>
        /// Unload and unlock current folder
        /// </summary>
        private void UnlockFolder()
        {
            if (_lockfileStream == null)
            {
                return;
            }

            // Invalid or no selected folder
            if (!Directory.Exists(SelectedFolder))
            {
                return;
            }

            WriteLog("Unlocking folder...", Logger.LogLevels.Debug);

            string lockFilePath = Path.Combine(SelectedFolder, LOCK_FILENAME);
            if (!File.Exists(lockFilePath))
            {
                WriteLog("No lock file found...", Logger.LogLevels.Error);
                return;
            }
            else
            {
                _lockfileStream.Dispose();
                _lockfileStream = null;
                File.Delete(lockFilePath);
            }
        }

        private DateTime ExtractDatetime(string dateTimeStr, string format)
        {
            // Parse
            bool success = DateTime.TryParseExact(
                dateTimeStr,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date
            );
            if (!success)
            {
                return DateTime.MinValue;
            }

            return date;
        }

        private DateTime GetDateFromName(string name)
        {
            Regex regex = new Regex("^.*(?<datetime>\\d{8}.\\d{6}).*$");
            Match result = regex.Match(name);
            if (result.Success)
            {
                string dateTimeStr = result.Groups["datetime"].Value;
                dateTimeStr = dateTimeStr.Remove(8, 1);
                return ExtractDatetime(dateTimeStr, "yyyyMMddHHmmss");
            }

            regex = new Regex("^.*(?<datetime>\\d{4}.\\d{2}.\\d{2}.\\d{6}).*$");
            result = regex.Match(name);
            if (result.Success)
            {
                string dateTimeStr = result.Groups["datetime"].Value;
                dateTimeStr = dateTimeStr.Remove(4, 1).Remove(6, 1).Remove(8, 1);
                return ExtractDatetime(dateTimeStr, "yyyyMMddHHmmss");
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Sync the selection with the save in the folder
        /// </summary>
        private void UpdateFromSave()
        {
            string savePath = Path.Combine(SelectedFolder, saveFileName);

            if (!File.Exists(savePath))
                return;

            WriteLog($"Loading progress '{savePath}'");

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
                WriteLog("Failed to load progress...", Logger.LogLevels.Error);
                WriteLog(ex.Message, Logger.LogLevels.Error);
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
                    imageInfoCache[dataItem.Key].DateTimeTaken = dataItem.Value.DateTimeTaken;
                }
            }
            WriteLog("Progress loaded");
        }

        #endregion

        #region Image management

        private void RenameImages()
        {
            // Exporter et renommer les images
            ExportAndRenameSelectedImages();
        }

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
            WriteLog($"Date not found in {filePath}", Logger.LogLevels.Error);

            return DateTime.MinValue;
        }

        /// <summary>
        /// Sort the images according the the date taken
        /// </summary>
        public List<string> SortImageFilesByDateTaken(List<string> imageFiles)
        {
            List<ImageDateInfo> imageList = new List<ImageDateInfo>();

            foreach (string filePath in imageFiles)
            {
                ImageDateInfo info = new ImageDateInfo { FilePath = filePath };
                try
                {
                    DateTime dt = GetJpegDate(filePath);
                    info.DateTaken = dt;
                }
                catch (Exception ex)
                {
                    info.DateTaken = DateTime.MinValue;

                    // Handle exceptions (e.g., if the file does not have EXIF information)
                    WriteLog(
                        $"Unable to retrieve exif for '{filePath}' : {ex.Message}",
                        Logger.LogLevels.Error
                    );
                }
                imageList.Add(info);
            }

            // Sort the list by Date Taken in ascending order
            imageList = imageList.OrderBy(img => img.DateTaken).ToList();

            // Extract the file paths from the sorted ImageInfo list
            List<string> sortedImageFiles = imageList.Select(img => img.FilePath).ToList();

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
        private void SortDirectoryContent(string directoryPath)
        {
            WriteLog($"Renaming directory content '{directoryPath}'");
        }

        /// <summary>
        /// Close the current folder
        /// </summary>
        private void CloseFolder()
        {
            UnlockFolder();

            pictureBox1.Image = null;
            cacheManager.Clear();
            imageInfoCache.Clear();
            imageInfoIndices.Clear();
            treeView1.Nodes.Clear();
        }

        /// <summary>
        /// Load images from the specified directory
        /// </summary>
        /// <param name="directoryPath"></param>
        private void LoadImages(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            CloseFolder();
            WriteLog($"Loading directory '{directoryPath}'");

            SelectedFolder = directoryPath;
            lockFilePath = Path.Combine(SelectedFolder, LOCK_FILENAME);
            if (!LockFolder())
            {
                SelectedFolder = string.Empty;
                WriteLog($"Couldn't lock folder. Folder not open");
                return;
            }

            List<string> imageFiles = new List<string>();
            foreach (string filter in filters)
            {
                imageFiles.AddRange(Directory.GetFiles(directoryPath, filter));
            }

            foreach (string imageFullPath in imageFiles)
            {
                // Full path to the image
                string imageFileName = Path.GetFileName(imageFullPath);

                // New treeview node
                TreeNode node = new TreeNode
                {
                    ImageIndex = -1,
                    SelectedImageIndex = -1,
                    Text = imageFileName,
                };

                // Save image info
                ImageInfo imageInfo = new ImageInfo
                {
                    FullPath = imageFullPath,
                    FileName = imageFileName,
                    Index = -1,
                    Node = node,
                    IsSelected = true
                };

                // Cache the image info
                imageInfoCache[imageFileName] = imageInfo;
            }

            UpdateFromSave(); // Update selection from save
            SaveToFile(); // Save. Maybe more images, maybe no save yet

            WriteLog($"Processing missing DateTimeTaken...");
            frmProcessing frm = new frmProcessing();
            frm.SetText(Properties.strings.txtProcessingLoading);
            frm.Show();
            Application.DoEvents();
            Cursor.Current = Cursors.WaitCursor;
            // Update missing dateTimeTaken infos
            int ctr = 0;
            int max = imageInfoCache.Count;
            foreach (var item in imageInfoCache)
            {
                if (item.Value.DateTimeTaken == DateTime.MinValue)
                {
                    try
                    {
                        DateTime dt = GetJpegDate(item.Value.FullPath);
                        if (dt == DateTime.MinValue)
                            dt = GetDateFromName(Path.GetFileName(item.Value.FullPath));
                        item.Value.DateTimeTaken = dt;
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (e.g., if the file does not have EXIF information)
                        WriteLog(
                            $"Unable to retrieve exif for '{item.Value.FullPath}' : {ex.Message}",
                            Logger.LogLevels.Error
                        );
                    }
                }
                ctr++;
                frm.SetCounter(ctr, max);
            }

            frm.Close();
            Cursor.Current = Cursors.Default;
            WriteLog($"Processing complete, saving...");
            SaveToFile(); // Save. Maybe more images, maybe no save yet
            WriteLog($"Saving complete. Displaying images...");

            List<ImageInfo> images = imageInfoCache.Values.ToList();
            images.Sort((x, y) => x.DateTimeTaken.CompareTo(y.DateTimeTaken));
            int i = 0;
            foreach (ImageInfo item in images)
            {
                item.Index = i;
                item.Node.ImageIndex = item.Node.SelectedImageIndex = i;

                imageInfoIndices.Add(i++, item.FileName);
                treeView1.Nodes.Add(item.Node);
            }

            if (imageInfoCache.Count > 0)
                treeView1.SelectedNode = treeView1.Nodes[0];

            treeView1.Focus();
            WriteLog($"Directory loading complete !");
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
            WriteLog($"Exporting {selectedImages.Count()} images...");

            string directoryName = Path.GetFileName(SelectedFolder);
            // Check if current directory match the sortStr
            Regex matchSortStr = new Regex("^(.*)( tri [1-9][0-9]*)$");
            Match result = matchSortStr.Match(SelectedFolder);
            if (result.Success)
            {
                // When it does, remove that part from the output directory.
                // That correspond to keeping the group1 (the "(.*)" from the regex)
                directoryName = result.Groups[1].Value;
            }
            string folderSavePath = Path.Combine(
                Path.GetDirectoryName(SelectedFolder),
                directoryName + $" {Properties.strings.sortStr} "
            );

            int counter = 1;
            while (Directory.Exists(folderSavePath + counter))
            {
                counter++;
            }
            folderSavePath += counter;
            Directory.CreateDirectory(folderSavePath);
            WriteLog($"Folder choosen : {folderSavePath}");

            WriteLog($"Saving to '{folderSavePath}'");
            Console.WriteLine($"Saving to '{folderSavePath}'");

            Cursor.Current = Cursors.WaitCursor;
            frmProcessing frm = new frmProcessing();
            frm.SetText(Properties.strings.txtProcessingExport);
            frm.Show();

            Dictionary<string, ImageInfo> exportedImages = new Dictionary<string, ImageInfo>();
            int ctr = 0;
            int max = selectedImages.Count();
            foreach (var file in selectedImages)
            {
                string srcPath = Path.Combine(SelectedFolder, file.Key);
                string destPath = Path.Combine(folderSavePath, file.Key);
                File.Copy(srcPath, destPath, false);
                // Also copy image info dateTime
                exportedImages.Add(file.Key, file.Value);

                ctr++;
                frm.SetCounter(ctr, max);
            }

            // Save the exportedImages informations
            string savePath = Path.Combine(folderSavePath, saveFileName);
            if (File.Exists(savePath))
                File.SetAttributes(savePath, FileAttributes.Normal);

            SettingsManager.SaveTo(
                savePath,
                exportedImages,
                backup: SettingsManager.BackupMode.dotBak,
                indent: INDENT_SAVE_FILE,
                hide: HIDE_SAVE_FILE,
                zipFile: false
            );

            frm.Close();
            Cursor.Current = Cursors.Default;

            WriteLog($"Progres saved to '${savePath}'");
            Console.WriteLine("Saved !");

            WriteLog("Exporting complete !");
            Process.Start(folderSavePath);

            if (AppSettingsManager.Instance.OpenFolderInAppAfterExport)
            {
                WriteLog("Openning new folder just after export...");
                LoadImages(folderSavePath);
            }
        }

        class FileRenameSet
        {
            public string src;
            public string dest;

            public FileRenameSet(string src, string dest)
            {
                this.src = src;
                this.dest = dest;
            }
        }

        /// <summary>
        /// Rename all the images, without creating a new folder. Confirmation must be asked prior
        /// </summary>
        private void RenameAllImages()
        {
            var selectedImages = GetAllImages();

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
            WriteLog($"Renaming {selectedImages.Count()} images...");

            // Show processing... form
            Cursor.Current = Cursors.WaitCursor;
            frmProcessing frmProgress = new frmProcessing();
            frmProgress.SetText(Properties.strings.txtProcessingExport);
            frmProgress.Show();

            // Information set for exported images. Image name may change, so need to create a new one
            Dictionary<string, ImageInfo> newImageConfig = new Dictionary<string, ImageInfo>();
            List<FileRenameSet> RenameList = new List<FileRenameSet>();
            int ctr = 0;
            int max = selectedImages.Count() * 3;
            foreach (KeyValuePair<string, ImageInfo> file in selectedImages)
            {
                string srcFileName = Path.Combine(SelectedFolder, file.Key);
                string destFileName = RenameFileFromDate(file);
                string destPath = DetermineFileDestinationPathFromList(
                    SelectedFolder,
                    destFileName,
                    RenameList
                ); // Add suffix if necessary
                FileRenameSet set = new FileRenameSet(srcFileName, destPath);
                RenameList.Add(set);

                // Save to new config
                string acceptedFinalFileName = Path.GetFileName(destPath); // The filename with eventual suffixes

                // Also copy image info dateTime
                if (!newImageConfig.ContainsKey(acceptedFinalFileName))
                    newImageConfig.Add(acceptedFinalFileName, file.Value);
                else
                    WriteLog(
                        $"Image key already present in save... '{acceptedFinalFileName}'",
                        Logger.LogLevels.Error
                    );

                // Progress counter
                ctr++;
                frmProgress.SetCounter(ctr, max);
            }

            CloseFolder();
            Application.DoEvents();
            try
            {
                // First thing first, rename them to a unique name
                for (int i = 0; i < RenameList.Count; i++)
                {
                    FileRenameSet renameSet = RenameList[i];

                    // generate random name
                    string myUniqueFileName = $@"{Guid.NewGuid()}";
                    // get random path
                    string myRndPath = Path.Combine(
                        Path.GetDirectoryName(renameSet.src),
                        myUniqueFileName
                    );
                    myRndPath = Path.ChangeExtension(myRndPath, Path.GetExtension(renameSet.src));

                    WriteLog($"Renaming {renameSet.src} to {myRndPath}...");
                    File.Move(renameSet.src, myRndPath);

                    // Edit rename set
                    renameSet.src = myRndPath;

                    // Progress counter
                    ctr++;
                    frmProgress.SetCounter(ctr, max);
                }

                // Rename
                foreach (FileRenameSet renameSet in RenameList)
                {
                    WriteLog($"Renaming {renameSet.src} to {renameSet.dest}...");
                    File.Move(renameSet.src, renameSet.dest);

                    // Progress counter
                    ctr++;
                    frmProgress.SetCounter(ctr, max);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to rename files... :\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                WriteLog("Error trying to rename files...", Logger.LogLevels.Error);
                WriteLog(ex.Message, Logger.LogLevels.Error);
            }

            // Save the exportedImages informations
            string configSavePath = Path.Combine(SelectedFolder, saveFileName);
            if (File.Exists(configSavePath))
                File.SetAttributes(configSavePath, FileAttributes.Normal); // Need to unhide in order to delete/overwrite

            SettingsManager.SaveTo(
                configSavePath,
                newImageConfig,
                backup: SettingsManager.BackupMode.dotBak,
                indent: INDENT_SAVE_FILE,
                hide: HIDE_SAVE_FILE,
                zipFile: false
            );

            frmProgress.Close();
            Cursor.Current = Cursors.Default;

            WriteLog($"Progres saved to '${configSavePath}'");
            Console.WriteLine("Saved !");

            WriteLog("Exporting complete !");
            Process.Start(SelectedFolder);

            if (AppSettingsManager.Instance.OpenFolderInAppAfterExport)
            {
                WriteLog("Openning new folder just after export...");
                LoadImages(SelectedFolder);
            }
        }

        private Dictionary<string, ImageInfo> GetAllImages() => imageInfoCache;

        private IEnumerable<KeyValuePair<string, ImageInfo>> GetSelectedImages() =>
            imageInfoCache.Where((x) => x.Value.IsSelected);

        private string SelectTriFolder()
        {
            string directoryName = Path.GetFileName(SelectedFolder);
            // Check if current directory match the sortStr
            Regex matchSortStr = new Regex("^(.*)( tri [1-9][0-9]*)$");
            Match result = matchSortStr.Match(SelectedFolder);
            if (result.Success)
            {
                // When it does, remove that part from the output directory.
                // That correspond to keeping the group1 (the "(.*)" from the regex)
                directoryName = result.Groups[1].Value;
            }
            string folderSavePath = Path.Combine(
                Path.GetDirectoryName(SelectedFolder),
                directoryName + $" {Properties.strings.sortStr} "
            );

            int counter = 1;
            while (Directory.Exists(folderSavePath + counter))
            {
                counter++;
            }
            folderSavePath += counter;
            Directory.CreateDirectory(folderSavePath);
            WriteLog($"Folder choosen : {folderSavePath}");

            return folderSavePath;
        }

        private string RenameFileFromDate(KeyValuePair<string, ImageInfo> file)
        {
            string newFileName = file.Key;
            if (file.Value.DateTimeTaken != DateTime.MinValue)
            {
                // Datetime available. Rename
                newFileName =
                    $"img_{file.Value.DateTimeTaken.ToString("yyyyMMdd_HHmmss")}{Path.GetExtension(file.Key)}";
            }
            else
            {
                // No dat found, add '__' prefix
                newFileName = "__" + newFileName;
            }

            return newFileName;
        }

        /// <summary>
        /// Given the bsae destination path, add an incrementing suffix if already existing.
        /// Instead of checking from Hard Drive, check from a list
        /// </summary>
        /// <param name="fileName">The image filename</param>
        /// <param name="destinationFolder">The destination folder of the image</param>
        private string DetermineFileDestinationPathFromList(
            string destinationFolder,
            string fileName,
            List<FileRenameSet> renameList
        )
        {
            string ext = Path.GetExtension(fileName);
            string destinationPath = Path.Combine(destinationFolder, fileName);
            int i = 2;
            var destPaths = renameList.Select((x) => x.dest);
            while (destPaths.Contains(destinationPath))
            {
                destinationPath = Path.Combine(
                    destinationFolder,
                    $"{Path.GetFileNameWithoutExtension(fileName)}_{i}{ext}"
                );
                i++;
            }

            return destinationPath;
        }

        /// <summary>
        /// Given the bsae destination path, add an incrementing suffix if already existing
        /// </summary>
        /// <param name="fileName">The image filename</param>
        /// <param name="destinationFolder">The destination folder of the image</param>
        private string DetermineFileDestinationPath(string destinationFolder, string fileName)
        {
            string ext = Path.GetExtension(fileName);
            string destinationPath = Path.Combine(destinationFolder, fileName);
            int i = 2;
            while (File.Exists(destinationPath))
            {
                destinationPath = Path.Combine(
                    destinationFolder,
                    $"{Path.GetFileNameWithoutExtension(fileName)}_{i}{ext}"
                );
                i++;
            }

            return destinationPath;
        }

        /// <summary>
        /// Export the selected images into a predefined folder
        /// </summary>
        private void ExportAndRenameSelectedImages()
        {
            var selectedImages = GetSelectedImages();

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
            WriteLog($"Exporting {selectedImages.Count()} images...");

            string folderSavePath = SelectTriFolder();
            WriteLog($"Saving to '{folderSavePath}'");
            Console.WriteLine($"Saving to '{folderSavePath}'");

            // Show processing... form
            Cursor.Current = Cursors.WaitCursor;
            frmProcessing frmProgress = new frmProcessing();
            frmProgress.SetText(Properties.strings.txtProcessingExport);
            frmProgress.Show();

            // Information set for exported images. Image name may change, so need to create a new one
            Dictionary<string, ImageInfo> exportedImages = new Dictionary<string, ImageInfo>();
            int ctr = 0;
            int max = selectedImages.Count();
            foreach (KeyValuePair<string, ImageInfo> file in selectedImages)
            {
                string newFileName = RenameFileFromDate(file);
                string srcPath = Path.Combine(SelectedFolder, file.Key);
                string destPath = DetermineFileDestinationPath(folderSavePath, newFileName);
                string acceptedFinalFileName = Path.GetFileName(destPath); // The filename with eventual suffixes

                File.Copy(srcPath, destPath, overwrite: false);
                // Also copy image info dateTime
                if (!exportedImages.ContainsKey(acceptedFinalFileName))
                    exportedImages.Add(acceptedFinalFileName, file.Value);
                else
                    WriteLog(
                        $"Image key already present in save... '{acceptedFinalFileName}'",
                        Logger.LogLevels.Error
                    );

                // Progress counter
                ctr++;
                frmProgress.SetCounter(ctr, max);
            }

            // Save the exportedImages informations
            string configSavePath = Path.Combine(folderSavePath, saveFileName);
            if (File.Exists(configSavePath))
                File.SetAttributes(configSavePath, FileAttributes.Normal); // Need to unhide in order to delete/overwrite

            SettingsManager.SaveTo(
                configSavePath,
                exportedImages,
                backup: SettingsManager.BackupMode.dotBak,
                indent: INDENT_SAVE_FILE,
                hide: HIDE_SAVE_FILE,
                zipFile: false
            );

            frmProgress.Close();
            Cursor.Current = Cursors.Default;

            WriteLog($"Progres saved to '${configSavePath}'");
            Console.WriteLine("Saved !");

            WriteLog("Exporting complete !");
            Process.Start(folderSavePath);

            if (AppSettingsManager.Instance.OpenFolderInAppAfterExport)
            {
                WriteLog("Openning new folder just after export...");
                LoadImages(folderSavePath);
            }
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
            string path;
            if ((index > 0) && CacheManager.CACHE_MAX_SIZE >= 3) // only if 3 or more
            {
                WriteLog("Caching previous");
                path = imageInfoIndices[index - 1];
                toCache.Add(imageInfoCache[path]);
            }

            WriteLog($"Caching current, index={index}");
            path = imageInfoIndices[index];
            toCache.Add(imageInfoCache[path]);

            if (((index + 1) < maxIndex) && CacheManager.CACHE_MAX_SIZE >= 2) // only if 2 or more
            {
                WriteLog("Caching next");
                path = imageInfoIndices[index + 1];
                toCache.Add(imageInfoCache[path]);
            }

            cacheManager.Add(toCache);
        }
        #endregion

        #region Events

        /// <summary>
        /// Renommer les images en fonction de la date où elles ont été prises
        /// </summary>
        private void renameExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportAndRenameSelectedImages();
        }

        /// <summary>
        /// Quitter
        /// </summary>
        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WriteLog("Application Closing...");
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
            foreach (ImageInfo imageInfo in imageInfoCache.Values)
            {
                imageInfo.IsSelected = true;
            }

            UpdateIsSelectedBackground();
            // Update picturebox image (border)
            pictureBox1.Image = selectedImageInfo.ReapplyBorder(pictureBox1.Image);
            GC.Collect();

            // Save
            SaveToFile(backup: true);
        }

        /// <summary>
        /// Unselect all
        /// </summary>
        private void toutDécocherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ImageInfo imageInfo in imageInfoCache.Values)
            {
                imageInfo.IsSelected = false;
            }

            UpdateIsSelectedBackground();
            // Update picturebox image (border)
            pictureBox1.Image = selectedImageInfo.ReapplyBorder(pictureBox1.Image);
            GC.Collect();

            // Save
            SaveToFile(backup: true);
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

            // Update selected image index
            lblImageCounter.Text = $"{selectedImageInfo.Index + 1}/{imageInfoCache.Count}";

            // Set the picturebox image. Generally cached
            Image img = selectedImageInfo.GetImageAndCache();

            img = ImageTools.applyBorderToImage(
                img,
                selectedImageInfo.IsSelected ? ImageInfo.colorSelected : ImageInfo.colorNotSelected,
                0.01f // 1 %
            );
            pictureBox1.Image = img;

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

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteLog("Application Closing...");
            UnlockFolder();
        }

        #endregion

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameAllImages();
        }

        private void voirDossierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(SelectedFolder);
        }

        private void voirLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Logger.Instance.FileOutputPath);
        }
    }
}
