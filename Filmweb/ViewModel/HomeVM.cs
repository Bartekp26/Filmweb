using Filmweb.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;

namespace Filmweb.ViewModel
{
    public class HomeVM : INotifyPropertyChanged
    {
        private readonly MainVM _mainVM;
        private const int PageSize = 5;
        private int _currentPage = 0;
        private List<MovieListItemM> _filteredMovies = new List<MovieListItemM>();

        public ObservableCollection<MovieListItemM> AllMovies { get; set; } = new ObservableCollection<MovieListItemM>();
        public ObservableCollection<MovieListItemM> PagedMovies { get; set; } = new ObservableCollection<MovieListItemM>();
        public ObservableCollection<string> AvailableGenres { get; set; } = new ObservableCollection<string>();



        private string _selectedGenre;
        public string SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                _selectedGenre = value;
                FilterPagedMovies();
                OnPropertyChanged(nameof(SelectedGenre));
            }
        }
        public HomeVM(MainVM mainVM)
        {
            _mainVM = mainVM;
            LoadAllMovies();
            UpdatePagedMovies();
            LoadGenresFromDatabase();
            _filteredMovies = AllMovies.ToList();
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
                    F.Ocena AS Rating,
                    F.url AS ImageUrl,
                    (
                        SELECT DISTINCT G.Gatunek + ', '
                        FROM Conn_Filmy_Gat C2
                        JOIN Gatunek G ON G.ID_Gatunku = C2.ID_Gatunku
                        WHERE C2.ID_Filmu = F.ID_Filmu
                        FOR XML PATH(''), TYPE
                    ).value('.', 'NVARCHAR(MAX)') AS Genres
                FROM Filmy F
                ORDER BY F.ID_Filmu;";

            using (var cmd = new SqlCommand(sql, connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    movies.Add(new MovieListItemM
                    {
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"].ToString(),
                        Rating = Convert.ToDouble(reader["Rating"]),
                        ImageUrl = reader["ImageUrl"].ToString(),
                        GenreList = reader["Genres"]?.ToString()
                                           ?.Split(',')
                                           .Select(g => g.Trim())
                                           .Where(g => !string.IsNullOrWhiteSpace(g))
                                           .ToList()
                    });
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
            _currentPage = 0;

            if (!string.IsNullOrEmpty(SelectedGenre) && SelectedGenre != "Wszystkie gatunki")
            {
                _filteredMovies = AllMovies
                    .Where(m => m.GenreList != null && m.GenreList.Contains(SelectedGenre))
                    .ToList();
            }
            else
            {
                _filteredMovies = AllMovies.ToList();
            }

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


        public bool CanGoToNext => (_currentPage + 1) * PageSize < _filteredMovies.Count;
        public bool CanGoToPrevious => _currentPage > 0;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
