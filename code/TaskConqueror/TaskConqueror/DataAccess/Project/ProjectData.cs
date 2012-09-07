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
    public class ProjectData : IDisposable
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

            this.ProjectAdded += this.AllProjectsOnProjectAdded;
            this.ProjectUpdated += this.AllProjectsOnProjectUpdated;
            this.ProjectDeleted += this.AllProjectsOnProjectDeleted;
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
        public void UpdateProject(Project project, TaskData taskData)
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

                taskData.UpdateTasksByProject(project.ProjectId);
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
        public void DeleteProject(Project project, TaskData taskData)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            if (project.ProjectId == 0)
                throw new InvalidOperationException("delete project");

            var query = (from p in _appInfo.GcContext.Projects
                         where p.ProjectID == project.ProjectId
                         select p).First();

            Project requestedProject = Project.CreateProject(query);

            // delete child tasks first
            List<Task> childTasks = this.GetChildTasks(requestedProject.ProjectId);

            foreach (Task childTask in childTasks)
            {
                taskData.DeleteTask(childTask);
            }

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
        public List<Project> GetProjects(string filterTerm = "", SortableProperty sortColumn = null, int? pageNumber = null)
        {
            QueryCacheItem allProjectsCacheItem = _appInfo.GlobalQueryCache.GetCacheItem(Constants.AllProjectsCacheItem, filterTerm);
            List<Data.Project> allProjectsList;

            // retrieve the query from cache if available
            // this will avoid retrieving all records when only a page is needed
            if (allProjectsCacheItem == null)
            {
                IQueryable<Data.Project> allProjects = GetAllProjectsQuery(filterTerm);

                allProjectsList = GetOrderedList(allProjects, sortColumn);

                _appInfo.GlobalQueryCache.AddCacheItem(Constants.AllProjectsCacheItem, filterTerm, sortColumn, allProjectsList);
            }
            else
            {
                allProjectsList = (List<Data.Project>)allProjectsCacheItem.Value;

                if (allProjectsCacheItem.SortColumn != sortColumn)
                {
                    _appInfo.GlobalQueryCache.UpdateCacheItem(Constants.AllProjectsCacheItem, filterTerm, sortColumn, allProjectsList);
                    SortList(allProjectsList, sortColumn);
                }
            }
            
            if (pageNumber.HasValue)
            {
                allProjectsList = allProjectsList.Skip(Constants.RecordsPerPage * (pageNumber.Value - 1))
                                .Take(Constants.RecordsPerPage).ToList();
            }

            List<Project> projects = new List<Project>();

            foreach (Data.Project dbProject in allProjectsList)
            {
                projects.Add(Project.CreateProject(dbProject));
            }

            return projects;
        }

        /// <summary>
        /// Returns the count of all projects in the repository.
        /// </summary>
        public int GetProjectsCount(string filterTerm = "")
        {
            IQueryable<Data.Project> allProjects = GetAllProjectsQuery(filterTerm);

            return allProjects.Count();
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
        /// Returns a shallow-copied list of all completed projects within the specified date range.
        /// </summary>
        public List<Project> GetCompletedProjectsByDate(DateTime startDate, DateTime endDate)
        {
            List<Data.Project> dbProjects = (from p in _appInfo.GcContext.Projects
                                             where p.StatusID == Statuses.Completed && 
                                             p.CompletedDate >= startDate && 
                                             p.CompletedDate <= endDate
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

        public void AddProjectsToGoal(Goal parentGoal, List<int> childProjectIds)
        {
            // add projects to goal
            Data.Goal parentDbGoal = (from g in _appInfo.GcContext.Goals
                                            where g.GoalID == parentGoal.GoalId
                                            select g).FirstOrDefault();
            foreach (int projectId in childProjectIds)
            {
                Data.Project childDbProject = (from p in _appInfo.GcContext.Projects
                                         where p.ProjectID == projectId
                                         select p).FirstOrDefault();
                parentDbGoal.Projects.Add(childDbProject);

                _appInfo.GcContext.SaveChanges();

                if (this.ProjectUpdated != null)
                    this.ProjectUpdated(this, new ProjectUpdatedEventArgs(Project.CreateProject(childDbProject)));
            }
        }

        public void PurgeAbandonedProjects(TaskData taskData)
        {
            List<Data.Project> abandonedDbProjects = (from p in _appInfo.GcContext.Projects
                                                where p.StatusID == Statuses.Abandoned
                                                select p).ToList();

            foreach (Data.Project abandonedDbProject in abandonedDbProjects)
            {
                Project abandonedProject = Project.CreateProject(abandonedDbProject);

                this.DeleteProject(abandonedProject, taskData);
            }
        }

        public void PurgeCompletedProjects(TaskData taskData)
        {
            List<Data.Project> completedDbProjects = (from p in _appInfo.GcContext.Projects
                                                where p.StatusID == Statuses.Completed
                                                select p).ToList();

            foreach (Data.Project completedDbProject in completedDbProjects)
            {
                Project completedProject = Project.CreateProject(completedDbProject);

                this.DeleteProject(completedProject, taskData);
            }
        }

        /// <summary>
        /// Cleans up event handlers when finished using this object
        /// </summary>
        public virtual void Dispose()
        {
            foreach (Delegate d in ProjectAdded.GetInvocationList())
            {
                ProjectAdded -= (EventHandler<TaskConqueror.ProjectAddedEventArgs>)d;
            }

            foreach (Delegate d in ProjectUpdated.GetInvocationList())
            {
                ProjectUpdated -= (EventHandler<TaskConqueror.ProjectUpdatedEventArgs>)d;
            }

            foreach (Delegate d in ProjectDeleted.GetInvocationList())
            {
                ProjectDeleted -= (EventHandler<TaskConqueror.ProjectDeletedEventArgs>)d;
            }
        }

        /// <summary>
        /// Triggers update event handlers on all child projects when a goal is updated
        /// </summary>
        public void UpdateProjectsByGoal(int goalId)
        {
            if (this.ProjectUpdated != null)
            {
                List<Data.Project> childProjects = (from p in _appInfo.GcContext.Projects
                                                    where p.Goals.FirstOrDefault().GoalID == goalId
                                                    select p).ToList();

                foreach (var child in childProjects)
                {
                    this.ProjectUpdated(this, new ProjectUpdatedEventArgs(Project.CreateProject(child)));
                }
            }
        }

        #endregion // Public Interface

        #region Private Helpers

        private List<Data.Project> GetOrderedList(IQueryable<Data.Project> projectsQuery, SortableProperty sortColumn = null)
        {
            List<Data.Project> dbProjectList;

            if (sortColumn == null)
            {
                dbProjectList = projectsQuery.OrderBy(t => t.Title).ToList();
            }
            else
            {
                switch (sortColumn.Name)
                {
                    case "StatusId":
                        dbProjectList = projectsQuery.OrderBy(t => t.StatusID).ToList();
                        break;
                    case "EstimatedCost":
                        dbProjectList = projectsQuery.OrderBy(t => t.EstimatedCost).ToList();
                        break;
                    case "GoalTitle":
                        dbProjectList = projectsQuery.OrderBy(t => t.Goals.FirstOrDefault() == null ? "" : t.Goals.FirstOrDefault().Title).ToList();
                        break;
                    case "CreatedDate":
                        dbProjectList = projectsQuery.OrderBy(t => t.CreatedDate).ToList();
                        break;
                    case "CompletedDate":
                        dbProjectList = projectsQuery.OrderBy(t => t.CompletedDate).ToList();
                        break;
                    default:
                        dbProjectList = projectsQuery.OrderBy(t => t.Title).ToList();
                        break;
                }
            }

            return dbProjectList;
        }

        private void SortList(List<Data.Project> dbProjectList, SortableProperty sortColumn = null)
        {
            if (sortColumn == null)
            {
                dbProjectList = dbProjectList.OrderBy(t => t.Title).ToList();
            }
            else
            {
                switch (sortColumn.Name)
                {
                    case "StatusId":
                        dbProjectList = dbProjectList.OrderBy(t => t.StatusID).ToList();
                        break;
                    case "EstimatedCost":
                        dbProjectList = dbProjectList.OrderBy(t => t.EstimatedCost).ToList();
                        break;
                    case "GoalTitle":
                        dbProjectList = dbProjectList.OrderBy(t => t.Goals.FirstOrDefault() == null ? "" : t.Goals.FirstOrDefault().Title).ToList();
                        break;
                    case "CreatedDate":
                        dbProjectList = dbProjectList.OrderBy(t => t.CreatedDate).ToList();
                        break;
                    case "CompletedDate":
                        dbProjectList = dbProjectList.OrderBy(t => t.CompletedDate).ToList();
                        break;
                    default:
                        dbProjectList = dbProjectList.OrderBy(t => t.Title).ToList();
                        break;
                }
            }
        }

        private IQueryable<Data.Project> GetAllProjectsQuery(string filterTerm = "")
        {
            IQueryable<Data.Project> dbProjects;

            if (string.IsNullOrEmpty(filterTerm))
            {
                dbProjects = (from p in _appInfo.GcContext.Projects
                              select p);
            }
            else
            {
                dbProjects = (from p in _appInfo.GcContext.Projects
                              where p.Title.Contains(filterTerm)
                              select p);
            }

            return dbProjects;
        }

        /// <summary>
        /// Updates the all projects cached query results when a project is added
        /// </summary>
        void AllProjectsOnProjectAdded(object sender, ProjectAddedEventArgs e)
        {
            QueryCacheItem cachedQuery = _appInfo.GlobalQueryCache.GetCacheItem(Constants.AllProjectsCacheItem);

            if (cachedQuery != null)
            {
                // check if the added item satisfies the filter term
                if (cachedQuery.FilterTerm == null || e.NewProject.Title.Contains(cachedQuery.FilterTerm))
                {
                    // add the added item to the cached query results
                    List<Data.Project> allProjects = (List<Data.Project>)cachedQuery.Value;
                    Data.Project addedProject = _appInfo.GcContext.Projects.FirstOrDefault(p => p.ProjectID == e.NewProject.ProjectId);
                    if (addedProject != null)
                    {
                        allProjects.Add(addedProject);
                        // sort the query results according to the sort column
                        SortList(allProjects, cachedQuery.SortColumn);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the all projects cached query results when a project is deleted
        /// </summary>
        void AllProjectsOnProjectDeleted(object sender, ProjectDeletedEventArgs e)
        {
            QueryCacheItem cachedQuery = _appInfo.GlobalQueryCache.GetCacheItem(Constants.AllProjectsCacheItem);

            if (cachedQuery != null)
            {
                List<Data.Project> allProjects = (List<Data.Project>)cachedQuery.Value;
                Data.Project deletedProject = allProjects.FirstOrDefault(p => p.ProjectID == e.DeletedProject.ProjectId);
                if (deletedProject != null)
                {
                    allProjects.Remove(deletedProject);
                }
            }
        }

        /// <summary>
        /// Updates the all projects cached query results when a project is updated
        /// </summary>
        void AllProjectsOnProjectUpdated(object sender, ProjectUpdatedEventArgs e)
        {
            QueryCacheItem cachedQuery = _appInfo.GlobalQueryCache.GetCacheItem(Constants.AllProjectsCacheItem);

            if (cachedQuery != null)
            {
                // updated the query results if needed
                List<Data.Project> allProjects = (List<Data.Project>)cachedQuery.Value;
                if (cachedQuery.FilterTerm == null || e.UpdatedProject.Title.Contains(cachedQuery.FilterTerm))
                {
                    Data.Project oldProject = allProjects.FirstOrDefault(p => p.ProjectID == e.UpdatedProject.ProjectId);
                    Data.Project newProject = allProjects.FirstOrDefault(p => p.ProjectID == e.UpdatedProject.ProjectId);
                    if (oldProject != null && newProject != null)
                    {
                        allProjects.Remove(oldProject);
                        allProjects.Add(newProject);
                    }
                    else if (newProject != null)
                    {
                        allProjects.Add(newProject);
                    }

                    SortList(allProjects, cachedQuery.SortColumn);
                }
                else
                {
                    // the updated project doesnt meet the filter term so remove if it exists in list
                    Data.Project oldProject = allProjects.FirstOrDefault(p => p.ProjectID == e.UpdatedProject.ProjectId);
                    if (oldProject != null)
                    {
                        allProjects.Remove(oldProject);
                    }
                }
            }
        }

        #endregion // Private Helpers

    }
}