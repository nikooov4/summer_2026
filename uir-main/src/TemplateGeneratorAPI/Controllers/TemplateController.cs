using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TemplateGeneratorAPI.Models;

namespace TemplateGeneratorAPI.Controllers
{

    [Route("api/template")]
    [ApiController]
    public class TemplateController : Controller
    {
        private readonly IConfiguration _configuration;

        public TemplateController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetTemplate(string id)
        {
            try
            {
                const string query = @"
                SELECT file_name, mime_type, binary_file FROM templates WHERE id = @id::uuid;
            ";

                var dataSource = _configuration.GetConnectionString("TgApiDb");

                byte[]? binaryFile = null;
                string? mimeType = null;
                string? fileName = null;

                using (var connection = new NpgsqlConnection(dataSource))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            reader.Read();

                            binaryFile = (byte[])reader["binary_file"];
                            mimeType = reader["mime_type"].ToString();
                            fileName = reader["file_name"].ToString();
                        }

                        reader.Close();
                    }

                    connection.Close();
                }

                if (mimeType != null && binaryFile != null && fileName != null)
                    return new FileContentResult(binaryFile, mimeType)
                    {
                        FileDownloadName = fileName
                    };

                throw new Exception("No such template file");
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult("Error during template get: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("all")]
        public IActionResult GetAllTemplates()
        {
            try
            {
                const string query = @"
                SELECT id, file_name FROM templates;
            ";

                var dataSource = _configuration.GetConnectionString("TgApiDb");

                TemplateInfo[] templatesInfos = Array.Empty<TemplateInfo>();

                using (var connection = new NpgsqlConnection(dataSource))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            var id = reader["id"].ToString();
                            var fileName = reader["file_name"].ToString();

                            templatesInfos = templatesInfos.Append(new TemplateInfo { Id = id!, FileName = fileName! }).ToArray();
                        }

                        reader.Close();
                    }

                    connection.Close();
                }

                return new JsonResult(templatesInfos);
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult("Error during template get: " + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UploadTemplate()
        {
            try
            {
                var httpRequestForm = Request.Form;
                var uploadedTemplate = httpRequestForm.Files[0];

                using var memoryStream = new MemoryStream();
                uploadedTemplate.CopyTo(memoryStream);

                var fileName = uploadedTemplate.FileName;
                var mimeType = uploadedTemplate.ContentType;
                var binaryFile = memoryStream.ToArray();

                const string query = @"
                INSERT INTO templates(file_name, mime_type, binary_file) values(@file_name, @mime_type, @binary_file);
            ";

                var dataSource = _configuration.GetConnectionString("TgApiDb");

                using (var connection = new NpgsqlConnection(dataSource))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@binary_file", binaryFile);
                        command.Parameters.AddWithValue("@mime_type", mimeType);
                        command.Parameters.AddWithValue("@file_name", fileName);

                        var reader = command.ExecuteReader();

                        reader.Close();
                    }

                    connection.Close();
                }

                Response.StatusCode = StatusCodes.Status201Created;
                return new JsonResult("Success");
            }
            catch (Exception ex)
            {
                if (ex is NpgsqlException)
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;
                    return new JsonResult("Such file exists");
                }

                Console.WriteLine(ex);
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult("Error during template uploading: " + ex.Message);
            }
        }
    }
}