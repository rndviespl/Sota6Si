using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sota6Si.Models;

public partial class DpUserProj
{
    public int DpUserProjId { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; } = null!;
    public string Login { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<UserHasAchievement> UserHasAchievements { get; set; } = new List<UserHasAchievement>();
}
