﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;

namespace TaskConqueror
{
    /// <summary>
    /// Represents a container of ProjectViewModel objects
    /// that has support for staying synchronized with the
    /// database.
    /// </summary>
    public class AllProjectsViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly ProjectData _projectData;
        readonly TaskData _taskData;
        RelayCommand _newCommand;
        RelayCommand _editCommand;
        RelayCommand _deleteCommand;

        #endregion // Fields

        #region Constructor

        public AllProjectsViewModel(ProjectData projectData, TaskData taskData)
        {
            if (projectData == null)
                throw new ArgumentNullException("projectData");

            base.DisplayName = Properties.Resources.Projects_DisplayName;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/project.png";

            _projectData = projectData;
            _taskData = taskData;

            // Subscribe for notifications of when a new project is saved.
            _projectData.ProjectAdded += this.OnProjectAdded;
            _projectData.ProjectUpdated += this.OnProjectUpdated;
            _projectData.ProjectDeleted += this.OnProjectDeleted;

            // Populate the AllProjects collection with ProjectViewModels.
            this.CreateAllProjects();              
        }

        void CreateAllProjects()
        {
            List<ProjectViewModel> all =
                (from project in _projectData.GetProjects()
                 select new ProjectViewModel(project, _projectData, _taskData)).ToList();

            foreach (ProjectViewModel pvm in all)
                pvm.PropertyChanged += this.OnProjectViewModelPropertyChanged;

            this.AllProjects = new ObservableCollection<ProjectViewModel>(all);
            this.AllProjects.CollectionChanged += this.OnCollectionChanged;
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the ProjectViewModel objects.
        /// </summary>
        public ObservableCollection<ProjectViewModel> AllProjects { get; private set; }

        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (ProjectViewModel projectVM in this.AllProjects)
                projectVM.Dispose();

            this.AllProjects.Clear();
            this.AllProjects.CollectionChanged -= this.OnCollectionChanged;

            _projectData.ProjectAdded -= this.OnProjectAdded;
            _projectData.ProjectUpdated -= this.OnProjectUpdated;
            _projectData.ProjectDeleted -= this.OnProjectDeleted;
        }

        #endregion // Base Class Overrides

        #region Event Handling Methods

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (ProjectViewModel projectVM in e.NewItems)
                    projectVM.PropertyChanged += this.OnProjectViewModelPropertyChanged;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (ProjectViewModel projectVM in e.OldItems)
                    projectVM.PropertyChanged -= this.OnProjectViewModelPropertyChanged;
        }

        void OnProjectViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string IsSelected = "IsSelected";

            // Make sure that the property name we're referencing is valid.
            // This is a debugging technique, and does not execute in a Release build.
            (sender as ProjectViewModel).VerifyPropertyName(IsSelected);

        }

        void OnProjectAdded(object sender, ProjectAddedEventArgs e)
        {
            var viewModel = new ProjectViewModel(e.NewProject, _projectData, _taskData);
            this.AllProjects.Add(viewModel);
        }

        void OnProjectUpdated(object sender, ProjectUpdatedEventArgs e)
        {
            this.AllProjects.Remove(this.AllProjects.FirstOrDefault(p => p.ProjectId == e.UpdatedProject.ProjectId));
            var viewModel = new ProjectViewModel(e.UpdatedProject, _projectData, _taskData);
            this.AllProjects.Add(viewModel);
        }

        void OnProjectDeleted(object sender, ProjectDeletedEventArgs e)
        {
            using (var viewModel = this.AllProjects.FirstOrDefault(p => p.ProjectId == e.DeletedProject.ProjectId))
            {
                this.AllProjects.Remove(viewModel);
            }
        }

        #endregion // Event Handling Methods

        #region Commands

        /// <summary>
        /// Returns a command that creates a new project.
        /// </summary>
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand(
                        param => this.CreateProject()
                        );
                }
                return _newCommand;
            }
        }

        /// <summary>
        /// Returns a command that edits an existing project.
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(
                        param => this.EditProject(),
                        param => this.CanEditProject()
                        );
                }
                return _editCommand;
            }
        }

        /// <summary>
        /// Returns a command that deletes an existing project.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(
                        param => this.DeleteProject(),
                        param => this.CanDeleteProject()
                        );
                }
                return _deleteCommand;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Launches the new project window.
        /// </summary>
        public void CreateProject()
        {
            ProjectView window = new ProjectView();

            using (var viewModel = new ProjectViewModel(Project.CreateNewProject(), _projectData, _taskData))
            {
                this.ShowWorkspaceAsDialog(window, viewModel);
            }
        }

        /// <summary>
        /// Launches the edit task window.
        /// </summary>
        public void EditProject()
        {
            ProjectView window = new ProjectView();

            ProjectViewModel selectedProjectVM = AllProjects.FirstOrDefault(p => p.IsSelected == true);

            using (var viewModel = new ProjectViewModel(_projectData.GetProjectByProjectId(selectedProjectVM.ProjectId), _projectData, _taskData))
            {
                this.ShowWorkspaceAsDialog(window, viewModel);
            }
        }

        /// <summary>
        /// Deletes the selected project.
        /// </summary>
        public void DeleteProject()
        {
            ProjectViewModel selectedProjectVM = AllProjects.FirstOrDefault(p => p.IsSelected == true);
            
            if (selectedProjectVM != null && MessageBox.Show(Properties.Resources.Projects_Delete_Confirm, Properties.Resources.Delete_Confirm, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _projectData.DeleteProject(_projectData.GetProjectByProjectId(selectedProjectVM.ProjectId), _taskData);
                selectedProjectVM.Dispose();
            }
        }

        #endregion // Public Methods

        #region Private Helpers

        bool CanEditProject()
        {
            return AllProjects.Count(p => p.IsSelected == true) == 1; 
        }

        bool CanDeleteProject()
        {
            return AllProjects.Count(p => p.IsSelected == true) == 1;
        }

        #endregion
    }
}