using System;
using System.Collections.Generic;

namespace blog_app_ai_dotnet.models;

public partial class AuthenticationUser
{
    public long Id { get; set; }

    public string Password { get; set; } = null!;

    public DateTime? LastLogin { get; set; }

    public bool IsSuperuser { get; set; }

    public string Username { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsStaff { get; set; }

    public bool IsActive { get; set; }

    public DateTime DateJoined { get; set; }

    public string? Bio { get; set; }

    public string ProfileImage { get; set; } = null!;

    public string BackgroundImage { get; set; } = null!;

    public int FriendsNumber { get; set; }
}
