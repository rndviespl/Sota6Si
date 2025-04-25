using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class DpCategory
{
    public int DpCategoryId { get; set; }

    public string DpCategoryTitle { get; set; } = null!;

    public int SizeId { get; set; } 

    [JsonIgnore]
    public virtual DpSize Size { get; set; } = null!; 

    [JsonIgnore]
    public virtual ICollection<DpProduct> DpProducts { get; set; } = new List<DpProduct>();
}
