using System;
using System.ComponentModel.DataAnnotations;

namespace Task4.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Не указано имя.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указан Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Не указан пароль.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password",ErrorMessage = "Пароли не совпадают.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
