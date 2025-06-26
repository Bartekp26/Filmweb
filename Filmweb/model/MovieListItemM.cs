using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmweb.Model
{
    public class MovieListItemM
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public double? UserRating { get; set; } = null;


        public string ImageUrl { get; set; }
        public List<string> GenreList { get; set; } = new List<string>();
        public bool HasUserRating => UserRating != null;

        public string GenresAsText => string.Join(", ", GenreList);

    }
}
