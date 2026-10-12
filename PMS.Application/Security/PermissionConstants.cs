namespace PMS.Application.Security;

public static class PermissionConstants
{
    public const string ClaimType = "permission";

    public static class Projects
    {
        public const string View = "Projects.View";
        public const string Create = "Projects.Create";
        public const string Edit = "Projects.Edit";
        public const string Delete = "Projects.Delete";
    }

    public static class Tasks
    {
        public const string View = "Tasks.View";
        public const string Create = "Tasks.Create";
        public const string Edit = "Tasks.Edit";
        public const string Delete = "Tasks.Delete";
    }

    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Edit = "Users.Edit";
        public const string Delete = "Users.Delete";
    }
}