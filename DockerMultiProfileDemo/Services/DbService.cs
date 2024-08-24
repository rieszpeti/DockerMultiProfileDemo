using DockerMultiProfileDemo.Database;
using Microsoft.EntityFrameworkCore;

namespace DockerMultiProfileDemo.Services
{
    public class DbService
    {
        private readonly AppDbContext _context;

        public DbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SomeEntityModel?> ReturnFirst()
        {
            return await _context.SomeEntityModels.FirstOrDefaultAsync();
        }
    }
}
