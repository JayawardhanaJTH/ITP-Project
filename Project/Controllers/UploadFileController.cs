using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;
using System.IO;
using System.Net;
using System.Data.Entity;

namespace Project.Controllers
{
    public class UploadFileController : Controller
    {
        
        // GET: UploadFile
        public ActionResult Index()
        {
            ViewBag.FileMessage = "Select file you want to upload";
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
        public ActionResult Index(IEnumerable<HttpPostedFileBase> files,TeacherRel model, String message) 
        {
            dbModels db = new dbModels();
            upload_file log = new upload_file();
            upload_file_teacher log2 = new upload_file_teacher();
            

            int count = 0;
            if (!ModelState.IsValid)
            {
                return new JsonResult { Data = "File not uploaded" };
                //return View();
            }
            else
            {
                if (files != null)
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

                            int gradeid = model.grade_id;
                            int subjectid = model.subject_id;


                            var grades = db.grades.Where(u => u.grade_id == gradeid)
                                                             .Select(u => new
                                                             {
                                                                 grade = u.grade1
                                                             }).Single();

                            var subjects = db.subjects.Where(u => u.subject_id == subjectid)
                                                            .Select(u => new
                                                            {
                                                                subject = u.subject1
                                                            }).Single();

                            log.grade = grades.grade;
                            log.subject = subjects.subject;

                            db.upload_file.Add(log);
                            db.SaveChanges();

                            int teacherid = model.teacher_id;

                            log2.teacher_id = teacherid;

                            db.upload_file_teacher.Add(log2);
                            db.SaveChanges();

                            count++;
                        }
                    }
                     return new JsonResult { Data = "Successfully file Uploaded" };
                }
                else
                    return new JsonResult { Data = "File not uploaded" };

            }

        }

        public ActionResult ViewList()
        {
            dbModels db = new dbModels();

            List<upload_file> files = db.upload_file.ToList();

            return View(files);

        }

        public FileResult DownloadFile(string fileName)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(@"D:\MVC\Project\Project\UploadedFiles\"+fileName);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        
        }

        public ActionResult Delete(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            dbModels db = new dbModels();
            upload_file file = db.upload_file.Find(id);

            if(file == null)
            {
                return HttpNotFound();
            }
            return View(file);
            
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSucces(int id)
        {
            dbModels db = new dbModels();
            upload_file file = db.upload_file.Find(id);
           

            upload_file_teacher teacher = db.upload_file_teacher.Find(id);
            db.upload_file_teacher.Remove(teacher);
            db.SaveChanges();

            var directory =new DirectoryInfo(Server.MapPath("~/UploadedFiles"));
            FileInfo[] getFile = directory.GetFiles(file.file_name+".*");

            System.IO.File.Delete(getFile[0].FullName);

            db.upload_file.Remove(file);
            db.SaveChanges();

            return RedirectToAction("ViewList");
        }

        public ActionResult Edit(int? id)
        {
            dbModels db = new dbModels();
            upload_file file = db.upload_file.Find(id);

            ViewBag.teachersList = new SelectList(GetteachersList(), "teacher_id", "teacher_name");

            return View(file);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public ActionResult EditSucces(int id,upload_file model)
        {
            if (ModelState.IsValid)
            {
                dbModels db = new dbModels();

                upload_file file = db.upload_file.Find(id);

                string oldName = file.file_name;

                file.file_name = model.file_name;

                int gradeID = model.grade_id;
                int subjectID = model.subject_id;

                var grade = db.grades.Where(m => m.grade_id == gradeID)
                                .Select(u => new
                                {
                                    grade = u.grade1
                                }).Single();

                var subject = db.subjects.Where(m => m.subject_id == subjectID)
                                   .Select(u => new
                                   {
                                       subject = u.subject1
                                   }).Single(); 
               
                file.grade = grade.grade;
                file.subject = subject.subject ;

                db.Entry(file).State = EntityState.Modified;
                db.SaveChanges();

                int fileID = model.file_id;
            
                upload_file_teacher teacher = db.upload_file_teacher.Find(fileID);

                teacher.teacher_id = model.teacher_id;

                db.Entry(teacher).State = EntityState.Modified;
                db.SaveChanges();

                ChangeFileName(fileID, model.file_name,oldName);

                return RedirectToAction("ViewList");
            }
            else
            {
                ViewBag.teachersList = new SelectList(GetteachersList(), "teacher_id", "teacher_name");
                return View(model);
            }
        }

        public void ChangeFileName(int fileID,string fileName, string oldName)
        {
            dbModels db = new dbModels();

            upload_file file = db.upload_file.Find(fileID);

            var directory = new DirectoryInfo(Server.MapPath("~/UploadedFiles"));
            FileInfo[] getFile = directory.GetFiles(oldName+".*");

            //List<String> files = new List<string>();
            //var path = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);

            System.IO.File.Move(getFile[0].FullName,directory.FullName+"\\"+fileName);

            string path = file.file_path;
           
            path = "D:\\MVC\\Project\\Project\\UploadedFiles\\"+fileName;

            file.file_path = path;
            db.Entry(file).State = EntityState.Modified;
            db.SaveChanges();
        }

        public ActionResult homepage()
        {
            return View();
        }

        ////////////////////////////////////////////////////////////
        ////                                                    ////                    
        ////                   Student Side                     ////
        ////                                                    ////
        ////////////////////////////////////////////////////////////

          
        public ActionResult GetFileList()
        {
            dbModels db = new dbModels();

            List<subject> subjects = db.subjects.ToList();

            ViewBag.subjectList =new SelectList(subjects, "subject_id", "subject1");

            return View();
        }

       
        public ActionResult GetFiles(TeacherRel model)
        {
            dbModels db = new dbModels();

            var grades = db.grades.Where(u => u.grade_id == model.grade_id)
                                                            .Select(u => new
                                                            {
                                                                grade = u.grade1
                                                            }).Single();

            var subjects = db.subjects.Where(u => u.subject_id == model.subject_id)
                                            .Select(u => new
                                            {
                                                subject = u.subject1
                                            }).Single();

            List<upload_file> files = db.upload_file.Where(x => x.grade == grades.grade && x.subject == subjects.subject).ToList();
            return View(files);
        }

        public ActionResult GetAllGrades()
        {
            dbModels db = new dbModels();
            List<grade> grades = db.grades.ToList();

            ViewBag.allgradeList = new SelectList(grades, "grade_id", "grade1");

            return PartialView("DisplayAllGrades");
        }

    }
}