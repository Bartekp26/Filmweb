using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmweb.Model
{
    class MovieListItemM
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public double? UserRating { get; set; } = null;
        public string ImageUrl { get; set; }
    }
}
