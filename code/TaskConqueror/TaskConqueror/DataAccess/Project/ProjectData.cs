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
    /// Represents a source of projects in the application.
    /// </summary>
    public class ProjectData
    {
        #region Fields

        private AppInfo _appInfo;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new task data access class.
        /// </summary>
        public ProjectData()
        {
            _appInfo = AppInfo.Instance;
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Raised when a project is placed into the repository.
        /// </summary>
        public event EventHandler<ProjectAddedEventArgs> ProjectAdded;

        /// <summary>
        /// Places the specified project into the db.
        /// If the project is already in the repository, an
        /// exception is thrown.
        /// </summary>
        public int AddProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            Data.Project dbProject = new Data.Project();
            dbProject.ProjectID = AppInfo.Instance.GcContext.Projects.Max(p => p.ProjectID) + 1;
            dbProject.StatusID = project.StatusId;
            dbProject.CreatedDate = project.CreatedDate;
            dbProject.CompletedDate = project.CompletedDate;
            dbProject.Title = project.Title;
            dbProject.EstimatedCost = project.EstimatedCost;

            _appInfo.GcContext.AddToProjects(dbProject);
            _appInfo.GcContext.SaveChanges();

            if (this.ProjectAdded != null)
                this.ProjectAdded(this, new ProjectAddedEventArgs(project));

            return dbProject.ProjectID;
        }

        /// <summary>
        /// Raised when a project is modified in the db.
        /// </summary>
        public event EventHandler<ProjectUpdatedEventArgs> ProjectUpdated;

        /// <summary>
        /// Update the specified project into the db.
        /// If the project is not already in the repository, an
        /// exception is thrown.
        /// </summary>
        public void UpdateProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            if (project.ProjectId == 0)
                throw new InvalidOperationException("modify project");

            Data.Project dbProject = (from p in _appInfo.GcContext.Projects
             where p.ProjectID == project.ProjectId
             select p).FirstOrDefault();

            if (dbProject == null)
            {
                throw new InvalidDataException("project id");
            }
            else
            {
                dbProject.StatusID = project.StatusId;
                dbProject.EstimatedCost = project.EstimatedCost;
                dbProject.CreatedDate = project.CreatedDate;
                dbProject.CompletedDate = project.CompletedDate;
                dbProject.Title = project.Title;

                _appInfo.GcContext.SaveChanges();

                if (this.ProjectUpdated != null)
                    this.ProjectUpdated(this, new ProjectUpdatedEventArgs(project));
            }
        }

        /// <summary>
        /// Raised when a project is deleted in the db.
        /// </summary>
        public event EventHandler<ProjectDeletedEventArgs> ProjectDeleted;

        /// <summary>
        /// Delete the specified project from the db.
        /// If the project is not already in the repository, an
        /// exception is thrown.
        /// </summary>
        public void DeleteProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            if (project.ProjectId == 0)
                throw new InvalidOperationException("delete project");

            var query = (from p in _appInfo.GcContext.Projects
                         where p.ProjectID == project.ProjectId
                         select p).First();
            
            _appInfo.GcContext.DeleteObject(query);
            _appInfo.GcContext.SaveChanges();

            if (this.ProjectDeleted != null)
                this.ProjectDeleted(this, new ProjectDeletedEventArgs(project));
        }

        /// <summary>
        /// Returns true if the specified project exists in the
        /// db, or false if it is not.
        /// </summary>
        public bool ProjectExists(Project project)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            return _appInfo.GcContext.Projects.FirstOrDefault(p => p.ProjectID == project.ProjectId) != null;
        }

        /// <summary>
        /// Returns a shallow-copied list of all projects in the repository.
        /// </summary>
        public List<Project> GetProjects()
        {
            List<Data.Project> dbProjects = (from p in _appInfo.GcContext.Projects
                                       orderby p.Title
                                       select p).ToList();

            List<Project> projects = new List<Project>();

            foreach (Data.Project dbProject in dbProjects)
            {
                projects.Add(Project.CreateProject(dbProject));
            }

            return projects;
        }

        /// <summary>
        /// Returns a shallow-copied list of all projects in the repository that have no associated goal.
        /// </summary>
        public List<Project> GetUnassignedProjects()
        {
            List<Data.Project> dbProjects = (from p in _appInfo.GcContext.Projects
                                             where p.Goals.Count == 0
                                             orderby p.Title
                                             select p).ToList();

            List<Project> projects = new List<Project>();

            foreach (Data.Project dbProject in dbProjects)
            {
                projects.Add(Project.CreateProject(dbProject));
            }

            return projects;
        }

        /// <summary>
        /// Returns a shallow-copied list of all projects in the repository that have no associated goal and contain tasks.
        /// </summary>
        public List<Project> GetUnassignedProjectsContainingInactiveTasks()
        {
            List<Data.Project> dbProjects = (from p in _appInfo.GcContext.Projects
                                             where p.Goals.Count == 0 && p.Tasks.Any(t => t.IsActive == false && t.StatusID != Statuses.Completed && t.StatusID != Statuses.Abandoned)
                                             orderby p.Title
                                             select p).ToList();

            List<Project> projects = new List<Project>();

            foreach (Data.Project dbProject in dbProjects)
            {
                projects.Add(Project.CreateProject(dbProject));
            }

            return projects;
        }

        /// <summary>
        /// Returns a shallow-copied list of all projects that contain tasks.
        /// </summary>
        public List<Project> GetProjectsContainingTasks()
        {
            List<Data.Project> dbProjects = (from p in _appInfo.GcContext.Projects
                                       where p.Tasks.Count > 0
                                       orderby p.Title
                                       select p).ToList();

            List<Project> projects = new List<Project>();

            foreach (Data.Project dbProject in dbProjects)
            {
                projects.Add(Project.CreateProject(dbProject));
            }

            return projects;
        }

        /// <summary>
        /// Returns a shallow-copied list of a single project by project id.
        /// </summary>
        public Project GetProjectByProjectId(int projectId)
        {
            Project requestedProject = null;
            Data.Project requestedDbProject = (from p in _appInfo.GcContext.Projects
                                       where p.ProjectID == projectId
                                       select p).FirstOrDefault();

            if (requestedDbProject != null)
            {
                requestedProject = Project.CreateProject(requestedDbProject);
            }

            return requestedProject;
        }

        /// <summary>
        /// Returns child tasks for the project with the provided project id.
        /// </summary>
        public List<Task> GetChildTasks(int projectId)
        {
            List<Task> childTasks = new List<Task>();
            List<Data.Task> requestedDbTasks = (from t in _appInfo.GcContext.Tasks
                                                      where t.Projects.FirstOrDefault().ProjectID == projectId
                                                      select t).ToList();

            foreach (Data.Task childDbTask in requestedDbTasks)
            {
                childTasks.Add(Task.CreateTask(childDbTask));
            }

            return childTasks;
        }

        #endregion // Public Interface

        #region Private Helpers

        #endregion // Private Helpers

    }
}