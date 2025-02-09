using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories
{
    public class UserAccountServices : GenericRepository<UserAccount>, IUserAccountServices
    {

        public UserAccountServices(DataContext context) : base(context)
        {

        }


        public async Task<UserAccount> GetUserAccountByEmail(string email)
        {
            var userAccount = await _context.Auths.Select(a => a.Employee).Where(a => a.Email == email).Select(a => a.UserAccount).FirstOrDefaultAsync();

            if (userAccount == null)
            {
                throw new KeyNotFoundException($"No user account for an employee with an email {email}");
            }

            return userAccount;
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
            var auth = await _context.UserAccounts.FirstOrDefaultAsync(a => a.UserId == id);

            if (auth == null)
                return;

            auth.VerificationCode = new Random().Next(1000, 9999);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsVerified(int id)
        {
            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(a => a.UserId == id);

            if (userAccount == null)
            {
                throw new KeyNotFoundException($"No user account for an employee with an ${id}");
            }

            if (userAccount.SMSVerified || userAccount.EmailVerified)
                return true;

            return false;
        }

        public async Task<string> VerifyOTP(int id, int code)
        {
            var auth = await _context.Auths.FirstOrDefaultAsync(a => a.UserId == id);

            if (auth == null) return "Auth not found!";

            if (auth.VerificationCode == 0) return "No code generated!";

            if (auth.VerificationCode != code) return "Invalid code!";

            auth.SMSVerified = true;
            auth.VerificationCode = 0;

            await _context.SaveChangesAsync();

            return "Code confirmed!";
        }

        public async Task<string> RetrievePassword(string email)
        {
            var userAcount = await _context.UserAccounts.FirstOrDefaultAsync(user => user.Email == email);

            if (userAcount == null)
            {
                throw new KeyNotFoundException($"No user account found with an email {email}");
            }

            return userAcount.Password;
        }

        public async Task<string> RetrieveUsername(string email)
        {
            var userAcount = await _context.UserAccounts.FirstOrDefaultAsync(user => user.Email == email);

            if (userAcount == null)
            {
                throw new KeyNotFoundException($"No user account found with an email {email}");
            }

            return userAcount.UserName;
        }
    }
}
