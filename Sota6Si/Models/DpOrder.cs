using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class DpOrder
{
    public int DpOrderId { get; set; }

    public int DpUserId { get; set; }

    public DateTime DpDateTimeOrder { get; set; }

    public string? DpTypeOrder { get; set; }
    [JsonIgnore]
    public virtual DpUser DpUser { get; set; } = null!;
}
