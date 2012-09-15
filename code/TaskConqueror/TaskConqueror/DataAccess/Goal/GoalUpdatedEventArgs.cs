using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by GoalData's GoalUpdated event.
    /// </summary>
    public class GoalUpdatedEventArgs : EventArgs
    {
        public GoalUpdatedEventArgs(Goal updatedGoal, Data.Goal updatedDbGoal)
        {
            this.UpdatedGoal = updatedGoal;
            this.UpdatedDbGoal = updatedDbGoal;
        }

        public Goal UpdatedGoal { get; private set; }
        public Data.Goal UpdatedDbGoal { get; private set; }
    }
}