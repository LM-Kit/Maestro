using LMKit.Maestro.ViewModels;
using LMKit.Model;

namespace LMKit.Maestro.Helpers
{
    internal static class MaestroHelpers
    {
        public static ModelInfoViewModel? TryGetExistingModelInfoViewModel(ICollection<ModelInfoViewModel> modelCardViewModels, ModelCard modelCard)
        {
            foreach (var modelCardViewModel in modelCardViewModels)
            {
                if (modelCardViewModel.ModelInfo == modelCard)
                {
                    return modelCardViewModel;
                }
            }

            return new ModelInfoViewModel(modelCard);
        }

        public static ModelInfoViewModel? TryGetExistingModelInfoViewModel(ICollection<ModelInfoViewModel> modelCardViewModels, Uri modelFileUri)
        {
            foreach (var modelCardViewModel in modelCardViewModels)
            {
                if (modelCardViewModel.ModelInfo.ModelUri == modelFileUri)
                {
                    return modelCardViewModel;
                }
            }

            return new ModelInfoViewModel(new ModelCard(modelFileUri));
        }
    }
}
