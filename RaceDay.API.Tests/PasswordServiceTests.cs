using RaceDay.API.Services;

namespace RaceDay.API.Tests
{
    public class PasswordServiceTests
    {
        [Fact]
        public void PasswordCanBeHashedAndVerified()
        {
            var service = new PasswordService();

            string password = "Test123!";

            string hash = service.HashPassword(password);

            bool result = service.VerifyPassword(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void WrongPasswordIsRejected()
        {
            var service = new PasswordService();

            string hash = service.HashPassword("Test123!");

            bool result = service.VerifyPassword(
                "WrongPassword!",
                hash);

            Assert.False(result);
        }
    }
}