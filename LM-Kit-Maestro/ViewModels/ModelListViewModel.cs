﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Helpers;
using LMKit.Maestro.Services;
using LMKit.Model;
using System.Collections.ObjectModel;
using static LMKit.Maestro.Services.LLMFileManager;

namespace LMKit.Maestro.ViewModels
{
    public partial class ModelListViewModel : ViewModelBase
    {
        private readonly ILLMFileManager _fileManager;
        private readonly ILauncher _launcher;
        private readonly ISnackbarService _snackbarService;

        public LMKitService LMKitService { get; }

        public ObservableCollection<ModelInfoViewModel> ModelDownloads { get; } = new ObservableCollection<ModelInfoViewModel>();

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
                        _selectedModel.ModelCard.ModelUri != LMKitService.LMKitConfig.LoadedModelUri)
                    {
                        HandleModelSelectionChanged(_selectedModel);
                    }
                }
            }
        }

        public ModelListViewModel(ILLMFileManager fileManager, LMKitService lmKitService,
            ILauncher launcher, ISnackbarService snackbarService)
        {
            _fileManager = fileManager;
            LMKitService = lmKitService;
            _launcher = launcher;
            _snackbarService = snackbarService;

            _fileManager.SortedModelCollectionChanged += OnModelCollectionChanged;
            Models = [];

            _fileManager.ModelDownloadingProgressed += OnModelDownloadingProgressed;
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

        public void EjectModel()
        {
            if (SelectedModel != null)
            {
                LMKitService.UnloadModel();
                SelectedModel = null;
            }
        }

        public void LoadModel(Uri fileUri)
        {
            ModelCard? modelCard = null;

            foreach (var model in Models)
            {
                if (model.ModelCard.ModelUri == fileUri)
                {
                    modelCard = model.ModelCard;
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

        public void OpenModelInExplorer(ModelInfoViewModel modelInfoViewModel)
        {
            string filePath = modelInfoViewModel.ModelCard.LocalPath;

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


        public void OpenModelHfLink(ModelInfoViewModel modelInfoViewModel)
        {
            Task.Run(() =>
            {
                _ = _launcher.OpenAsync(FileHelpers.GetModelFileHuggingFaceLink(modelInfoViewModel.ModelCard));
            });
        }

        public void DeleteModel(ModelInfoViewModel modelCardViewModel)
        {
            try
            {
                _fileManager.DeleteModel(modelCardViewModel.ModelCard);
                modelCardViewModel.OnLocalModelRemoved();
            }
            catch (Exception ex)
            {
                _snackbarService.Show("Could not delete model file", ex.Message);
            }
        }

        #region Private methods

        private void HandleModelSelectionChanged(ModelInfoViewModel modelCardViewModel)
        {
            if (modelCardViewModel.IsLocallyAvailable)
            {
                LoadModel(modelCardViewModel.ModelCard.ModelUri);
            }
            else
            {
                // todo: DialogService.ConfirmDownload
                _fileManager.DownloadModel(modelCardViewModel.ModelCard);
                ModelDownloads.Add(modelCardViewModel);
            }

            SelectedModel = modelCardViewModel;
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

        private void AddNewModel(ModelCard modelCard)
        {
            AddModel(new ModelInfoViewModel(modelCard));
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

                if (LMKitService.LMKitConfig.LoadedModelUri == modelCardViewModel.ModelCard.ModelUri)
                {
                    LMKitService.UnloadModel();
                }
            }
        }

        private void ReplaceExistingModel(ModelCard modelCard, int index)
        {
            ModelInfoViewModel modelCardViewModel = Models[index];

            modelCardViewModel.ModelCard = modelCard;
        }

        private void ClearUserModelList()
        {
            Models.Clear();
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
                    if (userModel.ModelCard.ModelUri == modeUri)
                    {
                        userModel.OnLocalModelCreated();
                        _fileManager.OnModelDownloaded(userModel.ModelCard);
                        break;
                    }
                }
            }

            LoadingState = LoadingProgress == 1 ? ModelLoadingState.FinishinUp : ModelLoadingState.Loading;
            LoadingProgress = loadingEventArgs.Progress;
        }

        private void OnModelDownloadingProgressed(object? sender, EventArgs e)
        {
            var downloadingEventArgs = (DownloadOperationStateChangedEventArgs)e;

            ModelInfoViewModel? modelViewModel = MaestroHelpers.TryGetExistingModelInfoViewModel(ModelDownloads, downloadingEventArgs.ModelCard);

            if (modelViewModel != null)
            {
                if (downloadingEventArgs.Progress != 1)
                {
                    if (modelViewModel.DownloadInfo.Status != DownloadStatus.Downloading)
                    {
                        modelViewModel.DownloadInfo.Status = DownloadStatus.Downloading;
                    }

                    modelViewModel.DownloadInfo.BytesRead = downloadingEventArgs.BytesRead;
                    modelViewModel.DownloadInfo.Progress = downloadingEventArgs.Progress;
                }
                else
                {
                    modelViewModel.DownloadInfo.Status = DownloadStatus.Downloaded;
                }
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