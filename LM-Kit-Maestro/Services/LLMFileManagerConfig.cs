using LMKit.Model;

namespace LMKit.Maestro.Services;

public class LLMFileManagerConfig
{
    public List<ModelCapabilities> FilteredCapabilities { get; set; } = [ModelCapabilities.Chat, ModelCapabilities.Math, ModelCapabilities.CodeCompletion];
    public bool EnablePredefinedModels { get; set; } = true;
    public bool EnableCustomModels { get; set; } = true;
}
