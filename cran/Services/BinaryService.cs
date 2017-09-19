﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cran.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using cran.Data;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using cran.Model.Entities;
using System.Data.Common;

namespace cran.Services
{
    public class BinaryService : Service, IBinaryService
    {

        public BinaryService(ApplicationDbContext context, IDbLogService dbLogService, IPrincipal principal) 
            : base(context, principal)
        {
            
        }

        public async Task<IList<FileDto>> UploadFilesAsync(IList<IFormFile> files)
        {
            IList<FileDto> uploadedFiles = new List<FileDto>();
            foreach (IFormFile formFile in files)
            {
                FileDto uploadedFile = await SaveFileAsync(formFile);
                if(uploadedFile != null)
                {
                    uploadedFiles.Add(uploadedFile);
                }
            }
            return uploadedFiles;
        }

        private async Task<FileDto> SaveFileAsync(IFormFile file)
        {
            if (file.Length > 0)
            {
                FileDto dto = await CreateFileEntity(file);
                await SaveAsync(dto.Id, file.OpenReadStream());
                return dto;
            }
            return null;
        }

        private async Task<FileDto> CreateFileEntity(IFormFile formfile)
        {
            Binary fileEntity = new Binary();
            fileEntity.Length = (int) formfile.Length;
            fileEntity.ContentType = formfile.ContentType;
            fileEntity.FileName = formfile.FileName;
            fileEntity.ContentDisposition = formfile.ContentDisposition;
            fileEntity.Name = formfile.Name;

            _context.Binaries.Add(fileEntity);

            await SaveChangesAsync();
            FileDto dto = ToDto(fileEntity);
            return dto;
        }

        private FileDto ToDto(Binary binary)
        {
            FileDto result = new FileDto()
            {
                Id = binary.Id,
                FileName = binary.FileName,
                Name = binary.Name,
                Length = binary.Length,
                ContentType = binary.ContentType,
                ContentDisposition = binary.ContentDisposition,
            };
            return result;
        }

        public async Task SaveAsync(int id, Stream input)
        {
            using (input) {
                DbConnection connection = _context.Database.GetDbConnection();
                bool connectionOpended = false;
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    connectionOpended = true;
                }

                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE CranBinary SET Data=@Data WHERE ID = @Id";

                    DbParameter idParameter = command.CreateParameter();
                    idParameter.ParameterName = "Id";
                    idParameter.Value = id;
                    command.Parameters.Add(idParameter);


                    DbParameter streamParameter = command.CreateParameter();
                    streamParameter.ParameterName = "@Data";
                    streamParameter.Value = input;
                    command.Parameters.Add(streamParameter);

                    await command.ExecuteNonQueryAsync();
                }

                if (connectionOpended)
                {
                    connection.Close();
                }
            }
        }

        public async Task<Stream> GetBinaryAsync(int id)
        {
            MemoryStream memoryStream = new MemoryStream();
            DbConnection connection = _context.Database.GetDbConnection();
            bool connectionOpended = false;
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
                connectionOpended = true;
            }

            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT Data FROM CranBinary WHERE ID = @Id";

                DbParameter idParameter = command.CreateParameter();
                idParameter.ParameterName = "Id";
                idParameter.Value = id;
                command.Parameters.Add(idParameter);

                using (DbDataReader dr = command.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        byte[] binaryData = (byte[])dr["Data"];
                        memoryStream.Write(binaryData, 0, binaryData.Length);
                    }
                }

            }

            if (connectionOpended)
            {
                connection.Close();
            }

            memoryStream.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        public async Task<FileDto> GetFileInfoAsync(int id)
        {
            Binary binary = await _context.FindAsync<Binary>(id);
            return ToDto(binary);
        }
    }
}
