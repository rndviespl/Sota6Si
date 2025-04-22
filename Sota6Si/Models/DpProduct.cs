using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class DpProduct
{
    public int DpProductId { get; set; }

    public decimal DpPrice { get; set; }

    public string DpTitle { get; set; } = null!;

    public int? DpDiscountPercent { get; set; }

    public string? DpDescription { get; set; }

    public int? DpCategoryId { get; set; }

    public decimal DpPurchasePrice { get; set; }
    [JsonIgnore]
    public virtual DpCategory? DpCategory { get; set; }
    [JsonIgnore]
    public virtual ICollection<DpImage> DpImages { get; set; } = new List<DpImage>();
    [JsonIgnore]
    public virtual ICollection<DpProductAttribute> DpProductAttributes { get; set; } = new List<DpProductAttribute>();
}
