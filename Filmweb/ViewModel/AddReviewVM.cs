using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Filmweb.ViewModel
{
    public class AddReviewVM : INotifyPropertyChanged
    {
        private readonly MainVM _mainVM;

        private string _movieTitle;

        public string MovieTitle
        {
            get => _movieTitle;
            set
            {
                if (_movieTitle != value)
                {
                    _movieTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _rating;
        public double Rating
        {
            get => _rating;
            set
            {
                if (_rating != value)
                {
                    _rating = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _content;
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged();
                }
            }
        }

        public AddReviewVM(MainVM mainVM, string movieTitle)
        {
            _mainVM = mainVM;
            MovieTitle = movieTitle;
        }

        public bool SaveReview()
        {
            SqlConnection connection = DatabaseConnection.GetConnection();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string reviewquery = @"EXEC dbo.Dodaj_Opinie
                        @login = @Login,
                        @nazwa_filmu = @Title,
                        @tresc = @Content,
                        @ocena = @Rating;";

                    using (SqlCommand command = new SqlCommand(reviewquery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@Content", Content);
                        command.Parameters.AddWithValue("@Rating", Rating);
                        command.Parameters.AddWithValue("@Title", MovieTitle);
                        command.Parameters.AddWithValue("@Login", _mainVM.CurrentUser.Username);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
