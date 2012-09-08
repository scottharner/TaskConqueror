using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskConqueror
{
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

        public const string AllTasksCacheItem = "AllTasks";
        public const string ActiveTasksCacheItem = "ActiveTasks";
        public const string AllProjectsCacheItem = "AllProjects";
        public const string AllGoalsCacheItem = "AllGoals";
    }

    public static class Statuses
    {
        public const int New = 1;
        public const int InProgress = 2;
        public const int Completed = 3;
        public const int Abandoned = 4;
        public const int PendingResponse = 5;
    }

    public static class TaskPriorities
    {
        public const int Low = 1;
        public const int Medium = 2;
        public const int High = 3;
    }

    public static class GoalCategories
    {
        public const int Physical = 1;
        public const int Career = 2;
        public const int Financial = 3;
        public const int Relational = 4;
        public const int Spiritual = 5;
        public const int Other = 6;
    }

    public static class SortWeights
    {
        public const int Root = 1;
        public const int Goal = 2;
        public const int Project = 3;
        public const int Task = 4;
        public const int Unassigned = 5;
    }

    public static class ObjectTypes
    {
        public const string Any = "Any";
        public const string Goal = "Goal";
        public const string Project = "Project";
        public const string Task = "Task";
    }
}
