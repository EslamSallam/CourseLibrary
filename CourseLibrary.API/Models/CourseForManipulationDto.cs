using CourseLibrary.API.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    [CourseTitleMustBeDifferentFromDescriptionAttribute(ErrorMessage = "Title and Description must be different!!")]
    public abstract class CourseForManipulationDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(100, ErrorMessage = "Max length is 100 chars")]
        public string Title { get; set; }
        [MaxLength(1500)]
        public virtual string Description { get; set; }

    }
}
