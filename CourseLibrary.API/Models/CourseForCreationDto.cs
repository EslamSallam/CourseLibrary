using CourseLibrary.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    [CourseTitleMustBeDifferentFromDescriptionAttribute(ErrorMessage = "Title and Description must be different!!")]
    public class CourseForCreationDto // : IValidatableObject
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(100,ErrorMessage ="Max length is 100 chars")]
        public string Title { get; set; }
        [MaxLength(1500)]
        public string Description { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Title == Description)
        //    {
        //        yield return new ValidationResult("The provided description should be different from the title.", new[] { "CourseForCreationDto" });
        //    }
        //}
    }
}
