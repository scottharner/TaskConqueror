using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;

namespace TaskConqueror
{
    /// <summary>
    /// The ViewModel for the application's main window.
    /// </summary>
    public class MainWindowViewModel : WorkspaceViewModel
    {
        #region Fields
                
        ReadOnlyCollection<CommandViewModel> _commands;
        ObservableCollection<WorkspaceViewModel> _workspaces;
        TaskData _taskData;
        ProjectData _projectData;
        GoalData _goalData;

        #endregion // Fields

        #region Constructor

        public MainWindowViewModel()
        {
            base.DisplayName = Properties.Resources.AppName;
            _taskData = new TaskData();
            _projectData = new ProjectData();
            _goalData = new GoalData();
            WorkspaceViewModel activeVM = ListActiveTasks();
            ListTasks();
            ListProjects();
            ListGoals();
            ListReports();
            ShowAdminOptions();
            this.SetActiveWorkspace(activeVM);
        }

        #endregion // Constructor

        #region Commands

        /// <summary>
        /// Returns a read-only list of commands 
        /// that the UI can display and execute.
        /// </summary>
        public ReadOnlyCollection<CommandViewModel> Commands
        {
            get
            {
                if (_commands == null)
                {
                    List<CommandViewModel> cmds = this.CreateCommands();
                    _commands = new ReadOnlyCollection<CommandViewModel>(cmds);
                }
                return _commands;
            }
        }

        List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>
            {
                                    new CommandViewModel(
                    Properties.Resources.MainWindowViewModel_Command_ListTasks,
                    new RelayCommand(param => this.ListTasks()))

            };
        }


        #endregion // Commands

        #region Workspaces

        /// <summary>
        /// Returns the collection of available workspaces to display.
        /// A 'workspace' is a ViewModel that can request to be closed.
        /// </summary>
        public ObservableCollection<WorkspaceViewModel> Workspaces
        {
            get
            {
                if (_workspaces == null)
                {
                    _workspaces = new ObservableCollection<WorkspaceViewModel>();
                    _workspaces.CollectionChanged += this.OnWorkspacesChanged;
                }
                return _workspaces;
            }
        }

        void OnWorkspacesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.NewItems)
                    workspace.RequestClose += this.OnWorkspaceRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.OldItems)
                    workspace.RequestClose -= this.OnWorkspaceRequestClose;
        }

        void OnWorkspaceRequestClose(object sender, EventArgs e)
        {
            WorkspaceViewModel workspace = sender as WorkspaceViewModel;
            workspace.Dispose();
            this.Workspaces.Remove(workspace);
        }

        #endregion // Workspaces

        #region Private Helpers

        WorkspaceViewModel ListTasks()
        {
            TaskData taskData = _taskData;
            AllTasksViewModel workspace = new AllTasksViewModel(taskData);
            this.Workspaces.Add(workspace);
            return workspace;
        }

        WorkspaceViewModel ListProjects()
        {
            ProjectData projectData = _projectData;
            AllProjectsViewModel workspace = new AllProjectsViewModel(projectData, _taskData);
            this.Workspaces.Add(workspace);
            return workspace;
        }

        WorkspaceViewModel ListGoals()
        {
            AllGoalsViewModel workspace = new AllGoalsViewModel(_goalData, _projectData, _taskData);
            this.Workspaces.Add(workspace);
            return workspace;
        }

        WorkspaceViewModel ListReports()
        {
            AllReportsViewModel workspace = new AllReportsViewModel();
            this.Workspaces.Add(workspace);
            return workspace;
        }

        WorkspaceViewModel ShowAdminOptions()
        {
            AdminViewModel workspace = new AdminViewModel(_taskData, _projectData, _goalData);
            this.Workspaces.Add(workspace);
            return workspace;
        }

        WorkspaceViewModel ListActiveTasks()
        {
            TaskData taskData = _taskData;
            ActiveTasksViewModel workspace = new ActiveTasksViewModel(taskData);
            this.Workspaces.Add(workspace);
            this.SetActiveWorkspace(workspace);
            return workspace;
        }

        void SetActiveWorkspace(WorkspaceViewModel workspace)
        {
            Debug.Assert(this.Workspaces.Contains(workspace));

            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.Workspaces);
            if (collectionView != null)
                collectionView.MoveCurrentTo(workspace);
        }

        #endregion // Private Helpers
    }
}