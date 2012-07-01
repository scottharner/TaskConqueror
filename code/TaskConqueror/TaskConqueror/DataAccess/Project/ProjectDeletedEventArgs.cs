using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by ProjectData's ProjectDeleted event.
    /// </summary>
    public class ProjectDeletedEventArgs : EventArgs
    {
        public ProjectDeletedEventArgs(Project deletedProject)
        {
            this.DeletedProject = deletedProject;
        }

        public Project DeletedProject { get; private set; }
    }
}