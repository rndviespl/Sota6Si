using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class DpUser
{
    public int DpUserId { get; set; }

    public string DpUsername { get; set; } = null!;

    public string DpPassword { get; set; } = null!;

    public string? DpEmail { get; set; }

    public string? DpFullName { get; set; }

    public DateTime DpRegistrationDate { get; set; }

    public string? DpPhoneNumber { get; set; }

    [JsonIgnore]
    public virtual ICollection<DpOrder> DpOrders { get; set; } = new List<DpOrder>();
}
