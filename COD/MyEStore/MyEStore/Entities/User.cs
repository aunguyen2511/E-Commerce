using System;
using System.Collections.Generic;

namespace MyEStore.Entities;

public partial class User
{
    public string Uid { get; set; } = null!;

    public string Rid { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual Role RidNavigation { get; set; } = null!;
}
