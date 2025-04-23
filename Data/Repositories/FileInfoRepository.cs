using Microsoft.EntityFrameworkCore;
using SakuraDB_Mini.Models;

namespace SakuraDB_Mini.Data.Repositories
{
    public class FileInfoRepository
    {
        private readonly AppDbContext _context;

        public FileInfoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddFileInfoAsync(ProcessedFileInfo fileInfo)
        {
            await _context.FileInfos.AddAsync(fileInfo);
            await _context.SaveChangesAsync();
        }

        public async Task<ProcessedFileInfo?> GetByMD5Async(string md5)
        {
            return await _context.FileInfos.FirstOrDefaultAsync(f => f.MD5 == md5);
        }

        public async Task<ProcessedFileInfo?> GetBySHA1Async(string sha1)
        {
            return await _context.FileInfos.FirstOrDefaultAsync(f => f.SHA1 == sha1);
        }

        public async Task<List<ProcessedFileInfo>> GetAllAsync()
        {
            return await _context.FileInfos.ToListAsync();
        }
    }
}