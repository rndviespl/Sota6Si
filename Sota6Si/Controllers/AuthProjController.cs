using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sota6Si.Data;
using Sota6Si.Dto;
using Sota6Si.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sota6Si.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthProjController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthProjController> _logger;

        public AuthProjController(AppDbContext context, IConfiguration configuration, ILogger<AuthProjController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserProjDto userProjDto)
        {
            if (await _context.DpUserProjs.AnyAsync(u => u.Login == userProjDto.Login))
            {
                return BadRequest("User already exists.");
            }

            var userProj = new DpUserProj
            {
                Login = userProjDto.Login,
                Password = userProjDto.Password
            };

            _context.DpUserProjs.Add(userProj);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(userProj);
            return Ok(new { Token = token, UserProjId = userProj.DpUserProjId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserProjDto userProjDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogError("Validation errors: {Errors}", errors);
                return BadRequest(new { Errors = errors });
            }

            var userProj = await _context.DpUserProjs.FirstOrDefaultAsync(u => u.Login == userProjDto.Login && u.Password == userProjDto.Password);

            if (userProj == null)
            {
                return Unauthorized("Invalid login or password.");
            }

            var token = GenerateJwtToken(userProj);
            return Ok(new { Token = token, UserProjId = userProj.DpUserProjId });
        }

        private string GenerateJwtToken(DpUserProj userProj)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userProj.Login),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userProj.Login.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(180),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}