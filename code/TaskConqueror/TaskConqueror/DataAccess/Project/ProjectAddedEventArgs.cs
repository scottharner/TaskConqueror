using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by ProjectData's :ProjectAdded event.
    /// </summary>
    public class ProjectAddedEventArgs : EventArgs
    {
        public ProjectAddedEventArgs(Project newProject)
        {
            this.NewProject = newProject;
        }

        public Project NewProject { get; private set; }
    }
}