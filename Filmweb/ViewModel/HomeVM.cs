using Filmweb.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmweb.ViewModel
{
    
    public class HomeVM : INotifyPropertyChanged
    {
        private readonly MainVM _mainVM;
        private const int PageSize = 5;
        private int _currentPage = 0;

        public ObservableCollection<MovieListItemM> AllMovies { get; set; } = new ObservableCollection<MovieListItemM>();
        public ObservableCollection<MovieListItemM> PagedMovies { get; set; } = new ObservableCollection<MovieListItemM>();

        public HomeVM(MainVM mainVM)
        {
            _mainVM = mainVM;

            LoadMoviesFromDatabase();
            UpdatePagedMovies();
        }

        private void LoadMoviesFromDatabase()
        {
            // testowo
            AllMovies.Add(new MovieListItemM
            {
                Title = "Incepcja",
                Description = "Sen we śnie",
                Rating = 8.8,
                UserRating = null, // lub np. 7.5
                ImageUrl = "https://via.placeholder.com/180x150"
            });
            AllMovies.Add(new MovieListItemM
            {
                Title = "Incepcja",
                Description = "Sen we śnie",
                Rating = 8.8,
                UserRating = 4, // lub np. 7.5
                ImageUrl = "https://via.placeholder.com/180x150"
            });
            AllMovies.Add(new MovieListItemM
            {
                Title = "Incepcja",
                Description = "Sen we śnie",
                Rating = 8.8,
                UserRating = 4.7, // lub np. 7.5
                ImageUrl = "https://via.placeholder.com/180x150"
            });
            AllMovies.Add(new MovieListItemM
            {
                Title = "Incepcja",
                Description = "Sen we śnie",
                Rating = 8.8,
                UserRating = 4.7, // lub np. 7.5
                ImageUrl = "https://via.placeholder.com/180x150"
            });
            AllMovies.Add(new MovieListItemM
            {
                Title = "Incepcja",
                Description = "Sen we śnie",
                Rating = 8.8,
                UserRating = 4.7, // lub np. 7.5
                ImageUrl = "https://via.placeholder.com/180x150"
            });
            AllMovies.Add(new MovieListItemM
            {
                Title = "Incepcja",
                Description = "Sen we śnie",
                Rating = 8.8,
                UserRating = 4.7, // lub np. 7.5
                ImageUrl = "https://via.placeholder.com/180x150"
            });
            UpdatePagedMovies();
        }

        private void UpdatePagedMovies()
        {
            PagedMovies.Clear();

            var moviesToShow = AllMovies
                .Skip(_currentPage * PageSize)
                .Take(PageSize);

            foreach (var movie in moviesToShow)
                PagedMovies.Add(movie);

            OnPropertyChanged(nameof(CanGoToPrevious));
            OnPropertyChanged(nameof(CanGoToNext));
        }

        public void GoToNextPage()
        {
            if (CanGoToNext)
            {
                _currentPage++;
                UpdatePagedMovies();
            }
        }

        public void GoToPreviousPage()
        {
            if (CanGoToPrevious)
            {
                _currentPage--;
                UpdatePagedMovies();
            }
        }

        public bool CanGoToNext => (_currentPage + 1) * PageSize < AllMovies.Count;
        public bool CanGoToPrevious => _currentPage > 0;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
