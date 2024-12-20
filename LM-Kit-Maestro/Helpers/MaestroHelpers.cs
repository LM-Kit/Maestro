using LMKit.Maestro.Services;
using LMKit.Maestro.ViewModels;
using LMKit.Model;

namespace LMKit.Maestro.Helpers
{
    internal static class MaestroHelpers
    {
        public static ModelInfoViewModel? TryGetExistingModelInfoViewModel(ICollection<ModelInfoViewModel> modelCardViewModels, ModelCard modelCard)
        {
            foreach (var modelCardViewModel in modelCardViewModels)
            {//todo: use sha instead
                if (string.CompareOrdinal(modelCardViewModel.ModelInfo.FileName, modelCard.FileName) == 0 &&
                    string.CompareOrdinal(modelCardViewModel.ModelInfo.Repository, modelCard.Repository) == 0 &&
                    string.CompareOrdinal(modelCardViewModel.ModelInfo.Publisher, modelCard.Publisher) == 0)
                {
                    return modelCardViewModel;
                }
            }

            return null;
        }

        public static ModelInfoViewModel? TryGetExistingModelInfoViewModel(string modelsFolderPath, ICollection<ModelInfoViewModel> modelCardViewModels, Uri modelFileUri)
        {
            if (FileHelpers.GetModelInfoFromPath(modelFileUri.LocalPath, modelsFolderPath, out string publisher, out string repository, out string fileName))
            {
                foreach (var modelCardViewModel in modelCardViewModels)
                {
                    if (string.CompareOrdinal(modelCardViewModel.ModelInfo.FileName, fileName) == 0 &&
                        string.CompareOrdinal(modelCardViewModel.ModelInfo.Repository, repository) == 0 &&
                        string.CompareOrdinal(modelCardViewModel.ModelInfo.Publisher, publisher) == 0)
                    {
                        return modelCardViewModel;
                    }
                }
            }
            else
            {
                //handling unsorted models.
                foreach (var modelCardViewModel in modelCardViewModels)
                {
                    if (modelCardViewModel.ModelInfo.ModelUri == modelFileUri)
                    {
                        return modelCardViewModel;
                    }
                }
            }

            //Loïc: we have an architecture defect. We can reach this stage, especially at startup, while modelCardViewModels is not completely loaded.
            //todo Evan: fix.
            return new ModelInfoViewModel(new ModelCard()
            {
                Publisher = publisher,
                Repository = repository,
                ModelUri = modelFileUri
            });
        }
    }
}
