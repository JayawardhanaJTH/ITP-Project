using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class TeacherRel
    {
        [Required]
        public int teacher_id { get; set; }
        [Required]
        public int subject_id { get; set; }
        [Required]
        public int grade_id { get; set; }
        public string teacherName { get; set; }
        public string subject { get; set; }
        public string grade { get; set; }
        

    }
}