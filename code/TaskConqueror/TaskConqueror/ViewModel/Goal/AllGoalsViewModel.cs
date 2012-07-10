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
    /// Represents a container of GoalViewModel objects
    /// that has support for staying synchronized with the
    /// database.
    /// </summary>
    public class AllGoalsViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly GoalData _goalData;
        readonly ProjectData _projectData;
        readonly TaskData _taskData;
        RelayCommand _newCommand;
        RelayCommand _editCommand;
        RelayCommand _deleteCommand;

        #endregion // Fields

        #region Constructor

        public AllGoalsViewModel(GoalData goalData, ProjectData projectData, TaskData taskData)
        {
            if (goalData == null)
                throw new ArgumentNullException("goalData");

            if (projectData == null)
                throw new ArgumentNullException("projectData");

            if (taskData == null)
                throw new ArgumentNullException("taskData");

            base.DisplayName = Properties.Resources.Goals_DisplayName;            

            _goalData = goalData;
            _projectData = projectData;
            _taskData = taskData;

            // Subscribe for notifications of when a new goal is saved.
            _goalData.GoalAdded += this.OnGoalAdded;
            _goalData.GoalUpdated += this.OnGoalUpdated;
            _goalData.GoalDeleted += this.OnGoalDeleted;

            // Populate the AllGoals collection with GoalViewModels.
            this.CreateAllGoals();              
        }

        void CreateAllGoals()
        {
            List<GoalViewModel> all =
                (from goal in _goalData.GetGoals()
                 select new GoalViewModel(goal, _goalData, _projectData, _taskData)).ToList();

            foreach (GoalViewModel gvm in all)
                gvm.PropertyChanged += this.OnGoalViewModelPropertyChanged;

            this.AllGoals = new ObservableCollection<GoalViewModel>(all);
            this.AllGoals.CollectionChanged += this.OnCollectionChanged;
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the GoalViewModel objects.
        /// </summary>
        public ObservableCollection<GoalViewModel> AllGoals { get; private set; }

        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (GoalViewModel goalVM in this.AllGoals)
                goalVM.Dispose();

            this.AllGoals.Clear();
            this.AllGoals.CollectionChanged -= this.OnCollectionChanged;

            // todo: dispose of child project vms

            _goalData.GoalAdded -= this.OnGoalAdded;
            _goalData.GoalUpdated -= this.OnGoalUpdated;
            _goalData.GoalDeleted -= this.OnGoalDeleted;
        }

        #endregion // Base Class Overrides

        #region Event Handling Methods

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (GoalViewModel goalVM in e.NewItems)
                    goalVM.PropertyChanged += this.OnGoalViewModelPropertyChanged;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (GoalViewModel goalVM in e.OldItems)
                    goalVM.PropertyChanged -= this.OnGoalViewModelPropertyChanged;
        }

        void OnGoalViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string IsSelected = "IsSelected";

            // Make sure that the property name we're referencing is valid.
            // This is a debugging technique, and does not execute in a Release build.
            (sender as GoalViewModel).VerifyPropertyName(IsSelected);

        }

        void OnGoalAdded(object sender, GoalAddedEventArgs e)
        {
            var viewModel = new GoalViewModel(e.NewGoal, _goalData, _projectData, _taskData);
            this.AllGoals.Add(viewModel);
        }

        void OnGoalUpdated(object sender, GoalUpdatedEventArgs e)
        {
            this.AllGoals.Remove(this.AllGoals.FirstOrDefault(g => g.GoalId == e.UpdatedGoal.GoalId));
            var viewModel = new GoalViewModel(e.UpdatedGoal, _goalData, _projectData, _taskData);
            this.AllGoals.Add(viewModel);
        }

        void OnGoalDeleted(object sender, GoalDeletedEventArgs e)
        {
            this.AllGoals.Remove(this.AllGoals.FirstOrDefault(g => g.GoalId == e.DeletedGoal.GoalId));
        }

        #endregion // Event Handling Methods

        #region Commands

        /// <summary>
        /// Returns a command that creates a new goal.
        /// </summary>
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand(
                        param => this.CreateGoal()
                        );
                }
                return _newCommand;
            }
        }

        /// <summary>
        /// Returns a command that edits an existing goal.
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(
                        param => this.EditGoal(),
                        param => this.CanEditGoal()
                        );
                }
                return _editCommand;
            }
        }

        /// <summary>
        /// Returns a command that deletes an existing goal.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(
                        param => this.DeleteGoal(),
                        param => this.CanDeleteGoal()
                        );
                }
                return _deleteCommand;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Launches the new goal window.
        /// </summary>
        public void CreateGoal()
        {
            GoalView window = new GoalView();

            var viewModel = new GoalViewModel(Goal.CreateNewGoal(), _goalData, _projectData, _taskData);

            this.ShowWorkspaceAsDialog(window, viewModel);
        }

        /// <summary>
        /// Launches the edit goal window.
        /// </summary>
        public void EditGoal()
        {
            GoalView window = new GoalView();

            GoalViewModel selectedGoalVM = AllGoals.FirstOrDefault(g => g.IsSelected == true);
            
            var viewModel = new GoalViewModel(_goalData.GetGoalByGoalId(selectedGoalVM.GoalId), _goalData, _projectData, _taskData);

            this.ShowWorkspaceAsDialog(window, viewModel);
        }

        /// <summary>
        /// Deletes the selected goal.
        /// </summary>
        public void DeleteGoal()
        {
            GoalViewModel selectedGoalVM = AllGoals.FirstOrDefault(g => g.IsSelected == true);
            
            if (selectedGoalVM != null && MessageBox.Show(Properties.Resources.Goals_Delete_Confirm, Properties.Resources.Delete_Confirm, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _goalData.DeleteGoal(_goalData.GetGoalByGoalId(selectedGoalVM.GoalId));
            }
        }

        #endregion // Public Methods

        #region Private Helpers

        bool CanEditGoal()
        {
            return AllGoals.Count(g => g.IsSelected == true) == 1; 
        }

        bool CanDeleteGoal()
        {
            return AllGoals.Count(g => g.IsSelected == true) == 1;
        }

        #endregion
    }
}