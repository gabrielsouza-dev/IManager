namespace IManager.Web.Domain.Consts;

/// <summary>
/// Constantes do seed de demonstração.
/// Usuários criados pelo seed recebem a claim <see cref="ClaimType"/>, usada pela página pública /demo-logins.
/// </summary>
public static class DemoSeed
{
    public const string ClaimType = "DemoSeed";

    public const string AdminPassword = "Admin@123";
    public const string UserPassword = "User@123";
}
