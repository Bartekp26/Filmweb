using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Filmweb.Model
{
    public class MovieM
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double Rating { get; set; } 
        public List<string> Genres { get; set; }
        public bool IsPartOfSeries { get; set; } = false;
        public List<MovieListItemM> Series { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Actors { get; set; }
        public List<string> Directors { get; set; }
        public List<ReviewM> Reviews { get; set; }
    

    public MovieM()
        {
            Genres = new List<string>();
            Reviews = new List<ReviewM>();
            Series = new List<MovieListItemM>();
            Directors = new List<string>();
            Actors = new List<string>();
        }

    } 
}
