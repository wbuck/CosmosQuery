﻿namespace CosmosQuery.Tests.Data.Entities;

public sealed record Backup : EntityBase
{    
    public DateTimeOffset DateCreated { get; init; }
    public BackupLocation Location { get; init; } = default!;
    public ICollection<int> Values { get; init; } = new List<int>();
    public string[] StringValuesArray { get; init; } = Array.Empty<string>();
}


