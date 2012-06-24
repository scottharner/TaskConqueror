using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by TaskData's TaskUpdated event.
    /// </summary>
    public class TaskUpdatedEventArgs : EventArgs
    {
        public TaskUpdatedEventArgs(Task updatedTask)
        {
            this.UpdatedTask = updatedTask;
        }

        public Task UpdatedTask { get; private set; }
    }
}