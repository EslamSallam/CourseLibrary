using CourseLibrary.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    public class CourseForCreationDto : CourseForManipulationDto
    {
        [MaxLength(1200)]
        public override string Description { get => base.Description; set => base.Description = value; }
    }
}
