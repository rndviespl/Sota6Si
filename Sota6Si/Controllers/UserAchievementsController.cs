using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sota6Si.Data;
using Sota6Si.Models;

namespace Sota6Si.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAchievementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserAchievementsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserAchievements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserHasAchievement>>> GetUserAchievements()
        {
            return await _context.UserHasAchievements.ToListAsync();
        }

        // GET: api/UserAchievements/{userProjId}/{achievementId}
        [HttpGet("{userProjId}/{achievementId}")]
        public async Task<ActionResult<UserHasAchievement>> GetUserAchievement(int userProjId, int achievementId)
        {
            var userAchievement = await _context.UserHasAchievements
                .FirstOrDefaultAsync(ua => ua.DpUserProjId == userProjId && ua.AchievementId == achievementId);

            if (userAchievement == null)
            {
                return NotFound();
            }

            return userAchievement;
        }

        // POST: api/UserAchievements/Create/{userProjId}/{achievementId}
        [HttpPost("Create/{userProjId}/{achievementId}")]
        public async Task<ActionResult<UserHasAchievement>> CreateUserAchievement(int userProjId, int achievementId)
        {
            // Проверка, существует ли уже связь
            var existingUserAchievement = await _context.UserHasAchievements
                .FirstOrDefaultAsync(ua => ua.DpUserProjId == userProjId && ua.AchievementId == achievementId);

            if (existingUserAchievement != null)
            {
                return Conflict(); // Возвращаем конфликт, если связь уже существует
            }

            // Создание новой связи
            var userAchievement = new UserHasAchievement
            {
                DpUserProjId = userProjId,
                AchievementId = achievementId,
                IsObtained = false // По умолчанию достижение не получено
            };

            _context.UserHasAchievements.Add(userAchievement);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserAchievement", new { userProjId = userAchievement.DpUserProjId, achievementId = userAchievement.AchievementId }, userAchievement);
        }

        // PUT: api/UserAchievements/Unlock/{userProjId}/{achievementId}
        [HttpPut("Unlock/{userProjId}/{achievementId}")]
        public async Task<IActionResult> UnlockUserAchievement(int userProjId, int achievementId)
        {
            // Check if the user achievement exists
            bool exists = await UserAchievementExists(userProjId, achievementId);

            if (!exists)
            {
                // If it doesn't exist, create it
                await CreateUserAchievement(userProjId, achievementId);
            }

            // Retrieve the user achievement
            var userAchievement = await _context.UserHasAchievements
                .FirstOrDefaultAsync(ua => ua.DpUserProjId == userProjId && ua.AchievementId == achievementId);

            if (userAchievement == null)
            {
                return NotFound();
            }

            // Toggle the IsObtained status
            userAchievement.IsObtained = !userAchievement.IsObtained;

            _context.Entry(userAchievement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserAchievementExists(userProjId, achievementId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // GET: api/UserAchievements/Completed/{username}
        [HttpGet("Completed/{username}")]
        public async Task<ActionResult<IEnumerable<Achievement>>> GetCompletedAchievementsByUsername(string username)
        {
            // Получение проекта пользователя по логину
            var userProj = await _context.DpUserProjs.FirstOrDefaultAsync(up => up.Login == username);
            if (userProj == null)
            {
                return NotFound("User project not found.");
            }

            // Получение выполненных достижений
            var completedAchievements = await _context.UserHasAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.DpUserProjId == userProj.DpUserProjId && ua.IsObtained)
                .Select(ua => ua.Achievement)
                .ToListAsync();

            return completedAchievements;
        }

        // GET: api/UserAchievements/Exists/{userProjId}/{achievementId}
        [HttpGet("Exists/{userProjId}/{achievementId}")]
        public async Task<bool> UserAchievementExists(int userProjId, int achievementId)
        {
            return await _context.UserHasAchievements
                .AnyAsync(ua => ua.DpUserProjId == userProjId && ua.AchievementId == achievementId);
        }
    }
} 
