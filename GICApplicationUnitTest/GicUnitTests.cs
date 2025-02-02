using NUnit.Framework;
using Moq;
using Assert = Xunit.Assert;
using Xunit;
using System.Globalization;

namespace GICApplication
{
    [TestFixture]
    public class GicUnitTests
    {
        private readonly Mock<IGICBankRepository> _mockRepository;
        private readonly GICBankService _GICBankservice;

        public GicUnitTests()
        {
            // Initialize the mock repository.
            _mockRepository = new Mock<IGICBankRepository>();

            // Initialize the TransactionProcessor with the mock repository.
            _GICBankservice = new GICBankService(_mockRepository.Object);
        }

        [Fact]
        public void AddTransactions_SuccessUseCase()
        {
            // Arrange
            string input = "20230601 AC001 D 100.00";

            // Act
            _GICBankservice.AddTransactions(input);

            // Assert
            _mockRepository.Verify(repo => repo.AddTransactions(It.Is<InputTransaction>(t =>
             t.TransactionDate == new DateTime(2023, 06, 01) &&
             t.AccountNumber == "AC001" &&
             t.TransactionType == "D" &&
             t.Amount == 100.00
            )), Times.Once);
        }

        [Fact]
        public void AddTransactions_InvalidDateFormat()
        {
            // Arrange
            var input = "10-25-2023 12345678 DEPOSIT 100.00";

            // Act
            var ex = Record.Exception(() => _GICBankservice.AddTransactions(input));

            // Assert
            Assert.Null(ex);
            _mockRepository.Verify(repo => repo.AddTransactions(It.IsAny<InputTransaction>()), Times.Never);
        }

        [Fact]
        public void AddTransactions_MissingFields()
        {
            // Arrange
            var input = "20231025 12345678 D";

            // Act
            var ex = Record.Exception(() => _GICBankservice.AddTransactions(input));

            // Assert
            Assert.Null(ex);
            _mockRepository.Verify(repo => repo.AddTransactions(It.IsAny<InputTransaction>()), Times.Never);
        }

        [Fact]
        public void AddInterestRules_SuccessUseCase()
        {
            // Arrange
            string input = "20230601 1 10";

            // Act
            _GICBankservice.AddInterestRules(input);

            // Assert
            _mockRepository.Verify(repo => repo.AddInterestRules(It.Is<InputRule>(t =>
             t.RuleDate == new DateTime(2023, 06, 01) &&
             t.RuleId == "1" &&
             t.Rate == 10
            )), Times.Once);
        }

        [Fact]
        public void AddInterestRules_InvalidInput()
        {
            // Arrange
            string input = "20230601 1";

            // Act
            var ex = Record.Exception(() => _GICBankservice.AddInterestRules(input));

            // Assert
            Assert.Null(ex);
            _mockRepository.Verify(repo => repo.AddInterestRules(It.IsAny<InputRule>()), Times.Never);
        }

        [Fact]
        public void PrintStatement_SuccessUseCase()
        {
            // Arrange
            string input = "AC001 202306";

            // Act
            _GICBankservice.PrintStatement(input);

            // Assert
            _mockRepository.Verify(repo => repo.PrintStatement(It.Is<InputPrint>(t =>
             t.AccountNumber == "AC001" &
             t.MonthYear == DateTime.ParseExact("202306", "yyyyMM", CultureInfo.InvariantCulture)
            )), Times.Once);
        }

        [Fact]
        public void PrintStatement_InvalidInput()
        {
            // Arrange
            string input = "AC001";

            // Act
            var ex = Record.Exception(() => _GICBankservice.PrintStatement(input));

            // Assert
            Assert.Null(ex);
            _mockRepository.Verify(repo => repo.PrintStatement(It.IsAny<InputPrint>()), Times.Never);
        }

    }
}