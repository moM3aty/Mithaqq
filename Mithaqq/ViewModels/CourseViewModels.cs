using Mithaqq.Models;
using System.Collections.Generic;

namespace Mithaqq.ViewModels
{

    public class CourseContentViewModel
    {
        public string CourseName { get; set; }
        public List<LessonViewModel> Lessons { get; set; }
        public string UserIdentifier { get; set; } 
    }

    public class LessonViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SecureVideoUrl { get; set; }
    }
}
