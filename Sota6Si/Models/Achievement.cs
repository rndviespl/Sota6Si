using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class Achievement
{
    public int AchievementId { get; set; }

    public string Title { get; set; } = null!;

    public string? TextAchievement { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<DpUserProj> DpUserProjs { get; set; } = new List<DpUserProj>();
}
