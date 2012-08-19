using System;
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
    public class AllProjectsViewModel : NavigatorViewModel
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

            this.AllProjects = new ObservableCollection<ProjectViewModel>();
            this.AllProjects.CollectionChanged += this.OnCollectionChanged;

            this.PageFirst();

            SortColumns.Add(new SortableProperty() { Description = "Title", Name = "Title" });
            SortColumns.Add(new SortableProperty() { Description = "Status", Name = "StatusId" });
            SortColumns.Add(new SortableProperty() { Description = "Est. Cost", Name = "EstimatedCost" });
            SortColumns.Add(new SortableProperty() { Description = "Goal", Name = "GoalTitle" });
            SortColumns.Add(new SortableProperty() { Description = "Date Created", Name = "CreatedDate" });
            SortColumns.Add(new SortableProperty() { Description = "Date Completed", Name = "CompletedDate" });

            SelectedSortColumn = SortColumns.FirstOrDefault();
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
            RefreshPage();
        }

        void OnProjectUpdated(object sender, ProjectUpdatedEventArgs e)
        {
            RefreshPage();
        }

        void OnProjectDeleted(object sender, ProjectDeletedEventArgs e)
        {
            RefreshPage();
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

        public override void PerformFilter()
        {
            if (FilterTermHasChanged)
            {
                PageFirst();
            }
        }

        public override void SortResults()
        {
            PageFirst();
        }

        public override void GetPagedTasks(int pageNumber)
        {
            for (int i = (AllProjects.Count - 1); i >= 0; i--)
            {
                ProjectViewModel projectVm = AllProjects[i];
                this.AllProjects.Remove(projectVm);
                projectVm.Dispose();
            }

            List<ProjectViewModel> all =
                (from project in _projectData.GetProjects(FilterTerm, SelectedSortColumn, pageNumber)
                 select new ProjectViewModel(project, _projectData, _taskData)).ToList();

            foreach (ProjectViewModel pvm in all)
                pvm.PropertyChanged += this.OnProjectViewModelPropertyChanged;

            for (int i = 0; i < all.Count; i++)
            {
                this.AllProjects.Add(all[i]);
            }

            FirstRecordNumber = AllProjects.Count > 0 ? (Constants.RecordsPerPage * (pageNumber - 1)) + 1 : 0;
            LastRecordNumber = FirstRecordNumber + AllProjects.Count - 1;
            TotalRecordCount = _projectData.GetProjectsCount(FilterTerm);
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