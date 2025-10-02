using System;

namespace Mithaqq.ViewModels
{
    public class ReviewAdminViewModel
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string UserName { get; set; }
        public int Stars { get; set; }
        public string Comment { get; set; }
        public DateTime DatePosted { get; set; }
    }
}
