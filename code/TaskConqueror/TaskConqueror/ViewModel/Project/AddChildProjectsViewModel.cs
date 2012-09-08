using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Data;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace TaskConqueror
{
    /// <summary>
    /// Allows addition of a project to a goal.
    /// </summary>
    public class AddChildProjectsViewModel : WorkspaceViewModel
    {
        #region Fields

        RelayCommand _addCommand;
        RelayCommand _okCommand;
        RelayCommand _removeCommand;
        ObservableCollection<ProjectViewModel> _unassignedProjects = new ObservableCollection<ProjectViewModel>();
        ObservableCollection<ProjectViewModel> _selectedProjects = new ObservableCollection<ProjectViewModel>();
        readonly Goal _parentGoal;
        readonly GoalData _goalData;
        readonly ProjectData _projectData;

        #endregion // Fields

        #region Constructor

        public AddChildProjectsViewModel(ProjectData projectData, Goal parentGoal, GoalData goalData, TaskData taskData)
        {
            if (projectData == null)
                throw new ArgumentNullException("projectData");

            if (goalData == null)
                throw new ArgumentNullException("goalData");
            
            if (parentGoal == null)
                throw new ArgumentNullException("parentGoal");

            List<Project> unassignedProjects = projectData.GetUnassignedProjects();
            foreach (Project unassignedProject in unassignedProjects)
	        {
		        _unassignedProjects.Add(new ProjectViewModel(unassignedProject, projectData, taskData));
	        }

            _parentGoal = parentGoal;
            _goalData = goalData;
            _projectData = projectData;

            base.DisplayName = Properties.Resources.Add_Projects_DisplayName;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/project.png";
        }

        #endregion // Constructor

        #region Goal Properties

        /// <summary>
        /// the projects that have been selected for addition
        /// </summary>       
        public ObservableCollection<ProjectViewModel> SelectedProjects
        {
            get { return _selectedProjects; }
            set
            {
                if (_selectedProjects == value)
                    return;

                _selectedProjects = value;

                this.OnPropertyChanged("SelectedProjects");
            }
        }

        /// <summary>
        /// list of projects that have not be assigned to a goal
        /// </summary>       
        public ObservableCollection<ProjectViewModel> UnassignedProjects
        {
            get { return _unassignedProjects; }
            set
            {
                if (_unassignedProjects == value)
                    return;

                _unassignedProjects = value;

                this.OnPropertyChanged("UnassignedProjects");
            }
        }

        #endregion // Properties

        #region Presentation Properties

        /// <summary>
        /// Returns a command that adds the project to the selected list.
        /// </summary>
        public ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RelayCommand(
                        param => this.AddProject(),
                        param => this.CanAddProject
                        );
                }
                return _addCommand;
            }
        }

        /// <summary>
        /// Returns a command that adds the selected project to the current goal.
        /// </summary>
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(
                        param => { this.AddProjectsToGoal(); this.OnRequestClose(); },
                        param => this.CanAddProjectsToGoal
                        );
                }
                return _okCommand;
            }
        }

        /// <summary>
        /// Returns a command that removes a project from the selected project list.
        /// </summary>
        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new RelayCommand(
                        param => this.RemoveProject(),
                        param => this.CanRemoveProject
                        );
                }
                return _removeCommand;
            }
        }

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Adds the project to the selected list.  This method is invoked by the AddCommand.
        /// </summary>
        public void AddProject()
        {
            ProjectViewModel selectedProjectVM = _unassignedProjects.FirstOrDefault(p => p.IsSelected == true);
            SelectedProjects.Add(selectedProjectVM);
            UnassignedProjects.Remove(selectedProjectVM);
        }

        /// <summary>
        /// Removes the project from the selected list.  This method is invoked by the RemoveCommand.
        /// </summary>
        public void RemoveProject()
        {
            ProjectViewModel selectedProjectVM = SelectedProjects.FirstOrDefault(p => p.IsSelected == true);
            UnassignedProjects.Add(selectedProjectVM);
            SelectedProjects.Remove(selectedProjectVM);
        }

        /// <summary>
        /// Adds the selected projects to the current goal.
        /// </summary>
        public void AddProjectsToGoal()
        {
            List<int> selectedProjectIds = new List<int>();
            foreach (ProjectViewModel selectedProject in SelectedProjects)
            {
                selectedProjectIds.Add(selectedProject.ProjectId);
            }
            
            _projectData.AddProjectsToGoal(_parentGoal, selectedProjectIds);
        }

        public override void ViewHelp()
        {
            Help.ShowHelp(null, "TaskConqueror.chm", "html/goals/add_projects.htm");
        }

        #endregion // Public Methods

        #region Private Helpers

        /// <summary>
        /// Returns true if a project can be added to the goal.
        /// </summary>
        bool CanAddProject
        {
            get { return UnassignedProjects.Any(p => p.IsSelected == true); }
        }

        /// <summary>
        /// Returns true if there is a project available for removal from the selected list.
        /// </summary>
        bool CanRemoveProject
        {
            get { return SelectedProjects.Any(p => p.IsSelected == true); }
        }

        /// <summary>
        /// Returns true if there are projects to be added to the current goal.
        /// </summary>
        bool CanAddProjectsToGoal
        {
            get
            { return SelectedProjects.Count > 0; }
        }

        #endregion // Private Helpers

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (ProjectViewModel projectVM in this.UnassignedProjects)
                projectVM.Dispose();

            this.UnassignedProjects.Clear();

            foreach (ProjectViewModel projectVM in this.SelectedProjects)
                projectVM.Dispose();

            this.SelectedProjects.Clear();
        }

        #endregion // Base Class Overrides
    }
}