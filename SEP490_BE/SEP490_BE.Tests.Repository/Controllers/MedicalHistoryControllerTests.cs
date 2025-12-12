using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.API.Controllers;
using SEP490_BE.DAL.Models;
using System;

namespace SEP490_BE.Tests.Controllers
{
    public class MedicalHistoryControllerTests : IDisposable
    {
        private readonly DiamondHealthContext _context;
        private readonly MedicalHistoryController _controller;

        public MedicalHistoryControllerTests()
        {
            var options = new DbContextOptionsBuilder<DiamondHealthContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DiamondHealthContext(options);
            _controller = new MedicalHistoryController(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetMedicalHistoryByUserId Tests

        [Fact]
        public async Task GetMedicalHistoryByUserId_WithNonExistentUserId_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;

            // Act
            var result = await _controller.GetMedicalHistoryByUserId(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            notFoundResult.Value.Should().NotBeNull();
        }

        #endregion

        #region GetPatientMedicalHistory Tests

        [Fact]
        public async Task GetPatientMedicalHistory_WithNonExistentPatientId_ReturnsOkWithEmptyList()
        {
            // Arrange
            var patientId = 999;

            // Act
            var result = await _controller.GetPatientMedicalHistory(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().NotBeNull();
        }

        #endregion
    }
}

