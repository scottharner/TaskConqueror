using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by TaskData's TaskUpdated event.
    /// </summary>
    public class TaskUpdatedEventArgs : EventArgs
    {
        public TaskUpdatedEventArgs(Task updatedTask, Data.Task updatedDbTask)
        {
            this.UpdatedTask = updatedTask;
            this.UpdatedDbTask = updatedDbTask;
        }

        public Task UpdatedTask { get; private set; }
        public Data.Task UpdatedDbTask { get; private set; }
    }
}