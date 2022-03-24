using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Logic;
using MarketingBox.Redistribution.Service.Postgres;
using MarketingBox.Sdk.Common.Models;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.EntityFrameworkCore;

namespace MarketingBox.Redistribution.Service.Services
{
    public class RegistrationImporter: IRegistrationImporter
    {
        private readonly DatabaseContextFactory _databaseContextFactory;

        public RegistrationImporter(DatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task<Response<ImportResponse>> ImportAsync(ImportRequest request)
        {
            try
            {
                var registrationsFile = new RegistrationsFile()
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.UserId,
                    File = request.RegistrationsFile
                };
                await using var ctx = _databaseContextFactory.Create();
                ctx.RegistrationsFileCollection.Upsert(registrationsFile);

                return new Response<ImportResponse>()
                {
                    Status = ResponseStatus.Ok,
                    Data = new ImportResponse()
                    {
                        FileId = registrationsFile.Id
                    }
                };
            }
            catch (Exception ex)
            {
                return new Response<ImportResponse>()
                {
                    Status = ResponseStatus.InternalError,
                    Error = new Error()
                    {
                        ErrorMessage = ex.Message
                    }
                };
            }
        }

        public async Task<Response<GetRegistrationFilesResponse>> GetRegistrationFilesAsync()
        {
            try
            {
                await using var ctx = _databaseContextFactory.Create();
                var files = await ctx.RegistrationsFileCollection.ToListAsync();

                return new Response<GetRegistrationFilesResponse>()
                {
                    Status = ResponseStatus.Ok,
                    Data = new GetRegistrationFilesResponse()
                    {
                        Files = files
                    }
                };
            }
            catch (Exception ex)
            {
                return new Response<GetRegistrationFilesResponse>()
                {
                    Status = ResponseStatus.InternalError,
                    Error = new Error()
                    {
                        ErrorMessage = ex.Message
                    }
                };
            }
        }

        public async Task<Response<List<RegistrationFromFile>>> GetRegistrationsFromFileAsync(GetRegistrationsFromFileRequest request)
        {
            try
            {
                await using var ctx = _databaseContextFactory.Create();
                var registrationsFile = await ctx.RegistrationsFileCollection.FirstOrDefaultAsync(e => e.Id == request.FileId);

                if (registrationsFile == null)
                    return new Response<List<RegistrationFromFile>>()
                    {
                        Status = ResponseStatus.NotFound,
                        Error = new Error()
                        {
                            ErrorMessage = $"Cant find file with id {request.FileId}."
                        }
                    };
                
                var registrationsFromSvc =
                    await RegistrationsSvcParser.GetRegistrationsFromFile(registrationsFile.File);
                
                return new Response<List<RegistrationFromFile>>()
                {
                    Status = ResponseStatus.Ok,
                    Data = registrationsFromSvc
                };
            }
            catch (Exception ex)
            {
                return new Response<List<RegistrationFromFile>>()
                {
                    Status = ResponseStatus.InternalError,
                    Error = new Error()
                    {
                        ErrorMessage = ex.Message
                    }
                };
            }
        }
    }
}
