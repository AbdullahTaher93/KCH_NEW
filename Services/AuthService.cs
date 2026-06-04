using KCH_New.Data;
using KCH_New.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KCH_New.Services
{
    public class AuthService
    {
        public async Task<User?> LoginAsync(string username, string password)
        {
            using var db = new AppDbContext();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
        }

        public async Task<bool> ChangePasswordAsync(string username, string oldPassword, string newUsername, string newPassword)
        {
            using var db = new AppDbContext();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return false;
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash)) return false;
            user.Username = newUsername;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
