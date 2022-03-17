using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorID}/Courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository,IMapper mapper)
        {
            this._courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            this._mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet()]
        public ActionResult<IEnumerable<CoursesDto>> GetCoursesForAuthor(Guid AuthorID)
        {
            if (!_courseLibraryRepository.CourseHasAuthor(AuthorID))
            {
                return NotFound();
            }
            
            var CoursesForAuthorRepo = _courseLibraryRepository.GetCourses(AuthorID);
            return Ok(_mapper.Map<IEnumerable<CoursesDto>>(CoursesForAuthorRepo));
        }

        [HttpGet("{courseID}", Name = "GetCourseForAuthor")]
        public ActionResult<CoursesDto> GetCoursesForAuthor(Guid AuthorID,Guid CourseID)
        {
            if (!_courseLibraryRepository.CourseHasAuthor(AuthorID))
            {
                return NotFound();
            }

            var CourseForAuthorRepo = _courseLibraryRepository.GetCourse(AuthorID,CourseID);
            return Ok(_mapper.Map<CoursesDto>(CourseForAuthorRepo));
        }

        [HttpPost]
        public ActionResult<CoursesDto> CreateCourseForAuthor(Guid AuthorID , CourseForCreationDto course)
        {
            if (!_courseLibraryRepository.AuthorExists(AuthorID))
            {
                return NotFound();
            }
            
            var courseEntity = _mapper.Map<Entities.Course>(course);
            _courseLibraryRepository.AddCourse(AuthorID, courseEntity);
            _courseLibraryRepository.Save();
            var courseDto = _mapper.Map<CoursesDto>(courseEntity);
            return CreatedAtRoute("GetCourseForAuthor",new {authorID = AuthorID, courseID = courseDto.Id }, courseDto);
        }
    }
}
