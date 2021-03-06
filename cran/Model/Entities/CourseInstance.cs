﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cran.Model.Entities
{
    public class CourseInstance : CranEntity
    {
        public virtual int IdUser { get; set; }
        public virtual int IdCourse { get; set; }
        public virtual DateTime? StartedAt { get; set; }
        public virtual DateTime? EndedAt { get; set; }

        public virtual CranUser User { get; set; }
        public virtual Course Course { get; set; }

        public virtual IList<CourseInstanceQuestion> CourseInstancesQuestion { get; set; } = new List<CourseInstanceQuestion>();
    }
}
