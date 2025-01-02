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
        var chatPageViewModel = testService.ChatPageViewModel;
        testService.LMKitService.LMKitConfig.RequestTimeout = 60;
        bool loadingSuccess = await testService.LoadModel(MaestroTestsService.Model2);
        Assert.True(loadingSuccess);

        var result = await testService.LMKitService.Translation.Translate("moitié route sur mon défunt père ???", LMKit.TextGeneration.Language.French);
        Assert.False(string.IsNullOrEmpty(result));
    }
}
