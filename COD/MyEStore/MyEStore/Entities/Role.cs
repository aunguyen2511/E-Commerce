using System;
using System.Collections.Generic;

namespace MyEStore.Entities;

public partial class Role
{
    public string Rid { get; set; } = null!;

    public string NameRole { get; set; } = null!;

    public string NormalizedName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
