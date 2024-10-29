namespace LMKitMaestro;

internal static class AppConstants
{
    public const string DatabaseFilename = "LMKitMaestroSQLite.db3";

    public const SQLite.SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLite.SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLite.SQLiteOpenFlags.SharedCache;

    public const string ChatRoute = "Chat";

    public const string ModelsRoute = "Models";

    public const double ChatMessageMaximumWidth = 800;

    public const double ChatWindowLayoutMinimumWidth = 720;

    public const double WindowMinimumWidth = 568;

    public const double WindowMinimumHeight = 600;

    public const double PopupWidth = 536;

    public const double AlertPopupWidth = 400;

    public const double AppBarHeight = 72;

    public const double ChatPageTopBarHeight = AppBarHeight + ChatPageHeaderHeight - 8;

    public const double ModelSelectionButtonMaxWidth = 568;

    public const double MinimizedHeaderButtonWidth = ChatWindowLayoutMinimumWidth - (16 * 2);

    public const double ModelSelectionButtonHeight = 48;

    public const double ChatPageHeaderHeight = ModelSelectionButtonHeight + (8 * 2);

    public const double ChatPageSidebarWidth = 300;

    public const double HeaderHorizontalMargin = 12;

    public const double ChatPageSideTogglesWidth = (ChatPageToggleButtonWidth * 2) + 16 + 8;

    public const double ChatPageToggleButtonWidth = 32;

#if BETA_DOWNLOAD_MODELS
    //LM-Kit models catalog: https://huggingface.co/lm-kit
    public static readonly ModelInfo[] AvailableModels =
    {
        new ModelInfo(
            @"https://huggingface.co/lm-kit/llama-3-8b-instruct-gguf/resolve/main/Llama-3-8B-Instruct-Q4_K_M.gguf",
            new ModelInfo.ModelMetadata()
            {
                FileSize = 4920733952
            }),

        new ModelInfo(
            @"https://huggingface.co/lm-kit/LM-Kit.Sentiment_Analysis-TinyLlama-1.1B-1T-OpenOrca-en-q4-gguf/resolve/main/LM-Kit.Sentiment_Analysis-TinyLlama-1.1B-1T-OpenOrca-en-q4.gguf",
            new ModelInfo.ModelMetadata()
            {
            }),

        new ModelInfo(
            @"https://huggingface.co/lm-kit/LM-Kit.Sarcasm_Detection-TinyLlama-1.1B-1T-OpenOrca-en-q4-gguf/resolve/main/LM-Kit.Sarcasm_Detection-TinyLlama-1.1B-1T-OpenOrca-en-q4.gguf",
            new ModelInfo.ModelMetadata()
            {
            }),

        new ModelInfo(
            @"https://huggingface.co/lm-kit/bge-1.5-gguf/resolve/main/bge-small-en-v1.5-f16.gguf",
            new ModelInfo.ModelMetadata()
            {
            }),

        new ModelInfo(
            @"https://huggingface.co/lm-kit/mistral-0.1-openorca-7b-gguf/resolve/main/Mistral-0.1-OpenOrca-7B-Q4_K_M.gguf",
            new ModelInfo.ModelMetadata()
            {
            }),

        new ModelInfo(
            @"https://huggingface.co/lm-kit/mistral-0.3-7b-gguf/resolve/main/Mistral-0.3-7B-Q4_K_M.gguf",
            new ModelInfo.ModelMetadata()
            {
            }),

        new ModelInfo(
            @"https://huggingface.co/lm-kit/deepseek-coder-1.6-7b-gguf/resolve/main/DeepSeek-Coder-1.6-7B-Instruct-Q4_K_M.gguf",
            new ModelInfo.ModelMetadata()
            {
            }),

        new ModelInfo(
            @"https://huggingface.co/lm-kit/deepseek-coder-2-lite-15.7b-gguf/resolve/main/DeepSeek-Coder-2-Lite-15.7B-Instruct-Q4_K_M.gguf",
            new ModelInfo.ModelMetadata()
            {
            }),
        //https://huggingface.co/lm-kit/llama-3.1-8b-instruct-gguf
        //https://huggingface.co/lm-kit/phi-3.1-mini-4k-3.8b-instruct-gguf/tree/main
        //https://huggingface.co/lm-kit/gemma-2-2b-gguf/tree/main
        //https://huggingface.co/lm-kit/phi-3-medium-4k-14b-instruct-gguf/tree/main
        //https://huggingface.co/lm-kit/mistral-0.3-7b-instruct-gguf/tree/main
        //https://huggingface.co/lm-kit/mistral-nemo-2407-12.2b-instruct-gguf/tree/main
        //https://huggingface.co/lm-kit/qwen-2-7b-instruct-gguf/tree/main
        //https://huggingface.co/lm-kit/gemma-2-9b-gguf/tree/main
    };
#endif
}