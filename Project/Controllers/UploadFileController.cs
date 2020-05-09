using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;
using System.IO;
using System.Net;
using System.Data.Entity;
using Microsoft.Reporting.WebForms;
using System.Data.Entity.Validation;

namespace Project.Controllers
{
    public class UploadFileController : Controller
    {
        
        // GET: UploadFile
        public ActionResult Index()
        {
            try
            {
                ViewBag.FileMessage = "Select file you want to upload";
                ViewBag.teachersList = new SelectList(GetteachersList(), "teacher_id", "teacher_name");
                return View();
            }
            catch(Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "Index"));
            }
           
        }

        [HandleError]
        public List<teacher> GetteachersList()
        {
            List<teacher> teachers = null;
           
            DBmodel db = new DBmodel();
            teachers = db.teachers.ToList();

            return teachers;
        }

        public ActionResult GetSubjectList(int teacher_id)
        {
            try {
                DBmodel db = new DBmodel();
                List<teacher_subject> subID = db.teacher_subject.Where(x => x.teacher_id == teacher_id).ToList();

                foreach (var item in subID)
                {
                    int subjectId = item.subject_id;

                    List<subject> subjects = db.subjects.Where(x => x.subject_id == subjectId).ToList();
                    ViewBag.subList = new SelectList(subjects, "subject_id", "subject1");
                }


                return PartialView("DisplaySubjects");
            }
            catch(Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "GetSubjectList"));

            }
        }

        public ActionResult GetGradeList(int teacher_id)
        {
            try {
                DBmodel db = new DBmodel();
                List<teacher_grade> grades = db.teacher_grade.Where(x => x.teacher_id == teacher_id).ToList();

                ViewBag.gradeList = new SelectList(grades, "grade_id", "grade");

                return PartialView("DisplayGrades");
            }catch(Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "GetGradeList"));
            }
        }

        [HttpPost]
        public ActionResult Index(IEnumerable<HttpPostedFileBase> files,TeacherRel model, String message) 
        {
            try {
                    DBmodel db = new DBmodel();
                    upload_file log = new upload_file();
                    upload_file_teacher log2 = new upload_file_teacher();

                    int count = 0;
                    if (!ModelState.IsValid)
                    {
                        return new JsonResult { Data = "File not uploaded" };
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

                                    
                                    //db.SaveChanges();

                                    int teacherid = model.teacher_id;

                                    log2.teacher_id = teacherid;

                                    db.upload_file.Add(log);
                                    db.upload_file_teacher.Add(log2);

                                    file.SaveAs(path);
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
            catch(Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "Index"));
            }
        }

        public ActionResult ViewList()
        {
            try {
                DBmodel db = new DBmodel();

                List<upload_file> files = db.upload_file.ToList();

                return View(files);
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "ViewList"));
            }

        }
        
        [HandleError]
        public ActionResult DownloadFile(string fileName)
        {
            try {
                DBmodel db = new DBmodel();

                var directory = new DirectoryInfo(Server.MapPath("~/UploadedFiles"));
                
                string path = directory.FullName;
                
                //"D:\MVC\Project\Project\UploadedFiles\"
                byte[] fileBytes = System.IO.File.ReadAllBytes(@path +"\\"+ fileName);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch(Exception ex)
            {

                return View("Error", new HandleErrorInfo(ex, "UploadFile", "DownloadFile"));

            }


        }

        public ActionResult Delete(int? id)
        {
            try {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DBmodel db = new DBmodel();
                upload_file file = db.upload_file.Find(id);

                if (file == null)
                {
                    return HttpNotFound();
                }
                return View(file);
            }
            catch(Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "Delete"));
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSucces(int id)
        {
            try
            {
                DBmodel db = new DBmodel();
                upload_file file = db.upload_file.Find(id);


                upload_file_teacher teacher = db.upload_file_teacher.Find(id);
               
                //db.SaveChanges();

                var directory = new DirectoryInfo(Server.MapPath("~/UploadedFiles"));
                //FileInfo[] getFile = directory.GetFiles(file.file_name+".*");
                string getFile = directory.FullName + "\\" + file.file_name;

                System.IO.File.Delete(getFile);
                db.upload_file_teacher.Remove(teacher);
                db.upload_file.Remove(file);

                db.SaveChanges();

                return RedirectToAction("ViewList");
            }
            catch (FileNotFoundException ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "DeleteSucces"));
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "DeleteSucces"));
            }
        }

        public ActionResult Edit(int? id)
        {
            try
            {
                DBmodel db = new DBmodel();
                upload_file file = db.upload_file.Find(id);

                ViewBag.teachersList = new SelectList(GetteachersList(), "teacher_id", "teacher_name");

                return View(file);
            }
            catch(Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "Edit"));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public ActionResult EditSucces(int id,upload_file model)
        {
            try {
                if (ModelState.IsValid)
                {
                    DBmodel db = new DBmodel();

                    upload_file file = db.upload_file.Find(id);

                    string oldName = file.file_name;

                    file.file_name = model.file_name.Trim();
                   
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
                    file.subject = subject.subject;

                    var directory = new DirectoryInfo(Server.MapPath("~/UploadedFiles"));
                    string path = directory.FullName + "\\" + model.file_name;

                    // path = "D:\\MVC\\Project\\Project\\UploadedFiles\\" + fileName;

                    file.file_path = path.Trim();
                    

                    //db.SaveChanges();

                    int fileID = model.file_id;

                    upload_file_teacher teacher = db.upload_file_teacher.Find(fileID);

                    teacher.teacher_id = model.teacher_id;

                    

                   
                   
                    db.Entry(teacher).State = EntityState.Modified;
                    db.Entry(file).State = EntityState.Modified;
                    db.SaveChanges();
                    ChangeFileName(fileID, model.file_name, oldName);

                    return RedirectToAction("ViewList");
                }
                else
                {
                    ViewBag.teachersList = new SelectList(GetteachersList(), "teacher_id", "teacher_name");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "EditSucces"));
            }
        }

        [HandleError]
        public void ChangeFileName(int fileID,string fileName, string oldName)
        {
            try {
                

                var directory = new DirectoryInfo(Server.MapPath("~/UploadedFiles"));
                //FileInfo[] getFile = directory.GetFiles(oldName+".*");

                string getFile = directory.FullName + "\\" + oldName;

                //List<String> files = new List<string>();
                //var path = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);

                System.IO.File.Move(getFile, directory.FullName + "\\" + fileName);

                
            }
            catch (Exception ex)
            {
                
            }
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
            try {
                DBmodel db = new DBmodel();

                List<subject> subjects = db.subjects.ToList();

                ViewBag.subjectList = new SelectList(subjects, "subject_id", "subject1");

                return View();
            }
            catch(Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "GetFileList"));
            }
        }

       
        public ActionResult GetFiles(TeacherRel model)
        {
            try {
                DBmodel db = new DBmodel();

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
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "GetFiles"));
            }
        }

        public ActionResult GetAllGrades()
        {
            try {
                DBmodel db = new DBmodel();
                List<grade> grades = db.grades.ToList();

                ViewBag.allgradeList = new SelectList(grades, "grade_id", "grade1");

                return PartialView("DisplayAllGrades");
            }
            catch(Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "GetAllGrades"));
            }
        }

        public ActionResult Report(string ReportType)
        {
            try {
                DBmodel db = new DBmodel();
                LocalReport localReport = new LocalReport();
                localReport.ReportPath = Server.MapPath("~/Reports/uploadFileReport.rdlc");

                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Name = "UploadFiles";
                reportDataSource.Value = db.upload_file.ToList();

                localReport.DataSources.Add(reportDataSource);

                string Rtype = ReportType;
                string fileNameExtention = "pdf";
                byte[] renderByte;

                renderByte = localReport.Render(Rtype);

                Response.AddHeader("content-disposition", "attachment:filename = upload_file_report_" + DateTime.Now + "." + fileNameExtention);
                return File(renderByte, fileNameExtention);
            }
            catch(FileNotFoundException ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "Report"));
            }
            catch(Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "UploadFile", "Report"));
            }
        }

    }
}