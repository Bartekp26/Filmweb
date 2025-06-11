using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmweb.Model
{
    class ReviewM
    {
        public string Content { get; set; }    
        public double Rating { get; set; }       
        public DateTime DateAdded { get; set; }
        public UserM Author { get; set; }

    }
}
