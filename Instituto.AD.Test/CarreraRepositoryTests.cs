using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.AD.Repositories;
using Moq;
using System.Collections.Generic;

namespace Instituto.AD.Test;

[TestClass]
public sealed class CarreraRepositoryTests
{
    private Mock<ICarreraRepository> _mockRepo = null!;
    private Carrera _testCarrera = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<ICarreraRepository>();
        _testCarrera = new Carrera
        {
            Id = 1,
            Nombre = "Tecnicatura en Programación",
            DuracionAnios = 3,
            Turno = "Mañana",
            Modalidad = "Presencial",
            Horario = "08:00-13:00",
            Estado = "Activa"
        };
    }

    [TestMethod]
    public void GetAll_ReturnsListOfCarreras()
    {
        // Arrange
        var expected = new List<Carrera> { _testCarrera };
        _mockRepo.Setup(r => r.GetAll()).Returns(expected);

        // Act
        var result = _mockRepo.Object.GetAll();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Tecnicatura en Programación", result[0].Nombre);
    }

    [TestMethod]
    public void GetById_ExistingId_ReturnsCarrera()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(1)).Returns(_testCarrera);

        // Act
        var result = _mockRepo.Object.GetById(1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result!.Id);
    }

    [TestMethod]
    public void GetById_NonExistingId_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(99)).Returns((Carrera?)null);

        // Act
        var result = _mockRepo.Object.GetById(99);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Create_ValidCarrera_ReturnsCarreraWithId()
    {
        // Arrange
        var newCarrera = new Carrera { Nombre = "Nueva Carrera", DuracionAnios = 2 };
        _mockRepo.Setup(r => r.Create(It.IsAny<Carrera>())).Returns<Carrera>(c => { c.Id = 5; return c; });

        // Act
        var result = _mockRepo.Object.Create(newCarrera);

        // Assert
        Assert.AreEqual(5, result.Id);
        Assert.AreEqual("Nueva Carrera", result.Nombre);
    }

    [TestMethod]
    public void Delete_ExistingId_CallsRepository()
    {
        // Arrange
        _mockRepo.Setup(r => r.Exists(1)).Returns(true);

        // Act
        _mockRepo.Object.Delete(1);

        // Assert
        _mockRepo.Verify(r => r.Delete(1), Times.Once);
    }

    [TestMethod]
    public void Exists_ExistingId_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(r => r.Exists(1)).Returns(true);

        // Act
        var result = _mockRepo.Object.Exists(1);

        // Assert
        Assert.IsTrue(result);
    }
}