using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by TaskData's TaskDeleted event.
    /// </summary>
    public class TaskDeletedEventArgs : EventArgs
    {
        public TaskDeletedEventArgs(Task deletedTask)
        {
            this.DeletedTask = deletedTask;
        }

        public Task DeletedTask { get; private set; }
    }
}