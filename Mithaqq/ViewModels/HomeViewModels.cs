using Mithaqq.Models;
using System.Collections.Generic;

namespace Mithaqq.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<BlogPost> LatestPosts { get; set; }
    }

    public class CompanyStoreViewModel
    {
        public Company Company { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<TravelPackage> TravelPackages { get; set; }
        public List<TrainingFieldViewModel> TrainingFields { get; set; }
    }

    public class TrainingFieldViewModel
    {
        public string CategoryName { get; set; }
        public int CourseCount { get; set; }
        public string IconClass { get; set; }
    }
}
