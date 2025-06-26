using Filmweb.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Media;
using Filmweb.ViewModel.BaseClass;

namespace Filmweb.ViewModel
{
    public class HomeVM : ViewModelBase
    {
        private readonly MainVM _mainVM;
        private const int PageSize = 5;
        private int _currentPage = 0;
        private List<MovieListItemM> _filteredMovies = new List<MovieListItemM>();

        public ObservableCollection<MovieListItemM> AllMovies { get; set; } = new ObservableCollection<MovieListItemM>();
        public ObservableCollection<MovieListItemM> PagedMovies { get; set; } = new ObservableCollection<MovieListItemM>();
        public ObservableCollection<string> AvailableGenres { get; set; } = new ObservableCollection<string>();

        public string Placeholder => "Wyszukaj...";

        public Brush SearchBoxForeground =>
            string.IsNullOrWhiteSpace(SearchText) || SearchText == Placeholder
            ? Brushes.Gray
            : Brushes.Black;

        private string _selectedGenre;
        public string SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                _selectedGenre = value;
                _currentPage = 0;
                FilterPagedMovies();
                OnPropertyChanged(nameof(SelectedGenre));
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                _currentPage = 0;
                FilterPagedMovies();
                OnPropertyChanged(nameof(SearchText));
                OnPropertyChanged(nameof(SearchBoxForeground));
            }
        }
        public void InitializeMovies()
        {
            LoadAllMovies();
            _filteredMovies = AllMovies.ToList();
            LoadGenresFromDatabase();
            FilterPagedMovies();
            SearchText = Placeholder;
        }

        public HomeVM(MainVM mainVM)
        {
            _mainVM = mainVM;
        }

        private void LoadAllMovies()
        {
            var movies = new List<MovieListItemM>();
            var connection = DatabaseConnection.GetConnection();

            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();

            string sql = @"
                        SELECT 
                            F.ID_Filmu,
                            F.Nazwa AS Title,
                            F.Opis AS Description,
                            (
                                SELECT AVG(CAST(Ocena AS FLOAT))
                                FROM opinie O
                                WHERE O.ID_Filmu = F.ID_Filmu
                            ) AS Rating,
                            F.url AS ImageUrl,
                            (
                                SELECT DISTINCT G.Gatunek + ', '
                                FROM Conn_Filmy_Gat C2
                                JOIN Gatunek G ON G.ID_Gatunku = C2.ID_Gatunku
                                WHERE C2.ID_Filmu = F.ID_Filmu
                                FOR XML PATH(''), TYPE
                            ).value('.', 'NVARCHAR(MAX)') AS Genres,
                            (
                                SELECT TOP 1 O.Ocena
                                FROM opinie O
                                WHERE O.ID_Filmu = F.ID_Filmu
                                  AND O.ID_Uzytkownika = (
                                      SELECT ID_Uzytkownika
                                      FROM UZ_Login
                                      WHERE Login = @Login
                                  )
                                ORDER BY O.Data_dodania DESC
                            ) AS UserRating
                        FROM Filmy F
                        ORDER BY F.ID_Filmu;";

            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Login", _mainVM.CurrentUser.Username);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        movies.Add(new MovieListItemM
                        {
                            Title = reader["Title"].ToString(),
                            Description = reader["Description"].ToString(),
                            Rating = reader["Rating"] != DBNull.Value ? Convert.ToDouble(reader["Rating"]) : 0,
                            ImageUrl = reader["ImageUrl"].ToString(),
                            UserRating = reader["UserRating"] != DBNull.Value ? (double?)Convert.ToDouble(reader["UserRating"]) : null,
                            GenreList = reader["Genres"]?.ToString()
                                ?.Split(',')
                                .Select(g => g.Trim())
                                .Where(g => !string.IsNullOrWhiteSpace(g))
                                .ToList()
                        });
                    }
                }
            }

            foreach (var movie in movies)
                AllMovies.Add(movie);
        }


        private void LoadGenresFromDatabase()
        {
            var connection = DatabaseConnection.GetConnection();

            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();

            string sql = "SELECT DISTINCT Gatunek FROM Gatunek WHERE Gatunek IS NOT NULL ORDER BY Gatunek;";

            using (var cmd = new SqlCommand(sql, connection))
            using (var reader = cmd.ExecuteReader())
            {
                AvailableGenres.Clear();
                AvailableGenres.Add("Wszystkie gatunki");

                while (reader.Read())
                {
                    var genre = reader["Gatunek"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(genre))
                        AvailableGenres.Add(genre);
                }
            }

            SelectedGenre = "Wszystkie gatunki";
        }

        private void FilterPagedMovies()
        {
            IEnumerable<MovieListItemM> filtered = AllMovies;

            if (!string.IsNullOrWhiteSpace(SelectedGenre) &&
                SelectedGenre != "Wszystkie gatunki")
            {
                filtered = filtered.Where(m => m.GenreList != null && m.GenreList.Contains(SelectedGenre));
            }

            if (!string.IsNullOrWhiteSpace(SearchText) &&
                SearchText != "Wyszukaj...")
            {
                filtered = filtered.Where(m => m.Title != null &&
                    m.Title.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _filteredMovies = filtered.ToList();
            UpdatePagedMovies();
        }

        private void UpdatePagedMovies()
        {
            PagedMovies.Clear();

            var moviesToShow = _filteredMovies
                .Skip(_currentPage * PageSize)
                .Take(PageSize);

            foreach (var movie in moviesToShow)
                PagedMovies.Add(movie);

            OnPropertyChanged(nameof(CanGoToPrevious));
            OnPropertyChanged(nameof(CanGoToNext));
        }
        public void RefreshUserRating(string title, double rating, double newRating)
        {
            var movie = AllMovies.FirstOrDefault(m => m.Title.Trim().Equals(title.Trim(), StringComparison.OrdinalIgnoreCase));
            if (movie != null)
            {
                movie.UserRating = rating;
                movie.Rating = newRating;

                var paged = PagedMovies.FirstOrDefault(m => m.Title.Trim().Equals(title.Trim(), StringComparison.OrdinalIgnoreCase));
                if (paged != null)
                {
                    var index = PagedMovies.IndexOf(paged);
                    PagedMovies.RemoveAt(index);
                    PagedMovies.Insert(index, movie);
                }
            }
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

        public void OpenMovieDetails(string title)
        {
            _mainVM.NavigateToMovieDetails(title);
        }

        public bool CanGoToNext => (_currentPage + 1) * PageSize < _filteredMovies.Count;
        public bool CanGoToPrevious => _currentPage > 0;
    }
}
