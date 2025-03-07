using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories
{
    public class UserAccountServices : GenericRepository<UserAccount>, IUserAccountServices
    {

        private readonly DataContext _context;

        public UserAccountServices(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<UserAccount> GetUserAccountByEmail(string email)
        {
            var userAccount = await _context.Auths.Where(a => a.Email == email).FirstOrDefaultAsync();

            if (userAccount == null)
            {
                throw new KeyNotFoundException($"No user account for an employee with an email {email}");
            }

            return userAccount;
        }

        public async Task GenerateOTP(int id)
        {
            var auth = await _context.UserAccounts.FirstOrDefaultAsync(a => a.UserId == id);

            if (auth == null)
                return;

            auth.VerificationCode = new Random().Next(1000, 9999);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsVerified(string email)
        {
            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(a => a.Email == email);

            if (userAccount == null)
            {
                throw new KeyNotFoundException($"User account for employee with email: ${email} not found.");
            }

            if (userAccount.SMSVerified || userAccount.EmailVerified)
                return true;

            return false;
        }

        public async Task VerifyOTP(int id, int code)
        {
            var userAccount = await _context.Auths.FirstOrDefaultAsync(a => a.UserId == id);

            if (userAccount == null) throw new KeyNotFoundException("No user account found!");

            if (userAccount.VerificationCode == null) throw new KeyNotFoundException("No verification code found!");

            if (userAccount.VerificationCode != code) throw new ArgumentException("Invalid code!");

            userAccount.SMSVerified = true;
            userAccount.VerificationCode = 0;

            await UpdateAsync(userAccount);
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

        public async Task UpdateEmail(int employeeId, string email)
        {
            var userAccount = await GetByIdAsync(employeeId);

            if (userAccount == null)
            {
                throw new KeyNotFoundException($"User account with id: {employeeId} does not exist.");
            }

            userAccount.Email = email;

            await UpdateAsync(userAccount);
        }
    }
}
