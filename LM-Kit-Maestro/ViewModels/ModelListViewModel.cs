using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Helpers;
using LMKit.Maestro.Services;
using LMKit.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LMKit.Maestro.ViewModels
{
    public partial class ModelListViewModel : ViewModelBase
    {
        private readonly ILLMFileManager _fileManager;
        private readonly ILauncher _launcher;
        private readonly ISnackbarService _snackbarService;

        public LMKitService LMKitService { get; }
        public ModelsSettingsViewModel ModelsSettings { get; }
        public ObservableCollection<ModelCardViewModel> Models { get; }

        [ObservableProperty] public ModelLoadingState _loadingState;
        [ObservableProperty] private double? _loadingProgress;

        private ModelCardViewModel? _selectedModel;
        public ModelCardViewModel? SelectedModel
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

        public ModelListViewModel(ModelsSettingsViewModel modelsSettingsViewModel, ILLMFileManager fileManager, LMKitService lmKitService,
            ILauncher launcher, ISnackbarService snackbarService)
        {
            ModelsSettings = modelsSettingsViewModel;
            _fileManager = fileManager;
            LMKitService = lmKitService;
            _launcher = launcher;
            _snackbarService = snackbarService;

            _fileManager.ModelsCollectionChanged += OnModelCollectionChanged;
            Models = [];

            LMKitService.ModelDownloadingProgressed += OnModelDownloadingProgressed;
            LMKitService.ModelLoadingProgressed += OnModelLoadingProgressed;
            LMKitService.ModelLoadingFailed += OnModelLoadingFailed;
            LMKitService.ModelLoaded += OnModelLoadingCompleted;
            LMKitService.PropertyChanged += OnLmKitServicePropertyChanged;
        }

        public void Initialize()
        {
            foreach (var model in _fileManager.Models)
            {
                if (TryGetExistingModelCardViewModel(Models, model) == null)
                {
                    AddModel(new ModelCardViewModel(model));
                }
            }

            if (LMKitService.LMKitConfig.LoadedModelUri != null)
            {
                SelectedModel = TryGetExistingModelCardViewModel(Models, LMKitService.LMKitConfig.LoadedModelUri);
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
                _snackbarService.Show("Model file not found",
                    $"Make sure the file path points to your current model folder and that it exists: {fileUri.LocalPath}");
            }
        }

        public void OpenModelInExplorer(ModelCardViewModel modelCardViewModel)
        {
            string filePath = modelCardViewModel.ModelInfo.LocalPath;

            if (File.Exists(filePath))
            {
                Task.Run(() =>
                {
#if WINDOWS
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
#elif MACCATALYST
                    System.Diagnostics.Process.Start("open", "-R " + filePath);
#endif
                });
            }
        }

        public void OpenModelHfLink(ModelCardViewModel modelCardViewModel)
        {
            Task.Run(() =>
            {
                _ = _launcher.OpenAsync(FileHelpers.GetModelFileHuggingFaceLink(modelCardViewModel.ModelInfo));
            });
        }

        [RelayCommand]
        public void DeleteModel(ModelCardViewModel modelCardViewModel)
        {
            try
            {
                _fileManager.DeleteModel(modelCardViewModel.ModelInfo);
                modelCardViewModel.OnLocalModelRemoved();
            }
            catch (Exception ex)
            {
                _snackbarService.Show("Could not delete model file", ex.Message);
            }
        }

        #region Private methods

        private void OnAppSettingsServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(IAppSettingsService.EnableLowPerformanceModels))
            //{

            //    _ = CollectModelsAsync();
            //}
            //else if (e.PropertyName == nameof(IAppSettingsService.ShowOnlyLocalModels))
            //{

            //}
        }

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

        //private void FilterModelsList()
        //{
        //    List<ModelCardViewModel> filteredList = new List<ModelCardViewModel>();

        //    foreach (var model in Models)
        //    {
        //        if (!model.IsLocallyAvailable && !_settingsViewModel)
        //        {
        //            filteredList.Add(model);
        //        }
        //    }
        //}

        private void AddNewModel(ModelCard modelCard)
        {
#if BETA_DOWNLOAD_MODELS
            ModelCardViewModel? modelCardViewModel =
 MaestroHelpers.TryGetExistingModelCardViewModel(AvailableModels, modelCard);

            if (modelCardViewModel == null)
            {
                modelCardViewModel = new ModelCardViewModel(modelCard);
            }

            modelCardViewModel.DownloadInfo.Status = DownloadStatus.Downloaded;
#else
            ModelCardViewModel modelCardViewModel = new ModelCardViewModel(modelCard);
#endif

            AddModel(modelCardViewModel);
        }

        private void AddModel(ModelCardViewModel modelCardViewModel, bool sort = true)
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
            ModelCardViewModel? modelCardViewModel = TryGetExistingModelCardViewModel(Models, modelCard);

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
            ModelCardViewModel modelCardViewModel = Models[index];

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
            SelectedModel = TryGetExistingModelCardViewModel(Models, LMKitService.LMKitConfig.LoadedModelUri!);
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

        private static ModelCardViewModel? TryGetExistingModelCardViewModel(ICollection<ModelCardViewModel> modelCardViewModels, ModelCard modelCard)
        {
            foreach (var modelCardViewModel in modelCardViewModels)
            {
                if (modelCardViewModel.ModelInfo == modelCard)
                {
                    return modelCardViewModel;
                }
            }

            return null;
        }

        private static ModelCardViewModel? TryGetExistingModelCardViewModel(ICollection<ModelCardViewModel> modelCardViewModels, Uri modelFileUri)
        {
            foreach (var modelCardViewModel in modelCardViewModels)
            {
                if (modelCardViewModel.ModelInfo.ModelUri == modelFileUri)
                {
                    return modelCardViewModel;
                }
            }

            return null;
        }
        #endregion
    }
}