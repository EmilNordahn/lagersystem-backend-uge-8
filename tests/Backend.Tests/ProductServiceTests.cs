using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using Backend.Application.Services;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;
using Backend.Application.DTOs;
using Backend.Domain.ValueObjects; // for Price

namespace Backend.Tests
{
  public class ProductServiceTests
  {
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<ILocationService> _location_service = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
      _sut = new ProductService(_productRepo.Object, _location_service.Object);
    }

    [Fact]
    public async Task Create_CallsRepositoryAndReturnsCreatedDto()
    {
      var createDto = new ProductCreateDto("Test product", "Description", 9.99m, null);
      var returnedDto = new ProductDto(Guid.NewGuid(), "Test product", "Description", new Price(9.99m, "DKK"), null, new List<InventoryEntryDto>());

      _productRepo.Setup(r => r.Create(createDto)).ReturnsAsync(returnedDto);

      var result = await _sut.Create(createDto);

      result.Should().BeSameAs(returnedDto);
      _productRepo.Verify(r => r.Create(createDto), Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsRepositoryResults()
    {
      var list = new List<ProductDto>
      {
        new ProductDto(Guid.NewGuid(), "P1", "Desc1", new Price(1.23m, "DKK"), null, null),
        new ProductDto(Guid.NewGuid(), "P2", "Desc2", new Price(4.56m, "DKK"), null, null)
      };
      _productRepo.Setup(r => r.GetAll()).ReturnsAsync(list);

      var result = await _sut.GetAll();

      result.Should().BeEquivalentTo(list);
      _productRepo.Verify(r => r.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNull_AndDoesNotCallLocationService()
    {
      var id = Guid.NewGuid();
      _productRepo.Setup(r => r.GetById(id)).ReturnsAsync((ProductDto?)null);

      var result = await _sut.GetById(id);

      result.Should().BeNull();
      _location_service.Verify(ls => ls.GetInventoryOfProductAtAllLocations(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetById_WhenFound_CallsLocationService()
    {
      var id = Guid.NewGuid();
      var product = new ProductDto(id, "Found", "Desc", new Price(3.21m, "DKK"), null, null);
      _productRepo.Setup(r => r.GetById(id)).ReturnsAsync(product);
      _location_service.Setup(ls => ls.GetInventoryOfProductAtAllLocations(id)).ReturnsAsync(new List<InventoryEntryDto>());

      var result = await _sut.GetById(id);

      result.Should().NotBeNull();
      _location_service.Verify(ls => ls.GetInventoryOfProductAtAllLocations(id), Times.Once);
    }

    // ---- New tests for Update and Delete ----

    [Fact]
    public async Task Update_WhenRepositoryReturnsUpdatedDto_ReturnsUpdatedDto()
    {
      var id = Guid.NewGuid();
      var updateDto = new ProductUpdateDto(id, "Updated", "NewDesc", 12.34m, null);
      var updatedDto = new ProductDto(id, "Updated", "NewDesc", new Price(12.34m, "DKK"), null, null);

      _productRepo.Setup(r => r.Update(updateDto)).ReturnsAsync(updatedDto);

      var result = await _sut.Update(updateDto);

      result.Should().BeSameAs(updatedDto);
      _productRepo.Verify(r => r.Update(updateDto), Times.Once);
    }

    [Fact]
    public async Task Update_WhenRepositoryReturnsNull_ReturnsNull()
    {
      var updateDto = new ProductUpdateDto(Guid.NewGuid(), "NoOne", null, null, null);

      _productRepo.Setup(r => r.Update(updateDto)).ReturnsAsync((ProductDto?)null);

      var result = await _sut.Update(updateDto);

      result.Should().BeNull();
      _productRepo.Verify(r => r.Update(updateDto), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenRepositoryReturnsTrue_ReturnsTrue()
    {
      var id = Guid.NewGuid();
      _productRepo.Setup(r => r.Delete(id)).ReturnsAsync(true);

      var result = await _sut.Delete(id);

      result.Should().BeTrue();
      _productRepo.Verify(r => r.Delete(id), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenRepositoryReturnsFalse_ReturnsFalse()
    {
      var id = Guid.NewGuid();
      _productRepo.Setup(r => r.Delete(id)).ReturnsAsync(false);

      var result = await _sut.Delete(id);

      result.Should().BeFalse();
      _productRepo.Verify(r => r.Delete(id), Times.Once);
    }
  }
}