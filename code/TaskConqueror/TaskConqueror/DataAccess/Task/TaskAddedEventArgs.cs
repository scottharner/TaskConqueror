using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by TaskData's TaskAdded event.
    /// </summary>
    public class TaskAddedEventArgs : EventArgs
    {
        public TaskAddedEventArgs(Task newTask)
        {
            this.NewTask = newTask;
        }

        public Task NewTask { get; private set; }
    }
}