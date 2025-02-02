using System;
using System.Globalization;

namespace GICApplication
{
    public class GICBankService
    {
        private readonly IGICBankRepository _transactionRepository;

        public GICBankService(IGICBankRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public void AddTransactions(string input)
        {
            try
            {
                // split the input string
                string[] transactionDetails = input.Split(' ');

                if (transactionDetails.Length < 4)
                {
                    Console.WriteLine("Incorrect transaction data, please enter transaction details in <Date> <Account> <Type> <Amount> format.");
                    return;
                }

                var transaction = new InputTransaction
                {
                    TransactionDate = DateTime.ParseExact(transactionDetails[0], "yyyyMMdd", CultureInfo.InvariantCulture),
                    AccountNumber = transactionDetails[1],
                    TransactionType = transactionDetails[2],
                    Amount = double.Parse(transactionDetails[3])
                };

                _transactionRepository.AddTransactions(transaction);
            }
            catch
            {
                Console.WriteLine("Incorrect interest transaction data, please enter transaction details in <Date> <Account> <Type> <Amount> format.");
            }
        }

        public void AddInterestRules(string input)
        {
            try
            {
                // split the input string
                string[] interestRules = input.Split(' ');

                if (interestRules.Length < 3)
                {
                    Console.WriteLine("Incorrect interest rule data, please enter interest rule details in <Date> <RuleId> <Rate in %> format.");
                    return;
                }

                var rule = new InputRule
                {
                    RuleId = interestRules[1],
                    RuleDate = DateTime.ParseExact(interestRules[0], "yyyyMMdd", CultureInfo.InvariantCulture),
                    Rate = double.Parse(interestRules[2])
                };

                _transactionRepository.AddInterestRules(rule);
            }
            catch
            {
                Console.WriteLine("Incorrect interest rule data, please enter interest rule details in <Date> <RuleId> <Rate in %> format.");
                throw;
            }
        }

        public void PrintStatement(string input)
        {
            try
            {
                // split the input string
                string[] print = input.Split(' ');

                if (print.Length < 2)
                {
                    Console.WriteLine("Incorrect print data, please enter account and month to generate the statement <Account> <Year><Month>.");
                    return;
                }

                var printStatement = new InputPrint
                {
                    AccountNumber = print[0],
                    MonthYear = DateTime.ParseExact(print[1], "yyyyMM", CultureInfo.InvariantCulture)
                };

                _transactionRepository.PrintStatement(printStatement);
            }
            catch
            {
                Console.WriteLine("Incorrect print data, please enter account and month to generate the statement <Account> <Year><Month>.");
                throw;
            }
        }

    }
}
