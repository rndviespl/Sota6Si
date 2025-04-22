using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class DpImage
{
    public int DpImagesId { get; set; }

    public int DpProductId { get; set; }

    public string DpImageTitle { get; set; } = null!;

    public byte[]? ImagesData { get; set; }
    [JsonIgnore]
    public virtual DpProduct DpProduct { get; set; } = null!;
}
