using Mithaqq.Models;
using System.Collections.Generic;

namespace Mithaqq.ViewModels
{
    // For the main store page (Views/Store/Index.cshtml)
    public class StoreViewModel
    {
        public IEnumerable<Product> AllProducts { get; set; }
        public IEnumerable<Course> AllCourses { get; set; }
        public IEnumerable<TravelPackage> AllTravelPackages { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }

    // For the product details page (Views/Store/ProductDetails.cshtml)
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public List<Review> Reviews { get; set; }
        public bool UserHasPurchased { get; set; }
        public bool UserHasReviewed { get; set; }
    }

    // For the course details page (Views/Store/CourseDetails.cshtml)
    public class CourseDetailViewModel
    {
        public Course Course { get; set; }
        public List<Review> Reviews { get; set; }
        public bool UserHasPurchased { get; set; }
        public bool UserHasReviewed { get; set; }
    }
}