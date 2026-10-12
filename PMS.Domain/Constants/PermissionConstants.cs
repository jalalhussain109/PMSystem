
namespace PMS.Domain.Constants;

public static class PermissionConstants
{
    public const string ClaimType = "permission";

    public static class Projects
    {
        public const string View = "Project.View";
        public const string AssignTeam = "Project.AssignTeam";
        public const string Create = "Project.Create";
        public const string Edit = "Project.Edit";
        public const string Delete = "Project.Delete";
    }

    public static class Tasks
    {
        public const string View = "Task.View";
        public const string Create = "Task.Create";
        public const string Edit = "Task.Edit";
        public const string Delete = "Task.Delete";
    }

    public static class Users
    {
        public const string View = "User.View";
        public const string Create = "User.Create";
        public const string Edit = "User.Edit";
        public const string AssignRoles = "User.AssignRoles";
    }

    public static class Role
    {
        public const string View = "Role.View";
        public const string Create = "Role.Create";
        public const string Edit = "Role.Edit";
        public const string AssignRoles = "Role.AssignRoles";
    }
}