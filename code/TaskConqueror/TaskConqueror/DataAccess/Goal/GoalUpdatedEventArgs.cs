using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by GoalData's GoalUpdated event.
    /// </summary>
    public class GoalUpdatedEventArgs : EventArgs
    {
        public GoalUpdatedEventArgs(Goal updatedGoal)
        {
            this.UpdatedGoal = updatedGoal;
        }

        public Goal UpdatedGoal { get; private set; }
    }
}