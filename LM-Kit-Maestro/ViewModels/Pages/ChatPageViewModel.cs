﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Services;
namespace LMKit.Maestro.ViewModels;

public partial class ChatPageViewModel : ViewModelBase
{
    public SettingsViewModel SettingsViewModel { get; }

    [ObservableProperty]
    public partial bool ChatsSidebarIsToggled { get; set; }

    [ObservableProperty]
    public partial bool SettingsSidebarIsToggled { get; set; }

    public LMKitService LmKitService { get; }
    public ConversationListViewModel ConversationListViewModel { get; }
    public ModelListViewModel ModelListViewModel { get; }

    public ChatPageViewModel(ConversationListViewModel conversationListViewModel, ModelListViewModel modelListViewModel,
        LMKitService lmKitService, SettingsViewModel settingsViewModel)
    {
        ConversationListViewModel = conversationListViewModel;
        ModelListViewModel = modelListViewModel;
        LmKitService = lmKitService;
        SettingsViewModel = settingsViewModel;
    }

    [RelayCommand]
    public void ToggleChatsSidebar()
    {
        ChatsSidebarIsToggled = !ChatsSidebarIsToggled;
    }

    [RelayCommand]
    public void ToggleSettingsSidebar()
    {
        SettingsSidebarIsToggled = !SettingsSidebarIsToggled;
    }

    [RelayCommand]
    public void StartNewConversation()
    {
        ConversationListViewModel.AddNewConversation();
    }

    [RelayCommand]
    public async Task DeleteConversation(ConversationViewModel conversationViewModel)
    {
        await ConversationListViewModel.DeleteConversation(conversationViewModel);
    }
}