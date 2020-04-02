using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;
using System.IO;

namespace Project.Controllers
{
    public class UploadFileController : Controller
    {
        
        // GET: UploadFile
        public ActionResult Index()
        {
            ViewBag.teachersList = new SelectList(GetteachersList(), "teacher_id", "teacher_name");
            return View();
        }

        public List<teacher> GetteachersList()
        {
            dbModels db = new dbModels();
            List<teacher> teachers = db.teachers.ToList();
            return teachers;
        }

        public ActionResult GetSubjectList(int teacher_id)
        {
            dbModels db = new dbModels();
            List<teacher_subject> subID = db.teacher_subject.Where(x => x.teacher_id == teacher_id).ToList();
            
            foreach(var item in subID)
            {
                int subjectId = item.subject_id;
                
                List<subject> subjects = db.subjects.Where(x => x.subject_id == subjectId).ToList();
                ViewBag.subList = new SelectList(subjects, "subject_id", "subject1");
            }
           
            
            return PartialView("DisplaySubjects");
        }

        public ActionResult GetGradeList(int teacher_id)
        {
            dbModels db = new dbModels();
            List<teacher_grade> grades = db.teacher_grade.Where(x => x.teacher_id == teacher_id).ToList();
    
            ViewBag.gradeList = new SelectList(grades, "grade_id", "grade");

            return PartialView("DisplayGrades");
        }

        [HttpPost]
        public ActionResult Index(IEnumerable<HttpPostedFileBase> files)
        {
            dbModels db = new dbModels();
            upload_file log = new upload_file();

            

            int count = 0;
            if(files != null)
            {
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = file.FileName;
                        var path = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);
                        file.SaveAs(path);

                        log.file_name = fileName;
                        log.file_path = path;
                        log.upload_date = DateTime.Now;
                        log.grade = "";
                        log.subject = "";

                        db.upload_file.Add(log);
                        db.SaveChanges();

                       count++;
                    }
                }
            }

            ViewBag.FileMessage = "Successfully "  + count+ " file(s) uploaded";

            return View();
        }

        public FileResult DownloadFile(string fileName)
        {
            var filePath = "~/UploadedFiles/" + fileName;
            return File(filePath, "application/force- download", Path.GetFileName(filePath));
        }

        public List<string> GetFileList()
        {
            var directory = new DirectoryInfo(Server.MapPath("~/UploadedFiles"));
            FileInfo[] fileNames = directory.GetFiles("*.*");

            List<String> files = new List<string>();

            foreach (var item in fileNames)
            {
                files.Add(item.Name);
            }
            return files;
        }



    }
}