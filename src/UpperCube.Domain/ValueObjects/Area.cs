using UpperCube.Domain.Enums;

namespace UpperCube.Domain.ValueObjects;

public sealed record Area(decimal Value, AreaUnit Unit = AreaUnit.SquareMeter);
