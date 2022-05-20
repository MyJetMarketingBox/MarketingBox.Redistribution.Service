using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Logic;
using MarketingBox.Redistribution.Service.Postgres;
using MarketingBox.Sdk.Common.Exceptions;
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

        public async Task<(List<RegistrationsFile>, int)> Search(GetFilesRequest request)
        {
            await using var ctx = _databaseContextFactory.Create();
            IQueryable<RegistrationsFile> query = ctx.RegistrationsFileCollection;
            var total = query.Count();

            if (request.Asc)
            {
                if (request.Cursor != null)
                {
                    query = query.Where(x => x.Id > request.Cursor);
                }

                query = query.OrderBy(x => x.Id);
            }
            else
            {
                if (request.Cursor != null)
                {
                    query = query.Where(x => x.Id < request.Cursor);
                }

                query = query.OrderByDescending(x => x.Id);
            }

            if (request.Take.HasValue)
            {
                query = query.Take(request.Take.Value);
            }

            await query.LoadAsync();

            var result = query.ToList();

            return (result, total);
        }

        public async Task<(List<RegistrationFromFile>, int)> ParseFile(GetRegistrationsFromFileRequest request)
        {
            await using var ctx = _databaseContextFactory.Create();
            var registrationsFile = await ctx.RegistrationsFileCollection.FirstOrDefaultAsync(
                e => e.Id == request.FileId);

            if (registrationsFile is null)
                throw new NotFoundException($"Cant find file with id {request.FileId}.");

            var files = await RegistrationsSvcParser.GetRegistrationsFromFile(request.FileId, registrationsFile.File);
            var count = files.Count;
            if (request.Asc)
            {
                if (request.Cursor != null)
                {
                    files = files.Where(x => x.Index > request.Cursor.Value).ToList();
                }
            }
            else
            {
                if (request.Cursor != null)
                {
                    files = files.Where(x => x.Index < request.Cursor.Value).ToList();
                }

                files.Reverse();
            }

            if (request.Take.HasValue)
            {
                files = files.Take(request.Take.Value).ToList();
            }

            return (files, count);
        }
    }
}