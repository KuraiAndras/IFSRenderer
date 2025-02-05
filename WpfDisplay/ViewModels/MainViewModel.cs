﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using IFSEngine.Utility;
using System;
using System.IO;
using System.Runtime;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfDisplay.Helper;
using WpfDisplay.Models;
using System.Diagnostics;
using System.Windows.Input;
using System.Collections.Generic;
using IFSEngine.Model;

namespace WpfDisplay.ViewModels
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        internal readonly Workspace workspace;

        public ToneMappingViewModel ToneMappingViewModel { get; }
        public CameraSettingsViewModel CameraSettingsViewModel { get; }
        public PerformanceViewModel PerformanceViewModel { get; }
        public QualitySettingsViewModel QualitySettingsViewModel { get; }
        public IFSViewModel IFSViewModel { get; }

        private bool transparentBackground;
        public bool TransparentBackground
        {
            get => transparentBackground;
            set
            {
                workspace.TakeSnapshot();
                if (value)
                    IFSViewModel.BackgroundColor = Colors.Black;
                SetProperty(ref transparentBackground, value);
                OnPropertyChanged(nameof(IsColorPickerEnabled));
            }
        }

        private string statusBarText;
        public string StatusBarText { get => statusBarText; set => SetProperty(ref statusBarText, value); }

        public bool IsColorPickerEnabled => !TransparentBackground;
        //Main display settings:
        public bool InvertAxisX => workspace.InvertAxisX;
        public bool InvertAxisY => workspace.InvertAxisY;
        public bool InvertAxisZ => workspace.InvertAxisZ;
        public float Sensitivity => (float)workspace.Sensitivity;


        public string WindowTitle => workspace.IFS is null ? "IFSRenderer" : $"{workspace.IFS.Title} - IFSRenderer";
        public string IFSTitle
        {
            get => workspace.IFS.Title;
            set
            {
                workspace.IFS.Title = value;
                OnPropertyChanged(nameof(IFSTitle));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
        public IEnumerable<Author> AuthorList => workspace.IFS.Authors;

        public MainViewModel(Workspace workspace)
        {
            this.workspace = workspace;
            workspace.StatusTextChanged += (s, e) => StatusBarText = e;
            workspace.PropertyChanged += (s, e) => OnPropertyChanged(string.Empty);
            PerformanceViewModel = new PerformanceViewModel(workspace);
            QualitySettingsViewModel = new QualitySettingsViewModel(workspace);
            IFSViewModel = new IFSViewModel(workspace);
            CameraSettingsViewModel = new CameraSettingsViewModel(workspace);
            CameraSettingsViewModel.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
            ToneMappingViewModel = new ToneMappingViewModel(workspace);
            ToneMappingViewModel.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
            workspace.UpdateStatusText($"Initialized");
        }

        private ICommand _newCommand;
        public ICommand NewCommand =>
            _newCommand ??= new RelayCommand(OnNewCommand);
        private void OnNewCommand()
        {
            workspace.LoadBlankParams();
            workspace.UpdateStatusText($"Blank parameters loaded");
        }

        private ICommand _loadRandomCommand;
        public ICommand LoadRandomCommand =>
            _loadRandomCommand ??= new RelayCommand(OnLoadRandomCommand);
        private void OnLoadRandomCommand()
        {
            workspace.LoadRandomParams();
            workspace.UpdateStatusText($"Randomly generated parameters loaded");
        }

        private AsyncRelayCommand _saveParamsCommand;
        public AsyncRelayCommand SaveParamsCommand =>
            _saveParamsCommand ??= new AsyncRelayCommand(OnSaveParamsCommand);
        private async Task OnSaveParamsCommand()
        {
            if (DialogHelper.ShowSaveParamsDialog(workspace.IFS.Title, out string path))
            {
                if (IFSTitle == "Untitled")//Set the file name as title
                    IFSTitle = Path.GetFileNameWithoutExtension(path);

                try
                {
                    await workspace.SaveParamsFileAsync(path);
                    workspace.UpdateStatusText($"Parameters saved to {path}");
                }
                catch(Exception)
                {
                    workspace.UpdateStatusText($"ERROR - Failed to save params.");
                }
            }
        }

        private AsyncRelayCommand _loadParamsCommand;
        public AsyncRelayCommand LoadParamsCommand =>
            _loadParamsCommand ??= new AsyncRelayCommand(OnLoadParamsCommand);
        private async Task OnLoadParamsCommand()
        {
            if (DialogHelper.ShowOpenParamsDialog(out string path))
            {
                try
                {
                    await workspace.LoadParamsFileAsync(path);
                    workspace.UpdateStatusText($"Parameters loaded from {path}");
                }
                catch (SerializationException ex)
                {
                    string logFilePath = App.LogException(ex);
                    workspace.UpdateStatusText($"ERROR - Failed to load params: {path}. See log: {logFilePath}");
                }
            }
        }

        /// <summary>
        /// Converts the raw pixel data into a BitmapSource with WPF specific calls.
        /// The Alpha channel is optionally removed and the image is flipped vertically, as required by CopyPixelDataToBitmap()
        /// </summary>
        /// <returns></returns>
        private async Task<BitmapSource> GetExportBitmapSource()
        {
            BitmapSource bs;
            WriteableBitmap wbm = new WriteableBitmap(workspace.Renderer.HistogramWidth, workspace.Renderer.HistogramHeight, 96, 96, PixelFormats.Bgra32, null);
            await workspace.Renderer.CopyPixelDataToBitmap(wbm.BackBuffer);
            wbm.Freeze();
            bs = wbm;
            if (!TransparentBackground)
            {//option to remove alpha channel
                var fcb = new FormatConvertedBitmap(wbm, PixelFormats.Bgr32, null, 0);
                fcb.Freeze();
                bs = fcb;
            }
            //flip vertically
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {//This transformation must happen on ui thread
                var tb = new TransformedBitmap();
                tb.BeginInit();
                tb.Source = bs;
                tb.Transform = new ScaleTransform(1, -1, 0, 0);
                tb.EndInit();
                tb.Freeze();
                bs = tb;
            });
            return bs;
        }

        private AsyncRelayCommand _exportToClipboardCommand;
        public AsyncRelayCommand ExportToClipboardCommand =>
            _exportToClipboardCommand ??= new AsyncRelayCommand(OnExportToClipboardCommand);
        private async Task OnExportToClipboardCommand()
        {
            BitmapSource bs = await GetExportBitmapSource();
            Clipboard.SetImage(bs);
            //TODO: somehow alpha channel is not copied

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            workspace.UpdateStatusText($"Image exported to clipboard");
        }

        private AsyncRelayCommand _saveImageCommand;
        public AsyncRelayCommand SaveImageCommand =>
            _saveImageCommand ??= new AsyncRelayCommand(OnSaveImageCommand);
        private async Task OnSaveImageCommand()
        {
            workspace.UpdateStatusText($"Exporting...");
            var makeBitmapTask = GetExportBitmapSource();

            if (DialogHelper.ShowExportImageDialog(workspace.IFS.Title, out string path))
            {
                BitmapSource bs = await makeBitmapTask;

                PngBitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bs));
                using (FileStream stream = new FileStream(path, FileMode.Create))
                    enc.Save(stream);

                //open the image for viewing. TODO: optional..
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                workspace.UpdateStatusText($"Image exported to {path}");
            }

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

        private AsyncRelayCommand _saveExrCommand;
        public AsyncRelayCommand SaveExrCommand =>
            _saveExrCommand ??= new AsyncRelayCommand(OnSaveExrCommand);
        private async Task OnSaveExrCommand()
        {
            workspace.UpdateStatusText($"Exporting...");
            Task<float[,,]> getDataTask = Task.Run(async () =>
            {
                return await workspace.Renderer.ReadHistogramData();
            });

            if (DialogHelper.ShowExportExrDialog(workspace.IFS.Title, out string path))
            {
                var histogramData = await getDataTask;
                using var fstream = File.Create(path);
                OpenEXR.WriteStream(fstream, histogramData);
                workspace.UpdateStatusText($"Image exported to {path}");
            }

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

        private AsyncRelayCommand _closeWorkspaceCommand;
        public AsyncRelayCommand CloseWorkspaceCommand =>
            _closeWorkspaceCommand ??= new AsyncRelayCommand(OnCloseWorkspaceCommand);

        private RelayCommand _takeSnapshotCommand;
        public RelayCommand TakeSnapshotCommand =>
            _takeSnapshotCommand ??= new RelayCommand(workspace.TakeSnapshot);

        private RelayCommand _interactionFinishedCommand;
        public ICommand InteractionFinishedCommand => _interactionFinishedCommand ??= new RelayCommand(CameraSettingsViewModel.RaisePropertyChanged);

        private async Task OnCloseWorkspaceCommand()
        {
            //TODO: prompt to save work?
            Dispose();
            Environment.Exit(0);
        }

        public void Dispose()
        {
            workspace.Renderer.Dispose();
        }

        private RelayCommand visitIssuesCommand;
        public ICommand VisitIssuesCommand => visitIssuesCommand ??= new RelayCommand(VisitIssues);
        private void VisitIssues()
        {
            //Open the Issues page in user's default browser
            string link = "https://github.com/bezo97/IFSRenderer/issues/";
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
        }

        private RelayCommand visitForumCommand;
        public ICommand VisitForumCommand => visitForumCommand ??= new RelayCommand(VisitForum);
        private void VisitForum()
        {
            //Open the Discussions page in user's default browser
            string link = "https://github.com/bezo97/IFSRenderer/discussions";
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
        }

        private RelayCommand reportBugCommand;
        public ICommand ReportBugCommand => reportBugCommand ??= new RelayCommand(ReportBug);
        private void ReportBug()
        {
            //Open the bug report template in user's default browser
            string link = "https://github.com/bezo97/IFSRenderer/issues/new?assignees=&labels=&template=bug_report.md";
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
        }

        private RelayCommand checkUpdatesCommand;
        public ICommand CheckUpdatesCommand => checkUpdatesCommand ??= new RelayCommand(CheckUpdates);
        private void CheckUpdates()
        {
            //Open the Releases page in user's default browser
            string link = "https://github.com/bezo97/IFSRenderer/releases";
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
        }

        private RelayCommand visitWikiCommand;
        public ICommand VisitWikiCommand => visitWikiCommand ??= new RelayCommand(VisitWiki);

        private void VisitWiki()
        {
            //Open the Wiki page in user's default browser
            string link = "https://github.com/bezo97/IFSRenderer/wiki";
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
        }
    }
}
