﻿namespace CosmosQuery.Benchmarks.Data.Models;

public sealed record CredentialsModel
{
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
}
