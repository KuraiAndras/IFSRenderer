﻿using IFSEngine.Model;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using IFSEngine.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WpfDisplay.Helper;
using System;
using WpfDisplay.Properties;
using IFSEngine.Serialization;
using IFSEngine.Generation;
using System.Configuration;

namespace WpfDisplay.Models
{
    /// <summary>
    /// The main workspace model that contains a <see cref="RendererGL"/> 
    /// and an <see cref="IFSEngine.Model.IFS"/> that it is rendering.
    /// </summary>
    public class Workspace : ObservableObject
    {
        private readonly IFSHistoryTracker tracker = new();
        private List<Transform> loadedTransforms = new();

        public event EventHandler<string> StatusTextChanged;
        public string TransformsDirectoryPath { get; } = Path.Combine(App.AppDataPath, "Transforms");
        public IReadOnlyCollection<Transform> LoadedTransforms => loadedTransforms;
        public Author CurrentUser { get; set; } = Author.Unknown;
        public bool InvertAxisX, InvertAxisY, InvertAxisZ;
        public double Sensitivity;

        private RendererGL renderer;
        public RendererGL Renderer
        {
            get => renderer;
            set
            {
                SetProperty(ref renderer, value);
                if (ifs != null)
                    renderer.LoadParams(ifs);
            }
        }


        private IFS ifs;
        public IFS IFS
        {
            get => ifs;
            private set => SetProperty(ref ifs, value);
        }

        public bool IsHistoryUndoable => tracker.IsHistoryUndoable;
        public bool IsHistoryRedoable => tracker.IsHistoryRedoable;

        public Workspace(RendererGL r)
        {
            LoadTransformLibrary();
            Renderer = r;
            Renderer.Initialize(loadedTransforms);
            IFS = new IFS();
            Renderer.LoadParams(ifs);
            LoadUserSettings();
        }

        public async Task ReloadTransforms()
        {
            LoadTransformLibrary();
            IFS.ReloadTransforms(LoadedTransforms);
            await Renderer.LoadTransforms(LoadedTransforms);
            OnPropertyChanged(nameof(LoadedTransforms));
        }

        private void LoadTransformLibrary()
        {
            loadedTransforms = Directory
                .GetFiles(TransformsDirectoryPath, "*.ifstf", SearchOption.AllDirectories)
                .Select(file => Transform.FromFile(file))
                .ToList();
            OnPropertyChanged(nameof(LoadedTransforms));
        }

        public void LoadParams(IFS ifs)
        {
            TakeSnapshot();
            renderer?.LoadParams(ifs);
            IFS = ifs;
            if (!Renderer.IsRendering)
                Renderer.StartRenderLoop();
        }

        public async Task LoadParamsFileAsync(string path)
        {
            IFS ifs;
            try
            {
                ifs = await IfsSerializer.LoadJsonFileAsync(path, LoadedTransforms, false);
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
                if (System.Windows.MessageBox.Show("Loading params failed. Try again and ignore transform versions?", "Loading failed", System.Windows.MessageBoxButton.OKCancel)
                    == System.Windows.MessageBoxResult.OK)
                {
                    ifs = await IfsSerializer.LoadJsonFileAsync(path, LoadedTransforms, true);
                }
                else
                    throw;
            }
            LoadParams(ifs);
        }

        public void LoadBlankParams()
        {
            LoadParams(new IFS());
        }

        public void LoadRandomParams()
        {
            Generator g = new Generator(LoadedTransforms);
            IFS r = g.GenerateOne(new GeneratorOptions());
            r.ImageResolution = new System.Drawing.Size(1920, 1080);
            LoadParams(r);
        }

        public async Task SaveParamsFileAsync(string path)
        {
            IFS.AddAuthor(CurrentUser);
            await IfsSerializer.SaveJsonFileAsync(IFS, path);
        }

        public void UndoHistory()
        {
            //LoadParams without taking snapshot
            IFS = tracker.Undo(IFS, LoadedTransforms);
            renderer?.LoadParams(IFS);
        }

        public void RedoHistory()
        {
            //LoadParams without taking snapshot
            IFS = tracker.Redo(IFS, LoadedTransforms);
            renderer?.LoadParams(ifs);
        }

        public void ClearHistory()
        {
            tracker.Clear();
        }

        public void TakeSnapshot()
        {
            tracker.TakeSnapshot(IFS);
        }

        public void LoadUserSettings()
        {
            if (!ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).HasFile)
            {//migrate user settings from previous version
                Settings.Default.Upgrade();
                Settings.Default.Save();
            }
            Renderer.EnablePerceptualUpdates = Settings.Default.PerceptuallyUniformUpdates;
            Renderer.SetWorkgroupCount(Settings.Default.WorkgroupCount).Wait();
            Renderer.TargetFramerate = Settings.Default.TargetFramerate;
            InvertAxisX = Settings.Default.InvertAxisX;
            InvertAxisY = Settings.Default.InvertAxisY;
            InvertAxisZ = Settings.Default.InvertAxisZ;
            Sensitivity = Settings.Default.Sensitivity;
            CurrentUser = new Author
            {
                Name = Settings.Default.AuthorName,
                Link = Settings.Default.AuthorLink
            };
        }

        public void UpdateStatusText(string statusText)
        {
            StatusTextChanged?.Invoke(this, statusText);
        }

    }
}
