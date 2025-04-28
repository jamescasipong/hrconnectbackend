// using hrconnectbackend.Data;
// using hrconnectbackend.Models;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
//
// public class AuthenticationService
// {
//     private readonly DataContext _context;
//     private readonly IPasswordHasher<UserAccount> _passwordHasher;
//
//     public AuthenticationService(DataContext context, IPasswordHasher<UserAccount> passwordHasher)
//     {
//         _context = context;
//         _passwordHasher = passwordHasher;
//     }
//
//     public async Task<bool> AuthenticateUserAsync(string username, string password)
//     {
//         var user = await _context.UserAccounts.SingleOrDefaultAsync(u => u.UserName == username);
//
//         if (user == null)
//         {
//             return false; // User not found
//         }
//
//         // Check if the account is locked and if the lock has expired
//         if (user.IsLocked)
//         {
//             if (user.LockExpiry.HasValue && user.LockExpiry.Value > DateTime.UtcNow)
//             {
//                 // Account is still locked
//                 throw new InvalidOperationException("Your account is locked. Please try again later.");
//             }
//             else
//             {
//                 // Unlock account if lock has expired
//                 user.IsLocked = false;
//                 user.LockReason = null;
//                 user.LockExpiry = null;
//                 user.FailedAttempts = 0;  // Reset failed attempts count
//                 await _context.SaveChangesAsync();
//             }
//         }
//
//         // Verify the password
//         var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
//         
//         if (result == PasswordVerificationResult.Success)
//         {
//             // Successful login: Reset failed attempts
//             user.FailedAttempts = 0;
//             await _context.SaveChangesAsync();
//             return true;
//         }
//
//         // Failed login: Increment failed attempt count
//         user.FailedAttempts++;
//         user.LastFailedLogin = DateTime.UtcNow;
//
//         if (user.FailedAttempts >= 5)
//         {
//             // Lock account after 5 failed attempts
//             user.IsLocked = true;
//             user.LockReason = "Too many failed login attempts.";
//             user.LockExpiry = DateTime.UtcNow.AddMinutes(15);  // Lock for 15 minutes
//         }
//
//         await _context.SaveChangesAsync();
//         return false; // Invalid login attempt
//     }
//
//     public async Task UnlockUserAccountAsync(int userId)
//     {
//         var user = await _context.Users.FindAsync(userId);
//
//         if (user != null)
//         {
//             user.IsLocked = false;
//             user.LockReason = null;
//             user.LockExpiry = null;
//             user.FailedAttempts = 0;  // Reset failed attempts count
//             await _context.SaveChangesAsync();
//         }
//     }
// }
