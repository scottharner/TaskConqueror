using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskConqueror
{
    // todo - check for proper use of constants

    public class Constants
    {
        public static List<Tuple<string, Type>> Reports = new List<Tuple<string, Type>>()
        {
            new Tuple<string, Type>("Active Tasks", typeof(ActiveTasksReport)),
            new Tuple<string, Type>("Completed Goals", typeof(CompletedGoalsReport)),
            new Tuple<string, Type>("Completed Projects", typeof(CompletedProjectsReport)),
            new Tuple<string, Type>("Completed Tasks", typeof(CompletedTasksReport)),
            new Tuple<string, Type>("Goal Progress", typeof(GoalProgressReport)),
            new Tuple<string, Type>("Project Progress", typeof(ProjectProgressReport)),
        };

        public const int RecordsPerPage = 20;
    }

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

    public class GoalCategories
    {
        public static int Physical = 1;
        public static int Career = 2;
        public static int Financial = 3;
        public static int Relational = 4;
        public static int Spiritual = 5;
        public static int Other = 6;
    }

    public class SortWeights
    {
        public static int Root = 1;
        public static int Goal = 2;
        public static int Project = 3;
        public static int Task = 4;
        public static int Unassigned = 5;
    }

    public class ObjectTypes
    {
        public static string Any = "Any";
        public static string Goal = "Goal";
        public static string Project = "Project";
        public static string Task = "Task";
    }
}
