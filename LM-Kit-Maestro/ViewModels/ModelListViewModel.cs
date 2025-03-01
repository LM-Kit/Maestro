using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Helpers;
using LMKit.Maestro.Services;
using LMKit.Model;
using Mopups.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LMKit.Maestro.ViewModels
{
    public partial class ModelListViewModel : ViewModelBase
    {
        private readonly ILLMFileManager _fileManager;
        private readonly ILauncher _launcher;
        public LMKitService LMKitService { get; }

        public ObservableCollection<ModelInfoViewModel> Models { get; }

        [ObservableProperty] public ModelLoadingState _loadingState;

        [ObservableProperty] private double? _loadingProgress;

        private ModelInfoViewModel? _selectedModel;

        public ModelInfoViewModel? SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (value != _selectedModel)
                {
                    _selectedModel = value;
                    OnPropertyChanged();

                    if (_selectedModel != null &&
                        _selectedModel.ModelInfo.ModelUri != LMKitService.LMKitConfig.LoadedModelUri)
                    {
                        LoadModel(_selectedModel.ModelInfo.ModelUri);
                    }
                }
            }
        }

        public ModelListViewModel(ILLMFileManager fileManager, LMKitService lmKitService,
            ILauncher launcher)
        {
            _fileManager = fileManager;
            LMKitService = lmKitService;
            _launcher = launcher;
            _fileManager.SortedModelCollectionChanged += OnModelCollectionChanged;
            Models = [];

            LMKitService.ModelDownloadingProgressed += OnModelDownloadingProgressed;
            LMKitService.ModelLoadingProgressed += OnModelLoadingProgressed;
            LMKitService.ModelLoadingFailed += OnModelLoadingFailed;
            LMKitService.ModelLoaded += OnModelLoadingCompleted;
            LMKitService.PropertyChanged += OnLmKitServicePropertyChanged;
        }

        public void Initialize()
        {
            if (LMKitService.LMKitConfig.LoadedModelUri != null)
            {
                SelectedModel =
                    MaestroHelpers.TryGetExistingModelInfoViewModel(Models, LMKitService.LMKitConfig.LoadedModelUri);
            }
        }

        [RelayCommand]
        public void EjectModel()
        {
            if (SelectedModel != null)
            {
                LMKitService.UnloadModel();
                SelectedModel = null;
            }
        }

        [RelayCommand]
        public void LoadModel(Uri fileUri)
        {
            ModelCard? modelCard = null;

            foreach (var model in Models)
            {
                if (model.ModelInfo.ModelUri == fileUri)
                {
                    modelCard = model.ModelInfo;
                    break;
                }
            }

            if (modelCard != null)
            {
                LMKitService.LoadModel(fileUri);
            }
            else
            {
                //todo: display snackbar
                //_popupService.DisplayAlert("Model not found",
                //    $"This model was not found in your model folder.\nMake sure the path points to your current model folder and that the file exists on your disk: {fileUri.LocalPath}",
                //    "OK");
            }
        }

        public void OpenModelInExplorer(ModelInfoViewModel modelInfoViewModel)
        {
            string filePath = modelInfoViewModel.ModelInfo.LocalPath;

            if (File.Exists(filePath))
            {
                Task.Run(() =>
                {
#if WINDOWS
                    Process.Start("explorer.exe", $"/select,\"{filePath}\"");
#elif MACCATALYST
                    Process.Start("open", "-R " + filePath);
#endif
                });
            }
        }


        public void OpenModelHfLink(ModelInfoViewModel modelInfoViewModel)
        {
            Task.Run(() =>
            {
                _ = _launcher.OpenAsync(FileHelpers.GetModelFileHuggingFaceLink(modelInfoViewModel.ModelInfo));
            });
        }

        [RelayCommand]
        public void DeleteModel(ModelInfoViewModel modelCardViewModel)
        {
            try
            {
                _fileManager.DeleteModel(modelCardViewModel.ModelInfo);
                modelCardViewModel.OnLocalModelRemoved();
            }
            catch (Exception ex)
            {
                //todo: show snackbar
                //_popupService.DisplayAlert("Failure to delete model file",
                //    $"{ex.Message}", "OK");
            }
        }

        #region Private methods

        private void OnModelCollectionChanged(object? sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems!)
                {
                    ModelCard modelCard = (ModelCard)item;
                    AddNewModel(modelCard);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems!)
                {
                    ModelCard modelCard = (ModelCard)item;
                    RemoveExistingModel(modelCard);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                ClearUserModelList();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                int index = e.NewStartingIndex;

                foreach (var item in e.NewItems!)
                {
                    ReplaceExistingModel((ModelCard)item, index);
                    index++;
                }
            }
        }

        private void AddNewModel(ModelCard modelCard)
        {
#if BETA_DOWNLOAD_MODELS
            ModelInfoViewModel? modelCardViewModel =
 MaestroHelpers.TryGetExistingModelInfoViewModel(AvailableModels, modelCard);

            if (modelCardViewModel == null)
            {
                modelCardViewModel = new ModelInfoViewModel(modelCard);
            }

            modelCardViewModel.DownloadInfo.Status = DownloadStatus.Downloaded;
#else
            ModelInfoViewModel modelCardViewModel = new ModelInfoViewModel(modelCard);
#endif

            AddModel(modelCardViewModel);
        }

        private void AddModel(ModelInfoViewModel modelCardViewModel, bool sort = true)
        {
            if (sort)
            {
                int insertIndex = 0;

                //First sorting pass: Sort by short name in ascending order.
                while (insertIndex < Models.Count &&
                       string.Compare(Models[insertIndex].ShortName, modelCardViewModel.ShortName) < 0)
                {
                    insertIndex++;
                }

                //Second sorting pass: by model size
                while (insertIndex < Models.Count &&
                       string.Compare(Models[insertIndex].ShortName, modelCardViewModel.ShortName) == 0 &&
                       Models[insertIndex].FileSize < modelCardViewModel.FileSize)
                {
                    insertIndex++;
                }

                Models.Insert(insertIndex, modelCardViewModel);
            }
            else
            {
                Models.Add(modelCardViewModel);
            }
        }

        private void RemoveExistingModel(ModelCard modelCard)
        {
            ModelInfoViewModel? modelCardViewModel = MaestroHelpers.TryGetExistingModelInfoViewModel(Models, modelCard);

            if (modelCardViewModel != null)
            {
                modelCardViewModel.DownloadInfo.Status = DownloadStatus.NotDownloaded;
                Models.Remove(modelCardViewModel);

                if (LMKitService.LMKitConfig.LoadedModelUri == modelCardViewModel.ModelInfo.ModelUri)
                {
                    LMKitService.UnloadModel();
                }
            }
        }

        private void ReplaceExistingModel(ModelCard modelCard, int index)
        {
            ModelInfoViewModel modelCardViewModel = Models[index];

            modelCardViewModel.ModelInfo = modelCard;
        }

        private void ClearUserModelList()
        {
            Models.Clear();

#if BETA_DOWNLOAD_MODELS
            foreach (var model in AvailableModels)
            {
                model.DownloadInfo.Status = DownloadStatus.NotDownloaded;
            }

            if (_lmKitService.ModelLoadingState == LMKitModelLoadingState.Loaded)
            {
                _lmKitService.UnloadModel();
            }
#endif
        }

        private void OnModelLoadingCompleted(object? sender, EventArgs e)
        {
            SelectedModel =
                MaestroHelpers.TryGetExistingModelInfoViewModel(Models, LMKitService.LMKitConfig.LoadedModelUri!);
            LoadingProgress = 0;
            LoadingState = ModelLoadingState.FinishinUp;
        }

        private void OnModelLoadingProgressed(object? sender, EventArgs e)
        {
            var loadingEventArgs = (LMKitService.ModelLoadingProgressedEventArgs)e;

            if (LoadingState == ModelLoadingState.Downloading)
            {
                var modeUri = loadingEventArgs.FileUri;

                foreach (var userModel in Models)
                {
                    if (userModel.ModelInfo.ModelUri == modeUri)
                    {
                        userModel.OnLocalModelCreated();
                        _fileManager.OnModelDownloaded(userModel.ModelInfo);
                        break;
                    }
                }
            }

            LoadingState = LoadingProgress == 1 ? ModelLoadingState.FinishinUp : ModelLoadingState.Loading;
            LoadingProgress = loadingEventArgs.Progress;
        }

        private void OnModelDownloadingProgressed(object? sender, EventArgs e)
        {
            LoadingState = ModelLoadingState.Downloading;

            var downloadingEventArgs = (LMKitService.ModelDownloadingProgressedEventArgs)e;

            if (downloadingEventArgs.ContentLength != null)
            {
                LoadingProgress = (float)downloadingEventArgs.BytesRead / downloadingEventArgs.ContentLength.Value;
            }
        }

        private void OnModelLoadingFailed(object? sender, EventArgs e)
        {
            LoadingState = ModelLoadingState.NotLoaded;
            LoadingProgress = null;
            SelectedModel = null;
        }

        private void OnLmKitServicePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LMKitService.ModelLoadingState))
            {
                switch (LMKitService.ModelLoadingState)
                {
                    case LMKitModelLoadingState.Unloaded:
                        LoadingState = ModelLoadingState.NotLoaded;
                        break;
                    case LMKitModelLoadingState.Loading:
                        LoadingState = ModelLoadingState.Loading;
                        break;
                    case LMKitModelLoadingState.Loaded:
                        LoadingState = ModelLoadingState.Loaded;
                        break;
                }
            }
        }

        #endregion
    }
}