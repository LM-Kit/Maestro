using LMKit.TextGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Tests;

[Collection("Maestro Tests")]
public class TranslationTests
{
    [Fact]
    public async Task TranslateText()
    {
        var testService = new MaestroTestsService();
        testService.LMKitService.LMKitConfig.RequestTimeout = 60;
        bool loadingSuccess = await testService.LoadModel(Constants.Model2);
        Assert.True(loadingSuccess);

        var result = await testService.LMKitService.Translation.Translate("moitié route sur mon défunt père ???", LMKit.TextGeneration.Language.French);
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public async Task DetectLanguage()
    {
        var testService = new MaestroTestsService();
        testService.LMKitService.LMKitConfig.RequestTimeout = 60;
        bool loadingSuccess = await testService.LoadModel(Constants.TranslationModel);
        Assert.True(loadingSuccess);

        var result = await testService.LMKitService.Translation.DetectLanguage("l'existence du véhicule précède l'essence, qu'en est-il donc du pédoncule dans la question du sens de cette phrase ?");
        Assert.Equal(Language.French, result);
    }
}
