using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;

namespace TaskConqueror
{
    /// <summary>
    /// A UI-friendly wrapper for a Goal object.
    /// </summary>
    public class GoalViewModel : EditorViewModel, IDataErrorInfo
    {
        #region Fields

        readonly Goal _goal;
        readonly GoalData _goalData;
        readonly ProjectData _projectData;
        readonly TaskData _taskData;
        List<Data.Status> _statusOptions;
        Data.Status _selectedStatus;
        List<Data.GoalCategory> _categoryOptions;
        Data.GoalCategory _selectedCategory;
        bool _isSaved;
        string _statusDescription;
        string _categoryDescription;
        ObservableCollection<ProjectViewModel> _childProjectVMs = new ObservableCollection<ProjectViewModel>();
        RelayCommand _newProjectCommand;
        RelayCommand _editProjectCommand;
        RelayCommand _deleteProjectCommand;
        RelayCommand _addProjectsCommand;
        int _originalStatusId;

        #endregion // Fields

        #region Constructor

        public GoalViewModel(Goal goal, GoalData goalData, ProjectData projectData, TaskData taskData)
        {
            if (goal == null)
                throw new ArgumentNullException("goal");

            if (goalData == null)
                throw new ArgumentNullException("goalData");

            if (projectData == null)
                throw new ArgumentNullException("projectData");

            if (taskData == null)
                throw new ArgumentNullException("taskData");

            _goal = goal;
            _goalData = goalData;
            _projectData = projectData;
            _taskData = taskData;
            _statusOptions = AppInfo.Instance.StatusList;
            _selectedStatus = _statusOptions.FirstOrDefault(s => s.StatusID == this.StatusId);
            _categoryOptions = AppInfo.Instance.CategoryList;
            _selectedCategory = _categoryOptions.FirstOrDefault(c => c.CategoryID == this.CategoryId);
            _isSaved = true;
            _statusDescription = _statusOptions.FirstOrDefault(s => s.StatusID == this.StatusId).Description;
            _categoryDescription = _categoryOptions.FirstOrDefault(c => c.CategoryID == this.CategoryId).Description;
            List<Project> childProjects = _goalData.GetChildProjects(_goal.GoalId);
            foreach (Project childProject in childProjects)
            {
                _childProjectVMs.Add(new ProjectViewModel(childProject, projectData, taskData));
            }

            _originalStatusId = StatusId;

            // Subscribe for notifications of when a new project is saved.
            _projectData.ProjectAdded += this.OnProjectAdded;
            _projectData.ProjectUpdated += this.OnProjectUpdated;
            _projectData.ProjectDeleted += this.OnProjectDeleted;

            base.DisplayName = Properties.Resources.Edit_Goal_DisplayName;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/goal.png";
        }

        #endregion // Constructor

        #region Goal Properties

        public int GoalId
        {
            get { return _goal.GoalId; }
            set
            {
                if (value == _goal.GoalId)
                    return;

                _goal.GoalId = value;

                base.OnPropertyChanged("GoalId");
            }
        }

        public int StatusId
        {
            get { return _goal.StatusId; }
            set
            {
                if (value == _goal.StatusId)
                    return;

                _goal.StatusId = value;
                _isSaved = false;
                _statusDescription = _statusOptions.FirstOrDefault(s => s.StatusID == value).Description;

                base.OnPropertyChanged("StatusId");
            }
        }

        public int CategoryId
        {
            get { return _goal.CategoryId; }
            set
            {
                if (value == _goal.CategoryId)
                    return;

                _goal.CategoryId = value;
                _isSaved = false;
                _categoryDescription = _categoryOptions.FirstOrDefault(c => c.CategoryID == value).Description;

                base.OnPropertyChanged("CategoryId");
            }
        }

        public DateTime CreatedDate
        {
            get { return _goal.CreatedDate; }
            set
            {
                if (value == _goal.CreatedDate)
                    return;

                _goal.CreatedDate = value;

                base.OnPropertyChanged("CreatedDate");
            }
        }

