﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cran.Model.Entities
{
    public class Question
    {
        public virtual int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string Text { get; set; }

        public virtual string InsertUser { get; set; }
        public virtual DateTime InsertDate { get; set; }

        public virtual string UpdateUser { get; set; }
        public virtual DateTime UpdateDate { get; set; }

        public virtual IList<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}
