using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskConqueror
{
    public class Statuses
    {
        public static int New = 1;
        public static int InProgress = 2;
        public static int Completed = 3;
        public static int Abandoned = 4;
    }

    public class TaskPriorities
    {
        public static int Low = 1;
        public static int Medium = 2;
        public static int High = 3;
    }

    public class SortWeights
    {
        public static int Root = 1;
        public static int Goal = 2;
        public static int Project = 3;
        public static int Task = 4;
        public static int Unassigned = 5;
    }
}
