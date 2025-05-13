using System.Text.Json.Serialization;

namespace Sota6Si.Models
{
    public class UserHasAchievement
    {
        public int DpUserProjId { get; set; }
        public int AchievementId { get; set; }
        public bool IsObtained { get; set; } = false;

        [JsonIgnore]
        public virtual Achievement Achievement { get; set; } = null!;

        [JsonIgnore]
        public virtual DpUserProj DpUserProj { get; set; } = null!;
    }
}
