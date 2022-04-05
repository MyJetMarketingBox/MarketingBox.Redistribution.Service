using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Logic;
using MarketingBox.Redistribution.Service.Postgres;
using MarketingBox.Sdk.Common.Models;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingBox.Redistribution.Service.Services
{
    public class RegistrationImporter: IRegistrationImporter
    {
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly ILogger<RegistrationImporter> _logger;

        public RegistrationImporter(DatabaseContextFactory databaseContextFactory, 
            ILogger<RegistrationImporter> logger)
        {
            _databaseContextFactory = databaseContextFactory;
            _logger = logger;
        }

        public async Task<Response<ImportResponse>> ImportAsync(ImportRequest request)
        {
            try
            {
                _logger.LogInformation($"RegistrationImporter.ImportAsync receive request {JsonConvert.SerializeObject(request)}");
                
                var registrationsFile = new RegistrationsFile()
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.UserId,
                    File = request.RegistrationsFile
                };
                await using var ctx = _databaseContextFactory.Create();
                ctx.RegistrationsFileCollection.Add(registrationsFile);
                await ctx.SaveChangesAsync();

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
                _logger.LogError(ex, ex.Message);
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
                _logger.LogError(ex, ex.Message);
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
                _logger.LogError(ex, ex.Message);
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