        public DateTime? CompletedDate
        {
            get { return _goal.CompletedDate; }
            set
            {
                if (value == _goal.CompletedDate)
                    return;

                _goal.CompletedDate = value;

                base.OnPropertyChanged("CompletedDate");
            }
        }

        public string Title
        {
            get { return _goal.Title; }
            set
            {
                if (value == _goal.Title)
                    return;

                _goal.Title = value;
                _isSaved = false;

                base.OnPropertyChanged("Title");
            }
        }

        public ObservableCollection<ProjectViewModel> ChildProjects
        {
            get { return _childProjectVMs; }
        }

        #endregion // Properties

        #region Presentation Properties

        public List<Data.Status> StatusOptions
        {
            get { return _statusOptions; }
        }

        public Data.Status SelectedStatus
        {
            get { return _selectedStatus; }
            set
            {
                _selectedStatus = value;
                StatusId = _selectedStatus.StatusID;
            }
        }

        public string StatusDescription
        {
            get { return _statusDescription; }
        }

        public List<Data.GoalCategory> CategoryOptions
        {
            get { return _categoryOptions; }
        }

        public Data.GoalCategory SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                CategoryId = _selectedCategory.CategoryID;
            }
        }

        public string CategoryDescription
        {
            get { return _categoryDescription; }
        }

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Saves the goal to the database.  This method is invoked by the SaveCommand.
        /// </summary>
        public override void Save()
        {
            if (!_goal.IsValid)
                throw new InvalidOperationException(Properties.Resources.GoalViewModel_Exception_CannotSave);

            if (this.IsNewGoal)
            {
                CreatedDate = DateTime.Now;

                if (StatusId == Statuses.Completed)
                    CompletedDate = DateTime.Now;

                GoalId = _goalData.AddGoal(_goal);
            }
            else
            {
                if (StatusId != _originalStatusId && StatusId == Statuses.Completed)
                {
                    CompletedDate = DateTime.Now;
                }
                else if (StatusId != Statuses.Completed && _originalStatusId == Statuses.Completed)
                {
                    CompletedDate = null;
                }
                
                _goalData.UpdateGoal(_goal);
            }

            _originalStatusId = StatusId;
            _isSaved = true;
        }

        /// <summary>
        /// Launches the new project window.
        /// </summary>
        public void CreateProject()
        {
            ProjectView window = new ProjectView();

            using (var viewModel = new ProjectViewModel(Project.CreateNewProject(this.GoalId), _projectData, _taskData))
            {
                this.ShowWorkspaceAsDialog(window, viewModel);
            }
        }

        /// <summary>
        /// Launches the edit project window.
        /// </summary>
        public void EditProject()
        {
            ProjectView window = new ProjectView();

            ProjectViewModel selectedProjectVM = ChildProjects.FirstOrDefault(p => p.IsSelected == true);

            using (var viewModel = new ProjectViewModel(_projectData.GetProjectByProjectId(selectedProjectVM.ProjectId), _projectData, _taskData))
            {
                this.ShowWorkspaceAsDialog(window, viewModel);
            }
        }

        /// <summary>
        /// Launches the delete project window.
        /// </summary>
        public void DeleteProject()
        {
            ProjectViewModel selectedProjectVM = ChildProjects.FirstOrDefault(p => p.IsSelected == true);

            if (selectedProjectVM != null && MessageBox.Show(Properties.Resources.Projects_Delete_Confirm, Properties.Resources.Delete_Confirm, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _projectData.DeleteProject(_projectData.GetProjectByProjectId(selectedProjectVM.ProjectId), _taskData);
                selectedProjectVM.Dispose();
            }
        }

        /// <summary>
        /// Launches the add projects window.
        /// </summary>
        public void AddProjects()
        {
            AddChildProjectsView window = new AddChildProjectsView();

            using (var viewModel = new AddChildProjectsViewModel(_projectData, _goal, _goalData, _taskData))
            {
                this.ShowWorkspaceAsDialog(window, viewModel);
            }
        }

        #endregion // Public Methods

        #region Private Helpers

        /// <summary>
        /// Returns true if this goal was created by the user and it has not yet
        /// been saved to the db.
        /// </summary>
        bool IsNewGoal
        {
            get { return GoalId == 0; }
        }

        /// <summary>
        /// Returns true if no changes have been made since the last save.
        /// </summary>
        protected override bool IsSaved
        {
            get { return _isSaved; }
        }

        /// <summary>
        /// Returns true if the goal is valid and can be saved.
        /// </summary>
        protected override bool CanSave
        {
            get { return _goal.IsValid && (IsNewGoal || !IsSaved); }
        }

        bool CanEditProject()
        {
            return ChildProjects.Count(p => p.IsSelected == true) == 1;
        }

        bool CanDeleteProject()
        {
            return ChildProjects.Count(p => p.IsSelected == true) == 1;
        }

        #endregion // Private Helpers

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_goal as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                error = (_goal as IDataErrorInfo)[propertyName];

                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }

        #endregion // IDataErrorInfo Members

        #region Event Handling Methods

        void OnProjectAdded(object sender, ProjectAddedEventArgs e)
        {
            var viewModel = new ProjectViewModel(e.NewProject, _projectData, _taskData);
            this.ChildProjects.Add(viewModel);
        }

        void OnProjectUpdated(object sender, ProjectUpdatedEventArgs e)
        {
            this.ChildProjects.Remove(this.ChildProjects.FirstOrDefault(p => p.ProjectId == e.UpdatedProject.ProjectId));
            var viewModel = new ProjectViewModel(e.UpdatedProject, _projectData, _taskData);
            this.ChildProjects.Add(viewModel);
        }

        void OnProjectDeleted(object sender, ProjectDeletedEventArgs e)
        {
            using (var projectVM = this.ChildProjects.FirstOrDefault(p => p.ProjectId == e.DeletedProject.ProjectId))
            {
                this.ChildProjects.Remove(projectVM);
            }
        }

        #endregion

        #region Commands
        
        /// <summary>
        /// Returns a command that creates a new project.
        /// </summary>
        public ICommand NewProjectCommand
        {
            get
            {
                if (_newProjectCommand == null)
                {
                    _newProjectCommand = new RelayCommand(
                        param => this.CreateProject()
                        );
                }
                return _newProjectCommand;
            }
        }

        /// <summary>
        /// Returns a command that edits an existing project.
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                if (_editProjectCommand == null)
                {
                    _editProjectCommand = new RelayCommand(
                        param => this.EditProject(),
                        param => this.CanEditProject()
                        );
                }
                return _editProjectCommand;
            }
        }

        /// <summary>
        /// Returns a command that deletes an existing project.
        /// </summary>
        public ICommand DeleteProjectCommand
        {
            get
            {
                if (_deleteProjectCommand == null)
                {
                    _deleteProjectCommand = new RelayCommand(
                        param => this.DeleteProject(),
                        param => this.CanDeleteProject()
                        );
                }
                return _deleteProjectCommand;
            }
        }

        /// <summary>
        /// Returns a command that adds projects to the goal.
        /// </summary>
        public ICommand AddProjectsCommand
        {
            get
            {
                if (_addProjectsCommand == null)
                {
                    _addProjectsCommand = new RelayCommand(
                        param => this.AddProjects()
                        );
                }
                return _addProjectsCommand;
            }
        }

        #endregion

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (ProjectViewModel projectVM in this.ChildProjects)
                projectVM.Dispose();

            this.ChildProjects.Clear();

            _projectData.ProjectAdded -= this.OnProjectAdded;
            _projectData.ProjectUpdated -= this.OnProjectUpdated;
            _projectData.ProjectDeleted -= this.OnProjectDeleted;
        }

        #endregion // Base Class Overrides
    }
}