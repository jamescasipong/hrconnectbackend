using hrconnectbackend.Data;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories
{
    public class AuthRepositories
    {
        private readonly DataContext _context;

        public AuthRepositories(DataContext context)
        {
            _context = context;
        }

        public async Task<Auth> GetAuth(int id)
        {
            return await _context.Auths.Include(a => a.Employee).FirstOrDefaultAsync(a => a.AuthEmpId == id);
        }

        public async Task<List<Auth>> GetListAuth()
        {
            return await _context.Auths.Include(a => a.Employee).ToListAsync();
        }

        // CREATE: Adds a new Auth entry to the database
        public async Task<Auth> CreateAuth(Auth auth)
        {
            if (auth == null)
                return null;

            // Add the new Auth entity to the database
            await _context.Auths.AddAsync(auth);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the created entity
            return auth;
        }

        // DELETE: Deletes an existing Auth entry from the database by AuthEmpId
        public async Task<bool> DeleteAuth(int id)
        {
            var auth = await _context.Auths.FindAsync(id);

            if (auth == null)
                return false;

            // Remove the Auth entity from the database
            _context.Auths.Remove(auth);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
