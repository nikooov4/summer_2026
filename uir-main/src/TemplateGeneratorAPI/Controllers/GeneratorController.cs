using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TemplateGeneratorAPI.Services.Generation;
using TemplateGeneratorAPI.Services.Latex;
using TemplateGeneratorAPI.SubjectDomain;

namespace TemplateGeneratorAPI.Controllers
{

    [Route("api/generate")]
    [ApiController]
    public class GeneratorController : Controller
    {
        private readonly IConfiguration _configuration;

        public GeneratorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Generate(string id)
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

                    connection.CloseAsync();
                }

                if (fileName == null || mimeType == null || binaryFile == null)
                {
                    throw new Exception("No such template file");
                }

                var superviser = new Academic()
                {
                    Name = "Николай",
                    Surname = "Николаев",
                    Patronymic = "Николаевич",
                };

                var consultant = new Academic()
                {
                    Name = "Алексей",
                    Surname = "Алексеев",
                    Patronymic = "Алексеевич",
                };

                var commissionMember1 = new Academic()
                {
                    Name = "Иван",
                    Surname = "Иванов",
                    Patronymic = "Иванович",
                };

                var commissionMember2 = new Academic()
                {
                    Name = "Петр",
                    Surname = "Петров",
                    Patronymic = "Петрович",
                };

                var student = new Student()
                {
                    Name = "Федр",
                    Surname = "Федоров",
                    Patronymic = "Федорович",
                };

                var group = new Group()
                {
                    Code = "Б19-504",
                };

                var course = new Course()
                {
                    Code = "09.03.04",
                    Name = "Программная инженерия",
                };

                course.Groups = new List<Group>(){
                    group,
                };

                group.Course = new List<Course>(){
                    course,
                };
                group.Students = new List<Student>(){
                    student,
                };

                var fqw = new FinalQualifyingWork()
                {
                    Topic = "Генерация документов",
                    Student = student,
                    Superviser = superviser,
                    Consultants = new List<Academic>(){
                        consultant,
                    },
                    CommissionMembers = new List<Academic>() {
                        commissionMember1,
                        commissionMember2,
                    },
                    CommissionDate = new DateTime(2022, 12, 20).ToShortDateString(),
                };

                student.Group = group;
                student.FinalQualifyingWork = fqw;

                using (var stream = new MemoryStream())
                {
                    var tg = new TemplateGeneratorED(fileName, binaryFile, typeof(FinalQualifyingWork));
                    tg.GenerateBySubjectDomain(fqw, stream);
                    binaryFile = stream.ToArray();
                }

                var fileInfo = new FileInfo(fileName);

                if (fileInfo.Extension == ".tex")
                {
                    using (var latex = new Latex())
                    {
                        binaryFile = latex.CompilePDF(binaryFile);

                        fileInfo = new FileInfo(latex.GetPdfFileName());
                        mimeType = MediaTypeNames.Application.Pdf;
                    }
                }

                fileName = Path.GetFileNameWithoutExtension(fileInfo.Name) + "Result" + fileInfo.Extension;

                return new FileContentResult(binaryFile, mimeType)
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult("Error during template get: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}