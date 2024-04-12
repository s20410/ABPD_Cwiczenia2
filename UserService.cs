using System;

namespace LegacyApp
{
    public class UserService
    {
        public UserService()
        {
        }

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || !email.Contains("@") || !email.Contains("."))
                return false;

            var age = CalculateAge(dateOfBirth);
            if (age < 21)
                return false;

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);
            if (client == null)
                return false;

            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

            if (!CheckCreditLimit(client, user))
                return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            var age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
                age--;
            return age;
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        private bool CheckCreditLimit(Client client, User user)
        {
            if (client.Type == "VeryImportantClient")
                user.HasCreditLimit = false;
            else
            {
                var userCreditService = new UserCreditService();
                var creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                user.CreditLimit = client.Type == "ImportantClient" ? creditLimit * 2 : creditLimit;
                user.HasCreditLimit = true;
            }
            return !(user.HasCreditLimit && user.CreditLimit < 500);
        }
    }
}
