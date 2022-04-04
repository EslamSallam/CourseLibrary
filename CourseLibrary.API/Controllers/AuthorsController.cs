using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository,IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ?? 
                throw new ArgumentNullException(nameof(mapper));
        }
        [HttpGet(Name = "GetAuthors")]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            var authorsFromRepo = _courseLibraryRepository.GetAuthors(authorsResourceParameters);

            var previousPageLink = 
                authorsFromRepo.HasPrevious ? CreateAuthorsRecourseUri(authorsResourceParameters, ResourseUriType.PreviousPage) : null;
            var nextPageLink =
                authorsFromRepo.HasNext ? CreateAuthorsRecourseUri(authorsResourceParameters, ResourseUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink,
                nextPageLink
            };
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
        }

        [HttpGet("{authorID}",Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid authorID)
        {
            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorID);

            if (authorFromRepo == null)
            {
                return NotFound();
            }
            return Ok(authorFromRepo);
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }
            var authorEntity = _mapper.Map<Author>(author);
            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();
            var authorDto = _mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor",new {authorID = authorDto.Id},authorDto);
        }

        [HttpDelete("{authorId}")]
        public ActionResult DeleteCourseForAuthor(Guid authorId)
        {
            var AuthorToDelete = _courseLibraryRepository.GetAuthor(authorId);

            if (AuthorToDelete == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteAuthor(AuthorToDelete);
            _courseLibraryRepository.Save();

            return NoContent();
        }
        private string CreateAuthorsRecourseUri(AuthorsResourceParameters authorsResourceParameters,ResourseUriType type)
        {
            switch (type)
            {
                case ResourseUriType.PreviousPage:
                    return Url.Link("GetAuthors",
                    new {
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
                case ResourseUriType.NextPage:
                    return Url.Link("GetAuthors",
                    new
                    {
                        pageNumber = authorsResourceParameters.PageNumber + 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
                default:
                    return Url.Link("GetAuthors",
                    new
                    {
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
            }
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "Delete,Get,Options,Post");
            return Ok();
        }
    }
}
