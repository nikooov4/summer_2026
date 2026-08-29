using Microsoft.AspNetCore.Mvc;
using Mephi.Cybernetics.Omelea.ReportsConstructor;
using Mephi.Cybernetics.Omelea.ReportsConstructor.Models;
using Mephi.Cybernetics.Race.ReportsDescriptionLanguage;
using System.Threading.Tasks;
//TODO: валидация и нормальные ошибки вместо Internal Server Error


namespace Mephi.Cybernetics.Omelea.ReportsConstructor.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprListController : ControllerBase
    {
        private readonly IApprListRepository _repo;
        private readonly ConceptDtoConverter _dtoConverter;

        public ApprListController(IApprListRepository repo)
        {
            _repo = repo;
            _dtoConverter = new ConceptDtoConverter();
        }

        // GET: api/ApprList/definitions
        [HttpGet("definitions")]
        public async Task<ActionResult<ConceptDefCollectionDto>> GetDefinitions()
        {
            var defs = await _repo.LoadApprListDefsAsync();
            var dto = _dtoConverter.DefinitionsToDto(defs);
            return Ok(dto);
        }

        // GET: api/ApprList/students
        [HttpGet("students")]
        public ActionResult<ConceptDefCollectionDto> GetStudents()
        {
            var defs = _repo.LoadApprListDefsAsync().Result;
            _repo.LoadStudentsAsync(defs).Wait();
            var dto = _dtoConverter.ToStudentDtos(defs);
            return Ok(dto);
        }

        // GET: api/ApprList/marks/def/{conceptName}
        [HttpGet("marks/{conceptName}")]
        public async Task<ActionResult<ApprListMarksDto>> LoadMarks(string conceptName)
        {
            var defs = await _repo.LoadApprListDefsAsync();
            var conceptDef = defs.GetConceptDef(conceptName);
            await _repo.LoadStudentsAsync(defs);
            await _repo.LoadMarksAsync(conceptDef);
            var dto = _dtoConverter.ToApprListMarksDto(conceptDef);
            Console.WriteLine(conceptDef.GetInstances().Count);
            return Ok(dto);
        }

        // PUT: api/ApprList/definitions
        [HttpPut("definitions")]
        public async Task<IActionResult> SaveDefinitions([FromBody] ConceptDefCollectionDto dto)
        {
            var manager = _dtoConverter.DtoToDefinitions(dto);
            await _repo.SaveDefsAsync(manager);
            return NoContent();
        }

        // PUT: api/ApprList/marks/{conceptName}
        [HttpPut("marks/{conceptName}")]
        public async Task<IActionResult> SaveMarks(string conceptName, [FromBody] ApprListMarksDto marks)
        {
            var defs = await _repo.LoadApprListDefsAsync();
            var conceptDef = defs.GetConceptDef(conceptName);
            await _repo.LoadStudentsAsync(defs);
            _dtoConverter.ApplyApprListMarksDto(conceptDef, marks);
            await _repo.SaveMarksAsync(conceptDef);
            return NoContent();
        }
        
        // // POST: api/ApprList/marks/save
        // [HttpPost("marks/save")]
        // public async Task<IActionResult> SaveMarks([FromBody] ConceptDefDto apprListConceptDto)
        // {
        //     var manager = new ConceptManager();
        //     var conceptDef = manager.DefineConcept(apprListConceptDto.Name);
        //     foreach (var attr in apprListConceptDto.Attributes)
        //     {
        //         conceptDef.AddAttribute(attr.Name, ConceptDtoConverter.StringToTypeMapping(attr.Type));
        //     }
        //     await _repo.SaveMarksAsync(conceptDef);
        //     return NoContent();
        // }



        // // GET: api/ApprList/students/dto
        // [HttpGet("students/dto")]
        // public async Task<ActionResult<List<StudentDto>>> GetStudentsDto()
        // {
        //     var defs = await _repo.LoadApprListDefsAsync();
        //     await _repo.LoadStudentsAsync(defs);
        //     var students = _dtoConverter.ToStudentDtos(defs);
        //     return Ok(students);
        // }

        // // POST: api/ApprList/students/dto
        // [HttpPost("students/dto")]
        // public async Task<IActionResult> SaveStudentsDto([FromBody] List<StudentDto> students)
        // {
        //     var defs = await _repo.LoadApprListDefsAsync();
        //     _dtoConverter.ApplyStudentDtos(defs, students);
        //     await _repo.LoadStudentsAsync(defs); // возможно, тут нужен другой метод для сохранения студентов
        //     return NoContent();
        // }
    }
} 