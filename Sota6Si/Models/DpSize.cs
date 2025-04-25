using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class DpSize
{
    public int SizeId { get; set; }

    public string Size { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<DpProductAttribute> DpProductAttributes { get; set; } = new List<DpProductAttribute>();

    [JsonIgnore]
    public virtual ICollection<DpCategory> DpCategories { get; set; } = new List<DpCategory>(); 
}
