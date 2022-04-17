using System.Collections.Generic;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Logic;
using MarketingBox.Redistribution.Service.Postgres;
using Microsoft.EntityFrameworkCore;

namespace MarketingBox.Redistribution.Service.Storage
{
    public class FileStorage
    {
        private readonly DatabaseContextFactory _databaseContextFactory;

        public FileStorage(DatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task Save(RegistrationsFile file)
        {
            await using var ctx = _databaseContextFactory.Create();
            ctx.RegistrationsFileCollection.Add(file);
            await ctx.SaveChangesAsync();
        }

        public async Task<List<RegistrationsFile>> Get()
        {
            await using var ctx = _databaseContextFactory.Create();
            return await ctx.RegistrationsFileCollection.ToListAsync();
        }

        public async Task<List<RegistrationFromFile>?> ParseFile(long fileId)
        {
            await using var ctx = _databaseContextFactory.Create();
            var registrationsFile = await ctx.RegistrationsFileCollection.FirstOrDefaultAsync(e => e.Id == fileId);

            if (registrationsFile == null)
                return null;
            
            return await RegistrationsSvcParser.GetRegistrationsFromFile(registrationsFile.File);
        }
    }
}