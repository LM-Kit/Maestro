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
                if (modelCardViewModel.ModelCard == modelCard)
                {
                    return modelCardViewModel;
                }
            }

            return null;
        }

        public static ModelInfoViewModel? TryGetExistingModelInfoViewModel(ICollection<ModelInfoViewModel> modelCardViewModels, Uri modelFileUri)
        {
            foreach (var modelCardViewModel in modelCardViewModels)
            {
                if (modelCardViewModel.ModelCard.ModelUri == modelFileUri)
                {
                    return modelCardViewModel;
                }
            }

            //Loïc: we have an architecture defect. We can reach this stage, especially at startup, while modelCardViewModels is not completely loaded.
            //todo Evan: fix.
            return new ModelInfoViewModel(new ModelCard(modelFileUri));
        }
    }
}
