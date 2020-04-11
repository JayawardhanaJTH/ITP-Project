using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class TeacherRel
    {
        [Required(ErrorMessage = "Please fill this section")]
        public int teacher_id { get; set; }

        [Required(ErrorMessage = "Please fill this section")]
        public int subject_id { get; set; }

        [Required(ErrorMessage = "Please fill this section")]
        public int grade_id { get; set; }

        public string teacherName { get; set; }

        public string subject { get; set; }

        public string grade { get; set; }
        

    }
}