using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by GoalData's GoalDeleted event.
    /// </summary>
    public class GoalDeletedEventArgs : EventArgs
    {
        public GoalDeletedEventArgs(Goal deletedGoal)
        {
            this.DeletedGoal = deletedGoal;
        }

        public Goal DeletedGoal { get; private set; }
    }
}