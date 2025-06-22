using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmweb.Model
{
    public class ReviewM
    {
        public string Content { get; set; }    
        public int Rating { get; set; }       
        public DateTime DateAdded { get; set; }
        public UserM Author { get; set; }

    }
}
