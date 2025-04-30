using System;
using System.Collections.Generic;

namespace Kahoot.Repository.Models;

public partial class UserPackage
{
    public int UserPackageId { get; set; }

    public int UserId { get; set; }

    public int PackageId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public string? Status { get; set; }

    public bool? IsUpgraded { get; set; }

    public int? PreviousPackageId { get; set; }

    public virtual Package Package { get; set; } = null!;

    public virtual Package? PreviousPackage { get; set; }

    public virtual User User { get; set; } = null!;
}
