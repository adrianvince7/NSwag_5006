using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MyToolkit.Storage;
using Newtonsoft.Json;
using NSwagStudio.ViewModels;

namespace NSwagStudio.Views;

public partial class MainWindow : Window
    {
        private bool _closeCancelled = false;

        public MainWindow()
        {
            InitializeComponent();
            CheckForApplicationUpdate();
            LoadWindowState();
            RegisterFileOpenHandler();

            Title += IntPtr.Size == 4 ? " (x86), v" : " (x64), v" + Model.NSwagVersion;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        private MainWindowModel Model => (MainWindowModel)Resources["ViewModel"];
        
        private void RegisterFileOpenHandler()
        {
            var fileHandler = new FileOpenHandler();
            fileHandler.FileOpen += async (sender, args) => { await Model.OpenDocumentAsync(args.FileName); };
            fileHandler.Initialize(this);
        }

        private async void CheckForApplicationUpdate()
        {
            var updater = new ApplicationUpdater(
                "NSwagStudio.msi",
                GetType().Assembly,
                "http://rsuter.com/Projects/NSwagStudio/updates.php");

            await updater.CheckForUpdate(this);
        }

        private void LoadWindowState()
        {
            Width = ApplicationSettings.GetSetting("WindowWidth", Width);
            Height = ApplicationSettings.GetSetting("WindowHeight", Height);

            var left = ApplicationSettings.GetSetting("WindowLeft", double.NaN);
            var top = ApplicationSettings.GetSetting("WindowTop", double.NaN);

            if (!double.IsNaN(left) && !double.IsNaN(top))
            {
                Position = new PixelPoint((int)left, (int)top);
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            var windowState = ApplicationSettings.GetSetting("WindowState", WindowState.Normal);
            WindowState = windowState;
        }

        protected override async void OnClosing(WindowClosingEventArgs e)
        {
            if (!_closeCancelled)
            {
                e.Cancel = true;

                var paths = Model.Documents
                    .Where(d => System.IO.File.Exists(d.Document.Path))
                    .Select(d => d.Document.Path)
                    .ToArray();

                foreach (var document in Model.Documents.ToArray())
                {
                    var success = await Model.CloseDocumentAsync(document);
                    if (!success)
                    {
                        base.OnClosing(e);
                        return;
                    }
                }

                ApplicationSettings.SetSetting("NSwagSettings", JsonConvert.SerializeObject(paths, Formatting.Indented));

                Model.CallOnUnloaded();
                Model.Documents.Clear();

                _closeCancelled = true;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Dispatcher.UIThread.InvokeAsync(Close);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            ApplicationSettings.SetSetting("WindowWidth", Width);
            ApplicationSettings.SetSetting("WindowHeight", Height);
            ApplicationSettings.SetSetting("WindowLeft", double.NaN);
            ApplicationSettings.SetSetting("WindowTop", double.NaN);
            ApplicationSettings.SetSetting("WindowState", WindowState);
        }

        private void OnOpenHyperlink(object sender, RoutedEventArgs e)
        {
            var uri = ((HyperlinkButton)sender).NavigateUri;
            Process.Start(uri.ToString());
        }
        
        IDisposable? _selectFilesInteractionDisposable;

        protected override void OnDataContextChanged(EventArgs e)
        {
            _selectFilesInteractionDisposable?.Dispose();

            if (DataContext is MainWindowModel vm)
            {
                _selectFilesInteractionDisposable =
                    vm.SelectFilesInteraction.RegisterHandler(InteractionHandler);
            }

            base.OnDataContextChanged(e);
        }

        private async Task<string[]?> InteractionHandler(string input)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            var storageFiles = await topLevel!.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions() 
                { 
                    AllowMultiple = true, 
                    Title = input
                });

            return storageFiles?.Select(x => x.Name)?.ToArray();
        }
    }
