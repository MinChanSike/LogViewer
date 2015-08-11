﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsGeneralCommandContainer.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LogViewer
{
    using System;
    using System.Linq;
    using Catel;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Reflection;
    using Catel.Services;

    public class SettingsGeneralCommandContainer: CommandContainerBase
    {
        private const string ViewModelType = "SettingsViewModel";

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IViewModelFactory _viewModelFactory;

        public SettingsGeneralCommandContainer(ICommandManager commandManager, IUIVisualizerService uiVisualizerService, IViewModelFactory viewModelFactory)
            : base(Commands.Settings.General, commandManager)
        {
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => viewModelFactory);

            _uiVisualizerService = uiVisualizerService;
            _viewModelFactory = viewModelFactory;
        }

        protected override void Execute(object parameter)
        {
            base.Execute(parameter);

            var settingsViewModelType = TypeCache.GetTypes(x => string.Equals(x.Name, ViewModelType)).FirstOrDefault();
            if (settingsViewModelType == null)
            {
                Log.ErrorAndThrowException<InvalidOperationException>("Cannot find type '{0}'", ViewModelType);
                return;
            }

            var viewModel = _viewModelFactory.CreateViewModel(settingsViewModelType, null);

            _uiVisualizerService.ShowDialog(viewModel);
        }
    }
}