﻿using LMKit.Maestro.Data;
using LMKit.Maestro.Models;
using LMKit.Maestro.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LMKit.Maestro.ViewModels
{
    public partial class ConversationListViewModel : ViewModelBase
    {
        private readonly ILogger<ConversationListViewModel> _logger;
        private readonly IMaestroDatabase _database;
        private readonly LMKitService _lmKitService;

        private ConversationViewModel? _currentConversation;

        public ConversationViewModel? CurrentConversation
        {
            get => _currentConversation;
            set
            {
                // Note Evan: null-check is a workaround for https://github.com/dotnet/maui/issues/15718
                if (value != null && value != _currentConversation)
                {
                    if (_currentConversation != null)
                    {
                        _currentConversation.IsSelected = false;
                    }

                    _currentConversation = value;
                    _currentConversation.IsSelected = true;

                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? ConversationPropertyChanged;

        public ObservableCollection<ConversationViewModel> Conversations { get; } =
            [];

        public ConversationListViewModel(ILogger<ConversationListViewModel> logger, IMaestroDatabase database, LMKitService lmKitService)
        {
            _logger = logger;
            _database = database;
            _lmKitService = lmKitService;

            Conversations.CollectionChanged += OnConversationCollectionChanged;
        }

        public async Task LoadConversationLogs()
        {
            List<ConversationLog> conversations = [];
            List<ConversationViewModel> loadedConversationViewModels = [];

            try
            {
                conversations.AddRange(await _database.GetConversations());
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to fetch conversations from database");
                return;
            }

            foreach (var conversation in conversations)
            {
                ConversationViewModel conversationViewModel = new ConversationViewModel(_lmKitService, _database, conversation);

                if (conversation.ChatHistoryData != null)
                {
                    if (!conversationViewModel.LoadConversationLog())
                    {
                        _logger.LogWarning($"Failed to restore conversation chat history - ID:  {conversationViewModel.ConversationLog.ID}");
                        continue;
                    }

                    loadedConversationViewModels.Insert(0, conversationViewModel);
                }
            }

            foreach (var loadedConversationViewModel in loadedConversationViewModels)
            {
                Conversations.Add(loadedConversationViewModel);
            }

            if (Conversations.Count == 0)
            {
                Conversations.Add(new ConversationViewModel(_lmKitService, _database));
            }

            CurrentConversation = Conversations.First();
        }

        public void AddNewConversation()
        {
            var newConversation = new ConversationViewModel(_lmKitService, _database);
            Conversations.Insert(0, newConversation);
            CurrentConversation = newConversation;
        }

        public async Task DeleteConversation(ConversationViewModel conversationViewModel)
        {
            if (Conversations.Count != 1 || (Conversations.Count == 1 && !Conversations[0].IsEmpty))
            {
                Conversations.Remove(conversationViewModel);
            }
            else
            {
                return;
            }

            var deletionTask = conversationViewModel.Delete();

            try
            {
                await _database.DeleteConversation(conversationViewModel.ConversationLog);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to delete conversation from database");
                return;
            }

            await deletionTask;

            if (conversationViewModel == CurrentConversation)
            {
                if (Conversations.Count == 0)
                {
                    AddNewConversation();
                }

                CurrentConversation = Conversations.First();
            }
        }

        private void OnConversationCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    ((ConversationViewModel)item).PropertyChanged += OnConversationPropertyChanged;
                }

                if (Conversations.Count == 1)
                {
                    CurrentConversation = Conversations[0];

                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var item in e.OldItems)
                    {
                        ((ConversationViewModel)item).PropertyChanged -= OnConversationPropertyChanged;
                    }
                }
            }
        }

        private void OnConversationPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ConversationPropertyChanged?.Invoke(sender, e);
        }
    }
}