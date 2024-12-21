using AgileCommercee.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileCommercee.Entities;

public partial class Cart
{
    public int CartId { get; set; }

    public string? MaKh { get; set; }

    public int MaHh { get; set; }

    public int SoLuong { get; set; }

    public DateTime NgayThem { get; set; }

    public virtual HangHoa MaHhNavigation { get; set; } = null!;

    public virtual KhachHang? MaKhNavigation { get; set; }
}
