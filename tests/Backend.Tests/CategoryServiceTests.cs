using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using Backend.Application.Services;
using Backend.Domain.Interfaces.Repositories;
using Backend.Application.DTOs;

namespace Backend.Tests
{
  public class CategoryServiceTests
  {
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
      _sut = new CategoryService(_categoryRepo.Object);
    }

    [Fact]
    public async Task AddProductToCategory_InvokesRepository()
    {
      var categoryId = Guid.NewGuid();
      var productId = Guid.NewGuid();

      _categoryRepo.Setup(r => r.AddProductToCategory(categoryId, productId)).ReturnsAsync(true);

      var result = await _sut.AddProductToCategory(categoryId, productId);

      result.Should().BeTrue();
      _categoryRepo.Verify(r => r.AddProductToCategory(categoryId, productId), Times.Once);
    }
  }
}