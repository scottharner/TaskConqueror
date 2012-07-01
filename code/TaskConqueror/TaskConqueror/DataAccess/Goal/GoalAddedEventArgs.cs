using System;

namespace TaskConqueror
{
    /// <summary>
    /// Event arguments used by GoalData's GoalAdded event.
    /// </summary>
    public class GoalAddedEventArgs : EventArgs
    {
        public GoalAddedEventArgs(Goal newGoal)
        {
            this.NewGoal = newGoal;
        }

        public Goal NewGoal { get; private set; }
    }
}