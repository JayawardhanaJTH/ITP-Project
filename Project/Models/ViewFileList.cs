using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public class ViewFileList
    {
        public int file_id { get; set; }
        public string file_name { get; set; }
        public System.DateTime upload_date { get; set; }
        public string subject { get; set; }
        public string grade { get; set; }
    }
}