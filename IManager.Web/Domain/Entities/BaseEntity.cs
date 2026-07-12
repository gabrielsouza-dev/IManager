using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Entities.Users;

namespace IManager.Web.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public Guid? LastModifierId { get; set; }
    public User? LastModifier { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }

    public void Deactivate(Guid? id = null)
    {
        LastModified = DateTime.UtcNow;
        LastModifierId = id;

        if (!IsActive)
        {
            IsActive = true;
            DeletedAt = null;
            return;
        }

        IsActive = false;
        DeletedAt = DateTime.UtcNow;
    }

    public void Touch(Guid? id = null)
    {
        LastModified = DateTime.UtcNow;
        LastModifierId = id;
    }
}