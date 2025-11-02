using WinCheck.Core.Helpers;

namespace WinCheck.Tests.Helpers;

[TestClass]
public class ValidationHelperTests
{
    [TestMethod]
    public void IsValidDriveLetter_WithValidLetter_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(ValidationHelper.IsValidDriveLetter("C"));
        Assert.IsTrue(ValidationHelper.IsValidDriveLetter("D"));
        Assert.IsTrue(ValidationHelper.IsValidDriveLetter("Z"));
        Assert.IsTrue(ValidationHelper.IsValidDriveLetter("c")); // lowercase should work
    }

    [TestMethod]
    public void IsValidDriveLetter_WithInvalidInput_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.IsFalse(ValidationHelper.IsValidDriveLetter(""));
        Assert.IsFalse(ValidationHelper.IsValidDriveLetter(null!));
        Assert.IsFalse(ValidationHelper.IsValidDriveLetter("CC"));
        Assert.IsFalse(ValidationHelper.IsValidDriveLetter("1"));
        Assert.IsFalse(ValidationHelper.IsValidDriveLetter("@"));
    }

    [TestMethod]
    public void IsValidProcessId_WithValidId_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(ValidationHelper.IsValidProcessId(1));
        Assert.IsTrue(ValidationHelper.IsValidProcessId(1000));
        Assert.IsTrue(ValidationHelper.IsValidProcessId(int.MaxValue));
    }

    [TestMethod]
    public void IsValidProcessId_WithInvalidId_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.IsFalse(ValidationHelper.IsValidProcessId(0));
        Assert.IsFalse(ValidationHelper.IsValidProcessId(-1));
        Assert.IsFalse(ValidationHelper.IsValidProcessId(-1000));
    }

    [TestMethod]
    public void IsValidApiKeyFormat_WithValidKey_ReturnsTrue()
    {
        // Arrange
        var validKey = "sk-1234567890abcdefghijklmnopqrstuvwxyz";

        // Act & Assert
        Assert.IsTrue(ValidationHelper.IsValidApiKeyFormat(validKey));
    }

    [TestMethod]
    public void IsValidApiKeyFormat_WithInvalidKey_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.IsFalse(ValidationHelper.IsValidApiKeyFormat(""));
        Assert.IsFalse(ValidationHelper.IsValidApiKeyFormat(null!));
        Assert.IsFalse(ValidationHelper.IsValidApiKeyFormat("short"));
        Assert.IsFalse(ValidationHelper.IsValidApiKeyFormat("key with spaces and special chars!@#"));
    }

    [TestMethod]
    public void IsValidPercentage_WithValidValue_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(ValidationHelper.IsValidPercentage(0));
        Assert.IsTrue(ValidationHelper.IsValidPercentage(50));
        Assert.IsTrue(ValidationHelper.IsValidPercentage(100));
        Assert.IsTrue(ValidationHelper.IsValidPercentage(99.99));
    }

    [TestMethod]
    public void IsValidPercentage_WithInvalidValue_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.IsFalse(ValidationHelper.IsValidPercentage(-1));
        Assert.IsFalse(ValidationHelper.IsValidPercentage(101));
        Assert.IsFalse(ValidationHelper.IsValidPercentage(-100));
        Assert.IsFalse(ValidationHelper.IsValidPercentage(200));
    }

    [TestMethod]
    public void IsValidPortNumber_WithValidPort_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(ValidationHelper.IsValidPortNumber(1));
        Assert.IsTrue(ValidationHelper.IsValidPortNumber(80));
        Assert.IsTrue(ValidationHelper.IsValidPortNumber(443));
        Assert.IsTrue(ValidationHelper.IsValidPortNumber(8080));
        Assert.IsTrue(ValidationHelper.IsValidPortNumber(65535));
    }

    [TestMethod]
    public void IsValidPortNumber_WithInvalidPort_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.IsFalse(ValidationHelper.IsValidPortNumber(0));
        Assert.IsFalse(ValidationHelper.IsValidPortNumber(-1));
        Assert.IsFalse(ValidationHelper.IsValidPortNumber(65536));
        Assert.IsFalse(ValidationHelper.IsValidPortNumber(100000));
    }

    [TestMethod]
    public void SanitizeFileName_WithValidName_ReturnsCleanName()
    {
        // Arrange
        var fileName = "My Document.txt";

        // Act
        var result = ValidationHelper.SanitizeFileName(fileName);

        // Assert
        Assert.AreEqual("My Document.txt", result);
    }

    [TestMethod]
    public void SanitizeFileName_WithInvalidChars_ReplacesWithUnderscore()
    {
        // Arrange
        var fileName = "My<Document>:File?.txt";

        // Act
        var result = ValidationHelper.SanitizeFileName(fileName);

        // Assert
        Assert.IsFalse(result.Contains('<'));
        Assert.IsFalse(result.Contains('>'));
        Assert.IsFalse(result.Contains(':'));
        Assert.IsFalse(result.Contains('?'));
        Assert.IsTrue(result.Contains('_'));
    }

    [TestMethod]
    public void SanitizeFileName_WithEmptyInput_ReturnsUnnamed()
    {
        // Arrange & Act & Assert
        Assert.AreEqual("unnamed", ValidationHelper.SanitizeFileName(""));
        Assert.AreEqual("unnamed", ValidationHelper.SanitizeFileName(null!));
        Assert.AreEqual("unnamed", ValidationHelper.SanitizeFileName("   "));
    }

    [TestMethod]
    public void IsValidIPAddress_WithValidIP_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(ValidationHelper.IsValidIPAddress("127.0.0.1"));
        Assert.IsTrue(ValidationHelper.IsValidIPAddress("192.168.1.1"));
        Assert.IsTrue(ValidationHelper.IsValidIPAddress("8.8.8.8"));
    }

    [TestMethod]
    public void IsValidIPAddress_WithInvalidIP_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.IsFalse(ValidationHelper.IsValidIPAddress(""));
        Assert.IsFalse(ValidationHelper.IsValidIPAddress(null!));
        Assert.IsFalse(ValidationHelper.IsValidIPAddress("invalid"));
        Assert.IsFalse(ValidationHelper.IsValidIPAddress("999.999.999.999"));
        Assert.IsFalse(ValidationHelper.IsValidIPAddress("not.an.ip.address"));
    }
}
