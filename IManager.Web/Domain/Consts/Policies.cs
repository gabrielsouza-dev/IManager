namespace IManager.Web.Domain.Consts;

public static class Policies
{
    public const string AdminOrUser = "AdminOrUser";
    public const string StaffOrAdmin = "StaffOrAdmin";

    public static readonly string[] All = { StaffOrAdmin, AdminOrUser };
}