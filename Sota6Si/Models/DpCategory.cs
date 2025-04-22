using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class DpCategory
{
    public int DpCategoryId { get; set; }

    public string DpCategoryTitle { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<DpProduct> DpProducts { get; set; } = new List<DpProduct>();
}
