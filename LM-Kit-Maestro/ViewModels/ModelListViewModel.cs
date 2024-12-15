using LMKit.Maestro.Services;
using LMKit.Maestro.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Interfaces;

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

        private ObservableCollection<ModelInfoViewModel> _userModels = new ObservableCollection<ModelInfoViewModel>();

        [ObservableProperty]
        long _totalModelSize;

        [ObservableProperty]
        private double _loadingProgress;

        [ObservableProperty]
        private bool _modelLoadingIsFinishingUp;

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

                    if (_selectedModel != null && _selectedModel.ModelInfo.FileUri != LMKitService.LMKitConfig.LoadedModelUri)
                    {
                        LoadModel(_selectedModel.ModelInfo.FileUri);
                    }
                }
            }
        }

        public ReadOnlyObservableCollection<ModelInfoViewModel> UserModels { get; }

        public ModelListViewModel(IMainThread mainThread, ILLMFileManager fileManager, LMKitService lmKitService, IPopupService popupService,
            INavigationService navigationService, IPopupNavigation popupNavigation)
        {
            _mainThread = mainThread;
            _fileManager = fileManager;
            LMKitService = lmKitService;
            _popupService = popupService;
            NavigationService = navigationService;
            PopupNavigation = popupNavigation;
            _fileManager.UserModels.CollectionChanged += OnUserModelsCollectionChanged;
            UserModels = new ReadOnlyObservableCollection<ModelInfoViewModel>(_userModels);

            LMKitService.ModelLoadingProgressed += OnModelLoadingProgressed;
            LMKitService.ModelLoadingFailed += OnModelLoadingFailed;
            LMKitService.ModelLoadingCompleted += OnModelLoadingCompleted;
        }

        public void Initialize()
        {
            if (LMKitService.LMKitConfig.LoadedModelUri != null)
            {
                SelectedModel = MaestroHelpers.TryGetExistingModelInfoViewModel(_fileManager.ModelsFolderPath, UserModels, LMKitService.LMKitConfig.LoadedModelUri);
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
            ModelInfo? modelInfo = null;

            foreach (var model in UserModels)
            {
                if (model.ModelInfo.FileUri == fileUri)
                {
                    modelInfo = model.ModelInfo;
                    break;
                }
            }

            if (modelInfo != null)
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

        private void OnUserModelsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems!)
                {
                    ModelInfo modelInfo = (ModelInfo)item;
                    AddNewModel(modelInfo);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems!)
                {
                    ModelInfo modelInfo = (ModelInfo)item;
                    RemoveExistingModel(modelInfo);
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
                    ReplaceExistingModel((ModelInfo)item, index);
                    index++;
                }
            }
        }

        private void AddNewModel(ModelInfo modelInfo)
        {
#if BETA_DOWNLOAD_MODELS
            ModelInfoViewModel? modelInfoViewModel = MaestroHelpers.TryGetExistingModelInfoViewModel(AvailableModels, modelInfo);

            if (modelInfoViewModel == null)
            {
                modelInfoViewModel = new ModelInfoViewModel(modelInfo);
            }

            modelInfoViewModel.DownloadInfo.Status = DownloadStatus.Downloaded;
#else
            ModelInfoViewModel modelInfoViewModel = new ModelInfoViewModel(modelInfo);
#endif

            _mainThread.BeginInvokeOnMainThread(() => AddModel(modelInfoViewModel));
            TotalModelSize += modelInfoViewModel.FileSize;
        }

        private void AddModel(ModelInfoViewModel modelInfoViewModel, bool sort = true)
        {
            if (sort)
            {
                int insertIndex = 0;

                while (insertIndex < UserModels.Count && string.Compare(_userModels[insertIndex].Name, modelInfoViewModel.Name) < 0)
                {
                    insertIndex++;
                }

                _userModels.Insert(insertIndex, modelInfoViewModel);
            }
            else
            {
                _userModels.Add(modelInfoViewModel);
            }
        }

        private void RemoveExistingModel(ModelInfo modelInfo)
        {
            ModelInfoViewModel? modelInfoViewModel = MaestroHelpers.TryGetExistingModelInfoViewModel(UserModels, modelInfo);

            if (modelInfoViewModel != null)
            {
                modelInfoViewModel.DownloadInfo.Status = DownloadStatus.NotDownloaded;
                _mainThread.BeginInvokeOnMainThread(() => _userModels.Remove(modelInfoViewModel));

                TotalModelSize -= modelInfoViewModel.FileSize;

                if (LMKitService.LMKitConfig.LoadedModelUri == modelInfoViewModel.ModelInfo.FileUri)
                {
                    LMKitService.UnloadModel();
                }
            }
        }

        private void ReplaceExistingModel(ModelInfo modelInfo, int index)
        {
            ModelInfoViewModel modelInfoViewModel = UserModels[index];

            modelInfoViewModel.ModelInfo = modelInfo;
        }

        private void ClearUserModelList()
        {
            TotalModelSize = 0;
            _userModels.Clear();

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
            SelectedModel = MaestroHelpers.TryGetExistingModelInfoViewModel(_fileManager.ModelsFolderPath, UserModels, LMKitService.LMKitConfig.LoadedModelUri!);
            LoadingProgress = 0;
            ModelLoadingIsFinishingUp = false;
        }

        private void OnModelLoadingProgressed(object? sender, EventArgs e)
        {
            var loadingEventArgs = (LMKitService.ModelLoadingProgressedEventArgs)e;

            LoadingProgress = loadingEventArgs.Progress;
            ModelLoadingIsFinishingUp = LoadingProgress == 1;
        }

        private void OnModelLoadingFailed(object? sender, EventArgs e)
        {
            SelectedModel = null;
        }
    }
}
