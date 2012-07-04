using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace TaskConqueror
{
    /// <summary>
    /// Represents a source of goals in the application.
    /// </summary>
    public class GoalData
    {
        #region Fields

        private AppInfo _appInfo;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new goal data access class.
        /// </summary>
        public GoalData()
        {
            _appInfo = AppInfo.Instance;
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Raised when a goal is placed into the repository.
        /// </summary>
        public event EventHandler<GoalAddedEventArgs> GoalAdded;

        /// <summary>
        /// Places the specified goal into the db.
        /// If the goal is already in the repository, an
        /// exception is thrown.
        /// </summary>
        public int AddGoal(Goal goal)
        {
            if (goal == null)
                throw new ArgumentNullException("goal");

            Data.Goal dbGoal = new Data.Goal();
            dbGoal.GoalID = AppInfo.Instance.GcContext.Goals.Max(g => g.GoalID) + 1;
            dbGoal.StatusID = goal.StatusId;
            dbGoal.CategoryID = goal.CategoryId;
            dbGoal.CreatedDate = goal.CreatedDate;
            dbGoal.CompletedDate = goal.CompletedDate;
            dbGoal.Title = goal.Title;

            _appInfo.GcContext.AddToGoals(dbGoal);
            _appInfo.GcContext.SaveChanges();

            if (this.GoalAdded != null)
                this.GoalAdded(this, new GoalAddedEventArgs(goal));

            return dbGoal.GoalID;
        }

        /// <summary>
        /// Raised when a goal is modified in the db.
        /// </summary>
        public event EventHandler<GoalUpdatedEventArgs> GoalUpdated;

        /// <summary>
        /// Update the specified goal into the db.
        /// If the goal is not already in the repository, an
        /// exception is thrown.
        /// </summary>
        public void UpdateGoal(Goal goal)
        {
            if (goal == null)
                throw new ArgumentNullException("goal");

            if (goal.GoalId == 0)
                throw new InvalidOperationException("modify goal");

            Data.Goal dbGoal = (from g in _appInfo.GcContext.Goals
             where g.GoalID == goal.GoalId
             select g).FirstOrDefault();

            if (dbGoal == null)
            {
                throw new InvalidDataException("goal id");
            }
            else
            {
                dbGoal.StatusID = goal.StatusId;
                dbGoal.CategoryID = goal.CategoryId;
                dbGoal.CreatedDate = goal.CreatedDate;
                dbGoal.CompletedDate = goal.CompletedDate;
                dbGoal.Title = goal.Title;

                _appInfo.GcContext.SaveChanges();

                if (this.GoalUpdated != null)
                    this.GoalUpdated(this, new GoalUpdatedEventArgs(goal));
            }
        }

        /// <summary>
        /// Raised when a goal is deleted in the db.
        /// </summary>
        public event EventHandler<GoalDeletedEventArgs> GoalDeleted;

        /// <summary>
        /// Delete the specified goal from the db.
        /// If the goal is not already in the repository, an
        /// exception is thrown.
        /// </summary>
        public void DeleteGoal(Goal goal)
        {
            if (goal == null)
                throw new ArgumentNullException("goal");

            if (goal.GoalId == 0)
                throw new InvalidOperationException("delete goal");

            var query = (from g in _appInfo.GcContext.Goals
                         where g.GoalID == goal.GoalId
                         select g).First();
            
            _appInfo.GcContext.DeleteObject(query);
            _appInfo.GcContext.SaveChanges();

            if (this.GoalDeleted != null)
                this.GoalDeleted(this, new GoalDeletedEventArgs(goal));
        }

        /// <summary>
        /// Returns true if the specified goal exists in the
        /// db, or false if it is not.
        /// </summary>
        public bool GoalExists(Goal goal)
        {
            if (goal == null)
                throw new ArgumentNullException("goal");

            return _appInfo.GcContext.Goals.FirstOrDefault(g => g.GoalID == goal.GoalId) != null;
        }

        /// <summary>
        /// Returns a shallow-copied list of all goals in the repository.
        /// </summary>
        public List<Goal> GetGoals()
        {
            List<Data.Goal> dbGoals = (from g in _appInfo.GcContext.Goals
                                       orderby g.Title
                                       select g).ToList();

            List<Goal> goals = new List<Goal>();

            foreach (Data.Goal dbGoal in dbGoals)
            {
                goals.Add(Goal.CreateGoal(dbGoal));
            }

            return goals;
        }

        /// <summary>
        /// Returns a shallow-copied list of all goals that contain tasks.
        /// </summary>
        public List<Goal> GetGoalsContainingInactiveTasks()
        {
            List<Data.Goal> dbGoals = (from p in _appInfo.GcContext.Projects
                                       where p.Tasks.Any(t => t.IsActive == false && t.StatusID != Statuses.Completed && t.StatusID != Statuses.Abandoned) && p.Goals.Count > 0
                                       orderby p.Goals.FirstOrDefault().Title
                                       select p.Goals.FirstOrDefault()).Distinct().ToList();

            List<Goal> goals = new List<Goal>();

            foreach (Data.Goal dbGoal in dbGoals)
            {
                goals.Add(Goal.CreateGoal(dbGoal));
            }

            return goals;
        }

        /// <summary>
        /// Returns a shallow-copied list of a single goal by goal id.
        /// </summary>
        public Goal GetGoalByGoalId(int goalId)
        {
            Goal requestedGoal = null;
            Data.Goal requestedDbGoal = (from g in _appInfo.GcContext.Goals
                                       where g.GoalID == goalId
                                       select g).FirstOrDefault();

            if (requestedDbGoal != null)
            {
                requestedGoal = Goal.CreateGoal(requestedDbGoal);
            }

            return requestedGoal;
        }

        /// <summary>
        /// Returns a child projects for the goal with the provided goal id.
        /// </summary>
        public List<Project> GetChildProjects(int goalId)
        {
            List<Project> childProjects = new List<Project>();
            List<Data.Project> requestedDbProjects = (from p in _appInfo.GcContext.Projects
                                         where p.Goals.FirstOrDefault().GoalID == goalId
                                         select p).ToList();

            foreach (Data.Project childDbProject in requestedDbProjects)
            {
                childProjects.Add(Project.CreateProject(childDbProject));
            }
            
            return childProjects;
        }

        /// <summary>
        /// Returns a child projects that contain tasks for the goal with the provided goal id.
        /// </summary>
        public List<Project> GetChildProjectsContainingInactiveTasks(int goalId)
        {
            List<Project> childProjects = new List<Project>();
            List<Data.Project> requestedDbProjects = (from p in _appInfo.GcContext.Projects
                                                      where p.Goals.FirstOrDefault().GoalID == goalId && p.Tasks.Any(t => t.IsActive == false && t.StatusID != Statuses.Completed && t.StatusID != Statuses.Abandoned)
                                                      select p).ToList();

            foreach (Data.Project childDbProject in requestedDbProjects)
            {
                childProjects.Add(Project.CreateProject(childDbProject));
            }

            return childProjects;
        }

        #endregion // Public Interface

        #region Private Helpers

        #endregion // Private Helpers
    }
}