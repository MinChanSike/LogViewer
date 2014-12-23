﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogNavigatorViewModel.cs" company="Orcomp development team">
//   Copyright (c) 2008 - 2014 Orcomp development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LogViewer.ViewModels
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Catel;
    using Catel.Collections;
    using Catel.Fody;
    using Catel.MVVM;
    using Catel.Services;

    using LogViewer.Models;
    using LogViewer.Models.Base;
    using LogViewer.Services;

    using Orchestra.Services;

    public class LogNavigatorViewModel : ViewModelBase
    {
        #region Fields
        private readonly IAppDataService _appDataService;

        private readonly ICompanyService _companyService;

        private readonly IMessageService _messageService;

        private readonly ISelectDirectoryService _selectDirectoryService;

        private ObservableCollection<NavigationNode> _prevSelectedItems;
        #endregion

        #region Constructors
        public LogNavigatorViewModel(LogViewerModel logViewerModel, ISelectDirectoryService selectDirectoryService, IMessageService messageService, ICompanyService companyService, IAppDataService appDataService)
        {
            Argument.IsNotNull(() => logViewerModel);
            Argument.IsNotNull(() => selectDirectoryService);
            Argument.IsNotNull(() => messageService);
            Argument.IsNotNull(() => companyService);
            Argument.IsNotNull(() => appDataService);

            _selectDirectoryService = selectDirectoryService;
            _messageService = messageService;
            _companyService = companyService;
            _appDataService = appDataService;

            LogViewer = logViewerModel;

            AddCompanyCommand = new Command(OnAddCompanyCommandExecute);

            DeleteCompanyCommand = new Command(OnDeleteCompanyCommandExecute, CanExecuteDeleteCompanyCommand);

            SelectedItems = new ObservableCollection<NavigationNode>();
        }
        #endregion

        #region Properties
        [Model]
        [Expose("Companies")]
        public LogViewerModel LogViewer { get; set; }

        /// <summary>
        /// Gets the AddCompanyCommand command.
        /// </summary>
        public Command AddCompanyCommand { get; private set; }

        public Command DeleteCompanyCommand { get; private set; }

        [ViewModelToModel("LogViewer")]
        public ObservableCollection<NavigationNode> SelectedItems { get; set; }
        #endregion

        #region Methods
        public void OnSelectedItemsChanged()
        {
            if (_prevSelectedItems != null)
            {
                _prevSelectedItems.CollectionChanged -= OnSelectedItemsCollectionChanged;
            }
            if (SelectedItems != null)
            {
                SelectedItems.CollectionChanged += OnSelectedItemsCollectionChanged;
            }
            _prevSelectedItems = SelectedItems;
        }

        private void OnSelectedItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DeleteCompanyCommand.RaiseCanExecuteChanged();
        }

        private void OnDeleteCompanyCommandExecute()
        {
            var selectedCompany = SelectedItems.SingleOrDefault() as Company;
            if (selectedCompany != null)
            {
                LogViewer.Companies.Remove(selectedCompany);
            }
        }

        private bool CanExecuteDeleteCompanyCommand()
        {
            if (SelectedItems.Count != 1)
            {
                return false;
            }
            var selectedCompany = SelectedItems.SingleOrDefault() as Company;
            return selectedCompany != null;
        }

        protected override async Task Initialize()
        {
            if (LogViewer.Companies == null)
            {
                LogViewer.Companies = new ObservableCollection<Company>();
            }
            else
            {
                LogViewer.Companies.Clear();
            }
            LogViewer.Companies.AddRange(_companyService.LoadCompanies());
        }

        protected override void OnClosing()
        {
            _companyService.SaveCompanies(LogViewer.Companies);
            base.OnClosing();
        }

        /// <summary>
        /// Method to invoke when the AddCompanyCommand command is executed.
        /// </summary>
        private void OnAddCompanyCommandExecute()
        {
            var rootAppDataDir = _appDataService.GetRootAppDataFolder();

            _selectDirectoryService.Title = "Select Company's application data folder";
            _selectDirectoryService.InitialDirectory = rootAppDataDir;
            if (_selectDirectoryService.DetermineDirectory())
            {
                var companyFolder = _selectDirectoryService.DirectoryName;
                if (string.Equals(companyFolder, rootAppDataDir) || !companyFolder.StartsWith(rootAppDataDir))
                {
                    _messageService.ShowError(string.Format("Must be selected subdirectory of the\"{0}\"", rootAppDataDir));
                    return;
                }

                var companyName = new DirectoryInfo(companyFolder).Name;
                if (LogViewer.Companies.Any(x => string.Equals(x.Name, companyName)))
                {
                    _messageService.ShowError(string.Format("The company {0} is already added", companyName));
                    return;
                }

                var company = _companyService.CreateCompanyByName(companyName);
                if (company != null)
                {
                    LogViewer.Companies.Add(company);
                }
            }
        }
        #endregion
    }
}