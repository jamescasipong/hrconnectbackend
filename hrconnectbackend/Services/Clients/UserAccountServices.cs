﻿using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.Sessions;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients
{
    public class UserAccountServices(DataContext context, ILogger<UserAccountServices> logger)
        : GenericRepository<UserAccount>(context), IUserAccountServices
    {

        public Task<UserAccount> CreateUserAccount(UserAccount userAccount)
        {
            throw new NotImplementedException();
        }

        public Task<UserAccount> AutomateCreateUserAccount(UserAccount userAccount)
        {
            throw new NotImplementedException();
        }

        public async Task<UserAccount> GetUserAccountByEmail(string email)
        {
            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(a => a.Email == email);

            return userAccount;
        }

        public async Task<UserAccount> GetUserAccountByRefreshToken(string refreshToken)
        {
            var user = await _context.RefreshTokens
                .Include(a => a.UserAccount)  // Include the UserAccount navigation property
                .Where(a => a.RefreshTokenId == refreshToken)
                .Include(a => a.UserAccount.RefreshTokens)  // Include the RefreshTokens navigation on UserAccount
                .Select(a => a.UserAccount)  // Now select UserAccount
                .FirstOrDefaultAsync();
            
            return user;
        }

        public async Task<string> GenerateOTP(int id, DateTime expiry)
        {
            var auth = await _context.UserAccounts.FirstOrDefaultAsync(a => a.UserId == id);

            if (auth == null) throw new KeyNotFoundException("No user account found!");
            //
            // auth.VerificationCode = new Random().Next(100000, 999999);
            // auth.VerificationCodeExpiry = expiry;
            
            await _context.SaveChangesAsync();

            return "auth.VerificationCode.Value.ToString()";
        }

        public async Task<bool> IsVerified(string email)
        {
            var userAccount = await GetUserAccountByEmail(email);

            if (userAccount == null)
            {
                throw new KeyNotFoundException($"User account for employee with email: ${email} not found.");
            }

            if (userAccount.SmsVerified || userAccount.EmailVerified)
                return true;

            return false;
        }

        public async Task VerifyOTP(int id, int code)
        {
            var userAccount = await _context.Auths.FirstOrDefaultAsync(a => a.UserId == id);

            if (userAccount == null) throw new KeyNotFoundException("No user account found!");
            //
            // if (userAccount.VerificationCode == null) throw new KeyNotFoundException("No verification code found!");
            //
            // if (userAccount.VerificationCode != code) throw new ArgumentException("Invalid code!");
            //
            // userAccount.SMSVerified = true;
            // userAccount.VerificationCode = 0;

            await UpdateAsync(userAccount);
        }

        public async Task<string> RetrievePassword(string email)
        {
            var userAcount = await GetUserAccountByEmail(email);

            if (userAcount == null)
            {
                throw new KeyNotFoundException($"No user account found with an email {email}");
            }

            return userAcount.Password;
        }

        public async Task<string> RetrieveUsername(string email)
        {
            var userAcount = await GetUserAccountByEmail(email);

            if (userAcount == null)
            {
                throw new KeyNotFoundException($"No user account found with an email {email}");
            }

            return userAcount.UserName;
        }

        public async Task UpdateEmail(int employeeId, string email)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(a => a.Id == employeeId);

            if (employee == null)
            {
                throw new KeyNotFoundException($"User account with id: {employeeId} does not exist.");
            }

            employee.Email = email;

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task ResetTokenExist(string token)
        {
            throw new NotImplementedException();
        }

        public async Task<ResetPasswordSession> GetResetPasswordSession(string token)
        {
            var expiredSessions = await _context.ResetPasswordSessions.Where(a => a.ExpiresAt < DateTime.Now).ExecuteDeleteAsync();

            await _context.SaveChangesAsync();

            var session = await _context.ResetPasswordSessions.FirstOrDefaultAsync(s => s.Token == token);

            return session;
        }

        public async Task DeleteResetPassword(string token)
        {
            var deletedCount = await _context.ResetPasswordSessions.FirstOrDefaultAsync(a => a.Token == token);

            _context.ResetPasswordSessions.Remove(deletedCount);
            await _context.SaveChangesAsync();
        }


        public async Task CreatePasswordSession(ResetPasswordSession resetPasswordSession)
        {
            await _context.ResetPasswordSessions.AddAsync(resetPasswordSession);

            await _context.SaveChangesAsync();
        }
    }
}
