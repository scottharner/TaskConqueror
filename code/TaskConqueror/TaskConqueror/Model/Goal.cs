using System;
using System.ComponentModel;
using System.Diagnostics;
using TaskConqueror.Properties;
using System.Linq;
using System.Collections.Generic;

namespace TaskConqueror
{
    /// <summary>
    /// Represents a goal.  This class
    /// has built-in validation logic. It is wrapped
    /// by the GoalViewModel class, which enables it to
    /// be easily displayed and edited by a WPF user interface.
    /// </summary>
    public class Goal : IDataErrorInfo
    {
        #region Creation

        public static Goal CreateNewGoal()
        {
            return new Goal()
            {
                StatusId = (int)Statuses.New,
            };
        }

        public static Goal CreateGoal(Data.Goal dbGoal)
        {
            return new Goal
            {
                GoalId = dbGoal.GoalID,
                Title = dbGoal.Title,
                CreatedDate = dbGoal.CreatedDate,
                CompletedDate = dbGoal.CompletedDate,
                CategoryId = dbGoal.CategoryID,
                StatusId = dbGoal.StatusID
            };
        }

        protected Goal()
        {
        }

        #endregion // Creation

        #region State Properties

        /// <summary>
        /// Gets/sets the goal id for the goal.
        /// </summary>
        public int GoalId { get; set; }

        /// <summary>
        /// Gets/sets the status id for the goal.
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets/sets the goal title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets/sets the goal's date of creation.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets/sets the goal's date of completion.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Gets/sets the category id for the goal.
        /// </summary>
        public int CategoryId { get; set; }

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
            "CategoryId"
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

                case "CategoryId":
                    error = this.ValidateCategoryId();
                    break;

                default:
                    Debug.Fail("Unexpected property being validated on Goal: " + propertyName);
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

        string ValidateCategoryId()
        {
            AppInfo appInfo = AppInfo.Instance;
            if (appInfo.CategoryList.FirstOrDefault(c => c.CategoryID == this.CategoryId) == null)
            {
                return Properties.Resources.Error_InvalidStatus;
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