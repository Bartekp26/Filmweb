using Filmweb.Model;
using Filmweb.ViewModel.BaseClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Filmweb.ViewModel
{
    public class RegisterVM 
    {
        private readonly MainVM _mainVM;

        public ICommand ForwardToLoginCommand => _mainVM.SwitchToLoginCommand;
        public ICommand RegisterInCommand => _mainVM.RegisterInCommand;

       

        public RegisterVM(MainVM mainVM)
        {
            _mainVM = mainVM;
        }
        //cos takiego ale trzeba pobierac dane z formularza, inne zmiennie i w registerView.xaml dowiazania zrobic
        //public string ValidateRegister()
        //{
        //    var errors = new List<string>();

        //    if (string.IsNullOrWhiteSpace(FirstName))
        //        errors.Add("Imię jest wymagane.");
        //    else if (FirstName.Length > 30)
        //        errors.Add("Imię nie może być dłuższe niż 30 znaków.");

        //    if (string.IsNullOrWhiteSpace(LastName))
        //        errors.Add("Nazwisko jest wymagane.");
        //    else if (LastName.Length > 30)
        //        errors.Add("Nazwisko nie może być dłuższe niż 30 znaków.");

        //    if (string.IsNullOrWhiteSpace(Username))
        //        errors.Add("Login jest wymagany.");
        //    else if (Username.Length > 30)
        //        errors.Add("Login nie może być dłuższy niż 30 znaków.");

        //    if (string.IsNullOrWhiteSpace(Password))
        //        errors.Add("Hasło jest wymagane.");
        //    else
        //    {
        //        if (Password.Length < 6)
        //            errors.Add("Hasło musi mieć co najmniej 6 znaków.");
        //        if (!Regex.IsMatch(Password, @"[A-Z]"))
        //            errors.Add("Hasło musi zawierać co najmniej jedną dużą literę.");
        //        if (!Regex.IsMatch(Password, @"\d"))
        //            errors.Add("Hasło musi zawierać co najmniej jedną cyfrę.");
        //        if (!Regex.IsMatch(Password, @"[!@#$%^&*(),.?""{}|<>]"))
        //            errors.Add("Hasło musi zawierać co najmniej jeden znak specjalny.");
        //    }

        //    if (string.IsNullOrWhiteSpace(Email))
        //        errors.Add("E-mail jest wymagany.");
        //    else
        //    {
        //        if (Email.Length > 30)
        //            errors.Add("E-mail nie może być dłuższy niż 30 znaków.");
        //        if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        //            errors.Add("Nieprawidłowy adres e-mail.");
        //    }

        //TODO: Sprawdź, czy e-mail lub login istnieje już w bazie danych
        //     if (EmailLubLoginIstniejeWBazie(Email, Username))
        //        errors.Add("Podany e-mail lub login jest już zajęty.");

        //    return string.Join("\n", errors);
        //}
    }
}
