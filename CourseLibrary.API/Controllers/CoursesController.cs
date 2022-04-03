using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        public CoursesController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
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
        public ActionResult<CoursesDto> GetCoursesForAuthor(Guid AuthorID, Guid CourseID)
        {
            if (!_courseLibraryRepository.CourseHasAuthor(AuthorID))
            {
                return NotFound();
            }

            var CourseForAuthorRepo = _courseLibraryRepository.GetCourse(AuthorID, CourseID);
            return Ok(_mapper.Map<CoursesDto>(CourseForAuthorRepo));
        }

        [HttpPost]
        public ActionResult<CoursesDto> CreateCourseForAuthor(Guid AuthorID, CourseForCreationDto course)
        {
            if (!_courseLibraryRepository.AuthorExists(AuthorID))
            {
                return NotFound();
            }

            var courseEntity = _mapper.Map<Entities.Course>(course);
            _courseLibraryRepository.AddCourse(AuthorID, courseEntity);
            _courseLibraryRepository.Save();
            var courseDto = _mapper.Map<CoursesDto>(courseEntity);
            return CreatedAtRoute("GetCourseForAuthor", new { authorID = AuthorID, courseID = courseDto.Id }, courseDto);
        }

        [HttpPut("{courseID}")]
        public IActionResult UpdateCourseForAuthor(Guid AuthorID, Guid CourseID, CourseForUpdateDto courseUpdate)
        {
            if (!_courseLibraryRepository.AuthorExists(AuthorID))
            {
                return NotFound();
            }

            var course = _courseLibraryRepository.GetCourse(AuthorID, CourseID);
            if (course == null)
            {
                var courseToAdd = _mapper.Map<Entities.Course>(courseUpdate);
                courseToAdd.Id = CourseID;

                _courseLibraryRepository.AddCourse(AuthorID, courseToAdd);

                _courseLibraryRepository.Save();

                var courseToReturn = _mapper.Map<CoursesDto>(courseToAdd);

                return CreatedAtRoute("GetCourseForAuthor", new { authorID = AuthorID, courseID = courseToReturn.Id }, courseToReturn);
            }

            // map the entity to a courseForUpdateDto
            // apply the updated field values to that dto
            // map the CourseForUpdateDto back to an entity
            _mapper.Map(courseUpdate, course);

            _courseLibraryRepository.UpdateCourse(course);

            _courseLibraryRepository.Save();

            return NoContent();

        }

        [HttpPatch("{CourseID}")]
        public ActionResult PartiallyUpdateCourseForAuthor(Guid AuthorID,Guid CourseID,JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if (!_courseLibraryRepository.AuthorExists(AuthorID))
            {
                return NotFound();
            }

            var course = _courseLibraryRepository.GetCourse(AuthorID, CourseID);
            if (course == null)
            {
                return NotFound();
            }

            // map the entity to a courseForUpdateDto
            // apply the updated field values to that dto
            // map the CourseForUpdateDto back to an entity
            var courseToPatch = _mapper.Map<CourseForUpdateDto>(course);
            //Add validations here
            patchDocument.ApplyTo(courseToPatch,ModelState);

            if (!TryValidateModel(courseToPatch))
            {
                var courseDto = new CourseForUpdateDto();
                patchDocument.ApplyTo(courseDto);
                // Model validation check
                if (!TryValidateModel(courseDto))
                {
                    return ValidationProblem(ModelState);
                }
                var courseToAdd = _mapper.Map<Entities.Course>(courseDto);
                courseToAdd.Id = CourseID;

                _courseLibraryRepository.AddCourse(AuthorID, courseToAdd);
                _courseLibraryRepository.Save();

                var courseToReturn = _mapper.Map<CoursesDto>(courseToAdd);

                return CreatedAtRoute("GetCourseForAuthor", new { AuthorID, CourseID = courseToReturn.Id }, courseToReturn);

            }

            _mapper.Map(courseToPatch, course);

            _courseLibraryRepository.UpdateCourse(course);

            _courseLibraryRepository.Save();

            return NoContent();
        }

        [HttpDelete("courseId")]
        public ActionResult DeleteCourseForAuthor(Guid authorId,Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseToDelete = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseToDelete == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteCourse(courseToDelete);
            _courseLibraryRepository.Save();

            return NoContent();
        }

        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}
