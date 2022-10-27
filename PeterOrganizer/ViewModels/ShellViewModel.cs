using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using PeterOrganizer.Models;

namespace PeterOrganizer.ViewModels
{
    public sealed class ShellViewModel : Conductor<object>
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private string _selectedFolder;
        public ObservableCollection<PeteFile> Files { get; set; } = new ObservableCollection<PeteFile>();

        public ShellViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            PropertyChanged += ShellViewModel_PropertyChanged;
        }

        private void ShellViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedFolder))
            {
                Files.Clear();
                Task.Run(ProcessFiles);
            }
        }

        public void OpenFolder()
        {
            Process.Start("explorer.exe", $"\"{SelectedFolder}\"");
        }

        public void StarProcessing()
        {
            Task.Run(async () =>
            {
                var progress = await _dialogCoordinator.ShowProgressAsync(this, "Processing", "Please wait while moving files...");
                progress.SetIndeterminate();

                foreach (var peteFile in Files)
                {
                    try
                    {
                        var destination = Path.Combine(SelectedFolder, peteFile.ProductName, $"{peteFile.ColorProfile}{peteFile.Extension}".ToUpper(), $"{peteFile.ProductName}_{peteFile.SizeState}{peteFile.Extension}");
                        var folderName = Path.GetDirectoryName(destination);
                        if (!string.IsNullOrWhiteSpace(folderName) && !Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }

                        File.Move(peteFile.Origin, destination);
                        Execute.OnUIThread(() => peteFile.Status = "SUCCESS");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        Execute.OnUIThread(() => peteFile.Status = $"ERROR - {e.Message}{Environment.NewLine}{e.StackTrace}");
                    }
                }

                await progress.CloseAsync();
            });

        }

        private void ProcessFiles()
        {
            var pattern = new Regex(@"([a-z0-9]+)_(.+?)([a-z0-9\-]+)\.([a-z0-9\-]+)", RegexOptions.IgnoreCase);

            var files = Directory.EnumerateFiles(SelectedFolder, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var parts = pattern.Match(Path.GetFileName(file));
                if (parts.Success)
                {
                    var peteFile = new PeteFile
                    {
                        Origin = file,
                        ProductName = parts.Groups[1].Value.Trim(" \t\r\n_".ToCharArray()),
                        SizeState = parts.Groups[2].Value.Trim(" \t\r\n_".ToCharArray()),
                        ColorProfile = parts.Groups[3].Value.Trim(" \t\r\n_".ToCharArray()),
                        Extension = Path.GetExtension(file).Trim(" \t\r\n_".ToCharArray()),
                        Filename = file.Replace(SelectedFolder,"").Trim("\\/".ToCharArray())
                    };

                    Execute.OnUIThread(() => Files.Add(peteFile));

                }
            }
        }

        public void SelectFolder()
        {
            var dlg = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false
            };
            var dlgResult = dlg.ShowDialog();
            if (dlgResult == CommonFileDialogResult.Ok)
            {
                SelectedFolder = dlg.FileName;
            }
        }

        public string SelectedFolder
        {
            get => _selectedFolder;
            set => Set(ref _selectedFolder, value);
        }
    }
}
