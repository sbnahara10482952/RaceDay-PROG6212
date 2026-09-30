using RaceDay.API.Data;
using RaceDay.API.Models;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.API.Tests
{
    public class EventRoleTests
    {
        [Fact]
        public async Task EventCanBeStoredForOrganiser()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);

            var organiser = new User
            {
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser@test.com",
                PasswordHash = "hash",
                Role = "Organiser"
            };

            context.Users.Add(organiser);
            await context.SaveChangesAsync();

            var eventItem = new Event
            {
                OrganiserID = organiser.UserID,
                EventName = "Test Race",
                Description = "Test event",
                EventDate = DateTime.Today.AddDays(10),
                StartTime = new TimeSpan(7, 0, 0),
                Location = "Durban",
                DistanceKm = 10,
                EventType = "Run",
                Status = "Upcoming"
            };

            context.Events.Add(eventItem);
            await context.SaveChangesAsync();

            Assert.Equal(1, await context.Events.CountAsync());
            Assert.Equal("Run", eventItem.EventType);
        }
    }
}