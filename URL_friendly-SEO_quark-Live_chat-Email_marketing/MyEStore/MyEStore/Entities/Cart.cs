using System;
using System.Collections.Generic;

namespace MyEStore.Entities;

public partial class Cart
{
    public int CartId { get; set; }

    public string MaKh { get; set; } = null!;

    public int MaHh { get; set; }

    public int SoLuong { get; set; }

    public DateTime NgayThem { get; set; }

    public virtual HangHoa MaHhNavigation { get; set; } = null!;

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
