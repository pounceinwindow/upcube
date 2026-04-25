namespace UpperCube.Domain.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;
}

public abstract class Entity : Entity<int>
{
}

public abstract class AuditableEntity<TId> : Entity<TId>
{
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public abstract class AuditableEntity : AuditableEntity<int>
{
}
