using System.ComponentModel.DataAnnotations;

namespace Mithaqq.Models
{
    /// <summary>
    /// Represents a single lesson within a course.
    /// Now includes fields for secure video hosting with Bunny.net.
    /// </summary>
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// The ID of your video library on Bunny.net.
        /// </summary>
        [Display(Name = "Bunny.net Library ID")]
        public long? BunnyLibraryId { get; set; }

        /// <summary>
        /// The unique Video ID for this lesson on Bunny.net.
        /// </summary>
        [Display(Name = "Bunny.net Video ID")]
        public string? BunnyVideoId { get; set; }

        /// <summary>
        /// Fallback URL. The primary method will be generating a secure URL from Bunny IDs.
        /// </summary>
        [Display(Name = "Fallback Video URL")]
        public string? VideoUrl { get; set; }

        /// <summary>
        /// The display order of the lesson within the course.
        /// </summary>
        public int Order { get; set; }

        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
    }
}
