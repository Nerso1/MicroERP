using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace MicroERP.UnitTests.Helpers;

internal static class MockDbSetHelper
{
    internal static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        => data.BuildMockDbSet();
}
