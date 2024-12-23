using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories
{
    public class AuthRepositories : GenericRepository<Auth>, IAuthRepositories
    {

        public AuthRepositories(DataContext context) : base(context)
        {

        }


        public async Task<Auth> GetAuthByEmail(string email)
        {
            return await _context.Auths.Select(a => a.Employee).Where(a => a.Email == email).Select(a => a.Auth).FirstOrDefaultAsync();
        }

        public async Task<string> ConfirmPhone(int id, int code)
        {
            var verify = await VerifyOTP(id, code);

            return verify;
        }

        public async Task<string> ConfirmEmail(int id, int code)
        {
            var verify = await VerifyOTP(id, code);

            return verify;

        }

        public async Task GenerateOTP(int id)
        {
            var auth = await _context.Auths.FirstOrDefaultAsync(a => a.AuthEmpId == id);

            if (auth == null)
                return;

            auth.VerificationCode = new Random().Next(1000, 9999);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsVerified(int id)
        {
            var auth = await _context.Auths.FirstOrDefaultAsync(a => a.AuthEmpId == id);

            if (auth.EmailConfirmed || auth.PhoneConfirmed)
                return true;

            return false;
        }

        public async Task<string> VerifyOTP(int id, int code)
        {
            var auth = await _context.Auths.FirstOrDefaultAsync(a => a.AuthEmpId == id);

            if (auth == null) return "Auth not found!";

            if (auth.VerificationCode == 0) return "No code generated!";

            if (auth.VerificationCode != code) return "Invalid code!";

            auth.PhoneConfirmed = true;
            auth.VerificationCode = 0;

            await _context.SaveChangesAsync();

            return "Code confirmed!";
        }

    }
}
