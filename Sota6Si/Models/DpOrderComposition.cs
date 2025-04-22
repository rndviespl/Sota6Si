using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class DpOrderComposition
{
    public int DpOrderId { get; set; }

    public int DpAttributesId { get; set; }

    public short DpQuantity { get; set; }

    public decimal DpCost { get; set; }
    [JsonIgnore]
    public virtual DpProductAttribute DpAttributes { get; set; } = null!;
    [JsonIgnore]
    public virtual DpOrder DpOrder { get; set; } = null!;
}
