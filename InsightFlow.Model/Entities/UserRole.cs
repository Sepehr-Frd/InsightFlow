﻿namespace InsightFlow.Model.Entities;

public class UserRole : BaseEntity
{
    public int UserId { get; init; }

    public User? User { get; init; }

    public int RoleId { get; init; }

    public Role? Role { get; init; }
}