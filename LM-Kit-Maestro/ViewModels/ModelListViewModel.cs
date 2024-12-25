﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Helpers;
using LMKit.Maestro.Services;
using LMKit.Model;
using Mopups.Interfaces;
using System.Collections.ObjectModel;

namespace LMKit.Maestro.ViewModels
{
    public partial class ModelListViewModel : ViewModelBase
    {
        private readonly IMainThread _mainThread;
        private readonly ILLMFileManager _fileManager;
        private readonly IPopupService _popupService;

        public IPopupNavigation PopupNavigation { get; }
        public INavigationService NavigationService { get; }
        public LMKitService LMKitService { get; }

        public ObservableCollection<ModelInfoViewModel> Models { get; }


        [ObservableProperty]
        private double _loadingProgress;

        [ObservableProperty]
        private bool _modelLoadingIsFinishingUp;

        [ObservableProperty]
        private bool _modelIsDownloading;


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

                    if (_selectedModel != null && _selectedModel.ModelInfo.ModelUri != LMKitService.LMKitConfig.LoadedModelUri)
                    {
                        LoadModel(_selectedModel.ModelInfo.ModelUri);
                    }
                }
            }
        }

        public ModelListViewModel(IMainThread mainThread, ILLMFileManager fileManager, LMKitService lmKitService, IPopupService popupService,
            INavigationService navigationService, IPopupNavigation popupNavigation)
        {
            _mainThread = mainThread;
            _fileManager = fileManager;
            LMKitService = lmKitService;
            _popupService = popupService;
            NavigationService = navigationService;
            PopupNavigation = popupNavigation;
            _fileManager.SortedModelCollectionChanged += OnModelCollectionChanged;
            Models = new ObservableCollection<ModelInfoViewModel>();

            LMKitService.ModelDownloadingProgressed += OnModelDownloadingProgressed;
            LMKitService.ModelLoadingProgressed += OnModelLoadingProgressed;
            LMKitService.ModelLoadingFailed += OnModelLoadingFailed;
            LMKitService.ModelLoadingCompleted += OnModelLoadingCompleted;
        }

        public void Initialize()
        {
            if (LMKitService.LMKitConfig.LoadedModelUri != null)
            {
                SelectedModel = MaestroHelpers.TryGetExistingModelInfoViewModel(Models, LMKitService.LMKitConfig.LoadedModelUri);
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
                _popupService.DisplayAlert("Model not found",
                    $"This model was not found in your model folder.\nMake sure the path points to your current model folder and that the file exists on your disk: {fileUri.LocalPath}",
                    "OK");
            }
        }

        private void OnModelCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            ModelInfoViewModel? modelCardViewModel = MaestroHelpers.TryGetExistingModelInfoViewModel(AvailableModels, modelCard);

            if (modelCardViewModel == null)
            {
                modelCardViewModel = new ModelInfoViewModel(modelCard);
            }

            modelCardViewModel.DownloadInfo.Status = DownloadStatus.Downloaded;
#else
            ModelInfoViewModel modelCardViewModel = new ModelInfoViewModel(modelCard);
#endif

            _mainThread.BeginInvokeOnMainThread(() => AddModel(modelCardViewModel));
        }

        private void AddModel(ModelInfoViewModel modelCardViewModel, bool sort = true)
        {
            if (sort)
            {
                int insertIndex = 0;

                while (insertIndex < Models.Count && string.Compare(Models[insertIndex].Name, modelCardViewModel.Name) < 0)
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
                _mainThread.BeginInvokeOnMainThread(() => Models.Remove(modelCardViewModel));

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
            SelectedModel = MaestroHelpers.TryGetExistingModelInfoViewModel(Models, LMKitService.LMKitConfig.LoadedModelUri!);
            LoadingProgress = 0;
            ModelLoadingIsFinishingUp = false;
        }

        private void OnModelLoadingProgressed(object? sender, EventArgs e)
        {
            var loadingEventArgs = (LMKitService.ModelLoadingProgressedEventArgs)e;

            if (ModelIsDownloading)
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

                ModelIsDownloading = false;
            }
            LoadingProgress = loadingEventArgs.Progress;
            ModelLoadingIsFinishingUp = LoadingProgress == 1;
        }

        private void OnModelDownloadingProgressed(object? sender, EventArgs e)
        {
            ModelIsDownloading = true;

            var downloadingEventArgs = (LMKitService.ModelDownloadingProgressedEventArgs)e;

            if (downloadingEventArgs.ContentLength != null)
            {
                LoadingProgress = (float)downloadingEventArgs.BytesRead / downloadingEventArgs.ContentLength.Value;
            }
        }

        private void OnModelLoadingFailed(object? sender, EventArgs e)
        {
            SelectedModel = null;
        }
    }
}