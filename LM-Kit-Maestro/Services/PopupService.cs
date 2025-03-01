﻿using LMKit.Maestro.UI;
using LMKit.Maestro.ViewModels;
using Mopups.Interfaces;

namespace LMKit.Maestro.Services;

internal class PopupService : IPopupService
{
    private readonly CommunityToolkit.Maui.Core.IPopupService _mctPopupService;
    private readonly IPopupNavigation _popupNavigation;

    public PopupService(CommunityToolkit.Maui.Core.IPopupService mctPopupService, IPopupNavigation popupNavigation)
    {
        _mctPopupService = mctPopupService;
        _popupNavigation = popupNavigation;
    }

    public async Task DisplayAlert(string title, string message, string okText = "OK")
    {
        AlertPopupViewModel viewModel = new AlertPopupViewModel()
        {
            Title = title,
            Message = message,
            OkText = okText
        };
    }
}