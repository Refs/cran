﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cran.Model.Dto
{
    public class CoursesListDto
    {
        public IList<CourseDto> Courses { get; set; } = new List<CourseDto>();
    }
}
