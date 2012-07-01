using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by ProjectData's ProjectUpdated event.
    /// </summary>
    public class ProjectUpdatedEventArgs : EventArgs
    {
        public ProjectUpdatedEventArgs(Project updatedProject)
        {
            this.UpdatedProject = updatedProject;
        }

        public Project UpdatedProject { get; private set; }
    }
}