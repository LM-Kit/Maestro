using LMKit.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Tests
{
    internal static class Constants
    {
        public static readonly Uri Model1 =
            new(
                @"https://huggingface.co/lm-kit/llama-3.2-1b-instruct.gguf/resolve/main/Llama-3.2-1B-Instruct-Q4_K_M.gguf?download=true");

        public static readonly Uri Model2 =
            new(
                @"https://huggingface.co/lm-kit/qwen-2.5-0.5b-instruct-gguf/resolve/main/Qwen-2.5-0.5B-Instruct-Q4_K_M.gguf?download=true");

        public static readonly Uri TranslationModel =
            new Uri(
                @"https://huggingface.co/lm-kit/falcon-3-10.3b-instruct-gguf/resolve/main/Falcon3-10B-Instruct-q4_k_m.gguf?download=true");

        public static readonly ModelCard ModelCard = new ModelCard(new Uri("https://huggingface.co/lm-kit/qwen-2.5-3.1b-instruct-gguf/resolve/main/Qwen-2.5-3.1B-Instruct-Q4_K_M.gguf"));
    }
}
