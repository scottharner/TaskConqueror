using System;
using System.ComponentModel;
using System.Diagnostics;
using TaskConqueror.Properties;
using System.Linq;
using System.Collections.Generic;

namespace TaskConqueror
{
    /// <summary>
    /// Represents a project.  This class
    /// has built-in validation logic. It is wrapped
    /// by the ProjectViewModel class, which enables it to
    /// be easily displayed and edited by a WPF user interface.
    /// </summary>
    public class Project : IDataErrorInfo
    {
        #region Creation

        public static Project CreateNewProject(int? goalId = null)
        {
            Project newProject = new Project()
            {
                StatusId = (int)Statuses.New
            };

            if (goalId.HasValue)
            {
                GoalData goalData = new GoalData();
                newProject.ParentGoal = goalData.GetGoalByGoalId(goalId.Value);
            }

            return newProject;
        }

        public static Project CreateProject(Data.Project dbProject)
        {
            Data.Goal parentGoal = (from pt in dbProject.Goals
                                          select pt).FirstOrDefault();

            return new Project
            {
                ProjectId = dbProject.ProjectID,
                Title = dbProject.Title,
                CreatedDate = dbProject.CreatedDate,
                CompletedDate = dbProject.CompletedDate,
                EstimatedCost = dbProject.EstimatedCost,
                StatusId = dbProject.StatusID,
                ParentGoal = parentGoal == null ? null : Goal.CreateGoal(parentGoal)
            };
        }

        protected Project()
        {
        }

        #endregion // Creation

        #region State Properties

        /// <summary>
        /// Gets/sets the project id for the project.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets/sets the status id for the project.
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets/sets the project title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets/sets the project's date of creation.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets/sets the project's date of completion.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Gets/sets the project's estimated cost.
        /// </summary>
        public decimal? EstimatedCost { get; set; }

        /// <summary>
        /// Gets/sets the project's parent goal.
        /// </summary>
        public Goal ParentGoal { get; set; }

        #endregion // State Properties

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error { get { return null; } }

        string IDataErrorInfo.this[string propertyName]
        {
            get { return this.GetValidationError(propertyName); }
        }

        #endregion // IDataErrorInfo Members

        #region Validation

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                    if (GetValidationError(property) != null)
                        return false;

                return true;
            }
        }

        static readonly string[] ValidatedProperties = 
        { 
            "Title",
            "StatusId",
            "EstimatedCost"
        };

        string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "Title":
                    error = this.ValidateTitle();
                    break;

                case "StatusId":
                    error = this.ValidateStatusId();
                    break;

                case "EstimatedCost":
                    error = this.ValidateEstimatedCost();
                    break;

                default:
                    Debug.Fail("Unexpected property being validated on Project: " + propertyName);
                    break;
            }

            return error;
        }

        string ValidateTitle()
        {
            if (IsStringMissing(this.Title))
            {
                return Properties.Resources.Error_MissingTitle;
            }
            return null;
        }

        string ValidateStatusId()
        {
            AppInfo appInfo = AppInfo.Instance;
            if (appInfo.StatusList.FirstOrDefault(s => s.StatusID == this.StatusId) == null)
            {
                return Properties.Resources.Error_InvalidStatus;
            }

            return null;
        }

        string ValidateEstimatedCost()
        {
            AppInfo appInfo = AppInfo.Instance;
            if (this.EstimatedCost.HasValue && this.EstimatedCost < 0)
            {
                return Properties.Resources.Project_Error_InvalidEstimatedCost;
            }

            return null;
        }

        static bool IsStringMissing(string value)
        {
            return
                String.IsNullOrEmpty(value) ||
                value.Trim() == String.Empty;
        }

        #endregion // Validation
    }
}