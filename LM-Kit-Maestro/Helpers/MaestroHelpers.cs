using LMKit.Maestro.Services;
using LMKit.Maestro.ViewModels;

namespace LMKit.Maestro.Helpers
{
    internal static class MaestroHelpers
    {
        public static ModelInfoViewModel? TryGetExistingModelInfoViewModel(ICollection<ModelInfoViewModel> modelInfoViewModels, ModelInfo modelInfo)
        {
            foreach (var modelInfoViewModel in modelInfoViewModels)
            {
                if (string.CompareOrdinal(modelInfoViewModel.ModelInfo.FileName, modelInfo.FileName) == 0 &&
                    string.CompareOrdinal(modelInfoViewModel.ModelInfo.Repository, modelInfo.Repository) == 0 &&
                    string.CompareOrdinal(modelInfoViewModel.ModelInfo.Publisher, modelInfo.Publisher) == 0)
                {
                    return modelInfoViewModel;
                }
            }

            return null;
        }

        public static ModelInfoViewModel? TryGetExistingModelInfoViewModel(ICollection<ModelInfoViewModel> modelInfoViewModels, Uri modelFileUri)
        {
            if (FileHelpers.GetModelInfoFromPath(modelFileUri.LocalPath, out string publisher, out string repository, out string fileName))
            {
                foreach (var modelInfoViewModel in modelInfoViewModels)
                {
                    if (string.CompareOrdinal(modelInfoViewModel.ModelInfo.FileName, fileName) == 0 &&
                        string.CompareOrdinal(modelInfoViewModel.ModelInfo.Repository, repository) == 0 &&
                        string.CompareOrdinal(modelInfoViewModel.ModelInfo.Publisher, publisher) == 0)
                    {
                        return modelInfoViewModel;
                    }
                }
            }

            return null;
        }
    }
}
