using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository
{

	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _context;
		private readonly DbSet<T> _dbSet;

		public Repository(ApplicationDbContext context)
		{
			_context = context;
			_dbSet = _context.Set<T>();
		}

		public async Task<IEnumerable<T>> GetAllAsync()
		{
			return await _dbSet.ToListAsync();
		}

		public async Task<T> GetByIdAsync(object id)
		{
			return await _dbSet.FindAsync(id);
		}

		public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
		{
			return await _dbSet.Where(predicate).ToListAsync();
		}

		public async Task AddAsync(T entity)
		{
			await _dbSet.AddAsync(entity);
		}

		public void Update(T entity)
		{
			_dbSet.Update(entity);
		}

		public void Remove(T entity)
		{
			_dbSet.Remove(entity);
		}

		public async Task SaveChangesAsync()
		{
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException ex)
			{
				// Log the exception
				Console.WriteLine($"DbUpdateException: {ex.Message}");

				// Check inner exception
				if (ex.InnerException != null)
				{
					Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
				}

				// Rethrow the exception if needed
				throw;
			}
			catch (Exception ex)
			{
				// Log any other exceptions
				Console.WriteLine($"Exception: {ex.Message}");
				if (ex.InnerException != null)
				{
					Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
				}

				throw;
			}
		}
	}

}
