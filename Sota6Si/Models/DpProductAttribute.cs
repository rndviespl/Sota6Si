using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class DpProductAttribute
{
    public int DpAttributesId { get; set; }

    public int DpProductId { get; set; }

    public int DpCount { get; set; }

    public int? DpColorId { get; set; }

    public int? DpSize { get; set; }
    [JsonIgnore]
    public virtual DpProduct DpProduct { get; set; } = null!;
    [JsonIgnore]
    public virtual DpSize? DpSizeNavigation { get; set; }
}
