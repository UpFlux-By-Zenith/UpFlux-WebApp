using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository;

public class ApplicationVersionRepository : Repository<ApplicationVersion>, IApplicationVersionRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationVersionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
}