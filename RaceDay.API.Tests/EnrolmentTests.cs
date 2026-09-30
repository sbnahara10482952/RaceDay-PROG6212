using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Tests
{
    public class EnrolmentTests
    {
        [Fact]
        public async Task EnrolmentCanBeCreated()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayDbContext(options);

            var participant = new User
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@test.com",
                PasswordHash = "hash",
                Role = "Participant"
            };

            context.Users.Add(participant);

            var category = new Category
            {
                CategoryName = "Test 10km",
                DistanceKm = 10
            };

            context.Categories.Add(category);

            var eventItem = new Event
            {
                EventName = "Test Event",
                EventDate = DateTime.Today.AddDays(10),
                StartTime = new TimeSpan(7, 0, 0),
                Location = "Durban",
                DistanceKm = 10,
                EventType = "Run",
                Status = "Upcoming",
                OrganiserID = 1
            };

            context.Events.Add(eventItem);

            await context.SaveChangesAsync();

            var eventCategory = new EventCategory
            {
                EventID = eventItem.EventID,
                CategoryID = category.CategoryID,
                EntryFee = 100,
                MaxParticipants = 100,
                AvailableSlots = 100
            };

            context.EventCategories.Add(eventCategory);

            await context.SaveChangesAsync();

            var enrolment = new Enrolment
            {
                ParticipantID = participant.UserID,
                EventCategoryID = eventCategory.EventCategoryID,
                EnrolmentStatus = "Confirmed",
                RaceNumber = 500
            };

            context.Enrolments.Add(enrolment);

            await context.SaveChangesAsync();

            Assert.Equal(1, await context.Enrolments.CountAsync());
            Assert.Equal("Confirmed", enrolment.EnrolmentStatus);
        }
    }
}