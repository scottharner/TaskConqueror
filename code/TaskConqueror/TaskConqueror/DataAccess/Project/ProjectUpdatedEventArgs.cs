using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by ProjectData's ProjectUpdated event.
    /// </summary>
    public class ProjectUpdatedEventArgs : EventArgs
    {
        public ProjectUpdatedEventArgs(Project updatedProject, Data.Project updatedDbProject)
        {
            this.UpdatedProject = updatedProject;
            this.UpdatedDbProject = updatedDbProject;
        }

        public Project UpdatedProject { get; private set; }
        public Data.Project UpdatedDbProject { get; private set; }
    }
}