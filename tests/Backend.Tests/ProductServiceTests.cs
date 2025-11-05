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
    private readonly Mock<ILocationService> _locationService = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
      _sut = new ProductService(_productRepo.Object, _locationService.Object);
    }

    [Fact]
    public async Task Create_CallsRepositoryAndReturnsCreatedDto()
    {
      var createDto = new ProductCreateDto("Test product", "Description", 9.99m, null);
      var returnedDto = new ProductDto(Guid.NewGuid(), "Test product", "Description", new Price(9.99m, "$"), null, new List<InventoryEntryDto>());

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
        new ProductDto(Guid.NewGuid(), "P1", "Desc1", new Price(1.23m, "$"), null, null),
        new ProductDto(Guid.NewGuid(), "P2", "Desc2", new Price(4.56m, "$"), null, null)
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
      _locationService.Verify(ls => ls.GetInventoryOfProductAtAllLocations(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetById_WhenFound_CallsLocationService()
    {
      var id = Guid.NewGuid();
      var product = new ProductDto(id, "Found", "Desc", new Price(3.21m, "$"), null, null);
      _productRepo.Setup(r => r.GetById(id)).ReturnsAsync(product);
      _locationService.Setup(ls => ls.GetInventoryOfProductAtAllLocations(id)).ReturnsAsync(new List<InventoryEntryDto>());

      var result = await _sut.GetById(id);

      result.Should().NotBeNull();
      _locationService.Verify(ls => ls.GetInventoryOfProductAtAllLocations(id), Times.Once);
    }
  }
}