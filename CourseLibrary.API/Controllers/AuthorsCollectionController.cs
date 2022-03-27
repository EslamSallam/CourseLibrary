using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/AuthorsCollection")]
    public class AuthorsCollectionController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICourseLibraryRepository _repository;

        public AuthorsCollectionController(ICourseLibraryRepository repository, IMapper mapper)
        {
            this._mapper = mapper;
            this._repository = repository;
        }

        [HttpGet("{ids}", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection(
        [FromRoute( Name = "ids")]
        [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorEntities = _repository.GetAuthors(ids);
            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsDTO = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsDTO);
        }


        [HttpPost]
        public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection( IEnumerable<AuthorForCreationDto> authors)
        {
            if (authors == null)
            {
                return BadRequest();
            }
            var AuthorsEntity = _mapper.Map<List<Entities.Author>>(authors);
            _repository.AddAuthors(AuthorsEntity);
            _repository.Save();
            var authorsDto = _mapper.Map<List<AuthorDto>>(AuthorsEntity);
            var idsAsString = string.Join(",", authorsDto.Select(a => a.Id));
            //string uri = $"api/AuthorsCollection/GetAuthorCollection?ids=({idsAsString})";

            
            return CreatedAtRoute("GetAuthorCollection", new { ids = idsAsString}, authorsDto);
        }
    }
}
