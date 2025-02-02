using System;
using System.Collections.Generic;
using System.Linq;

namespace GICApplication
{
    public class GICBankRepository : IGICBankRepository
    {
        /* Declartion */
        private static List<Account>? accounts = null;
        private static List<InterestRule>? interestRule = null;        

        public GICBankRepository()
        {
            accounts = new List<Account>();
            interestRule = new List<InterestRule>();
        }

        public void AddTransactions(InputTransaction inputData)
        {
            // check account already exists or not
            var account = accounts?.FirstOrDefault(x => x.AccountNumber == inputData.AccountNumber);

            // check transaction details
            if (!IsValidTransaction(inputData, account))
                return;

            var accountCount = accounts?.Count > 0 ? accounts.Select(x => x.AccountNumber == inputData.AccountNumber).Count() + 1 : 1;

            // get balance of the account object            
            var balance = accounts?.FirstOrDefault(x => x.AccountNumber == inputData.AccountNumber)?.AvailableBalance ?? 0;

            // add/subtract the balance amount with current transaction amount
            var amount = inputData.TransactionType.ToUpper() == Enum.GetName(typeof(TransactionType), TransactionType.W) ? balance - inputData.Amount : balance + inputData.Amount;

            var transaction = new Transaction
            {
                TransactionId = GetTransactionId(inputData.TransactionDate, inputData.TransactionType, account),                
                AccountNumber = account == null ? inputData.AccountNumber : account.AccountNumber,
                Amount = inputData.Amount,
                TransactionDate = inputData.TransactionDate,
                TransactionType = inputData.TransactionType,
                Balance = amount
            };

            // add/update account transaction details
            if (account == null)
            {
                accounts?.Add(new Account
                {                    
                    AccountNumber =  inputData.AccountNumber,
                    AvailableBalance = amount,
                    Transactions = new List<Transaction> { transaction }
                });
            }
            else
            {
                account.AvailableBalance = amount;
                account.Transactions.Add(transaction);
            }
        }

        public void AddInterestRules(InputRule inputData)
        {
            // check if the interest rate is valid
            if (Convert.ToDouble(inputData.Rate) < 0)
            {
                Console.WriteLine("\nInvalid interest rate.");
                return;
            }

            // add interest rule
            var rule = new InterestRule
            {
                Date = inputData.RuleDate,
                RuleId = inputData.RuleId,
                Rate = inputData.Rate
            };

            interestRule?.Add(rule);
        }

        public void PrintStatement(InputPrint inputData)
        {
            var statementData = accounts?.Where(x => x.AccountNumber == inputData.AccountNumber)
                .SelectMany(x => x.Transactions)?.Where(z => z.TransactionDate.ToString("yyyyMM") == inputData.MonthYear.ToString("yyyyMM")).ToList();

            // check and print the statement
            if (statementData == null || statementData.Count() == 0)
            {
                Console.WriteLine("No transactions found for the account and month.");
                return;
            }

            // calculate interest amount & add interest amount row
            CalculateInterest(statementData, inputData.MonthYear, accounts?.Where(x => x.AccountNumber == inputData.AccountNumber).First());

            // Print header
            Console.WriteLine("Account: " + inputData.AccountNumber);
            Console.WriteLine("| {0,-10} | {1,-11} | {2,-6} | {3,-10} | {4,-10} |", "Date", "Txn Id", "Type", "Amount", "Balance");
            Console.WriteLine(new string('-', 60));

            // Print transactions
            foreach (var txn in statementData)
            {
                Console.WriteLine("| {0,-10} | {1,-11} | {2,-6} | {3,10:N2} | {4,10:N2} |",
                    txn.TransactionDate.ToString("yyyyMMdd"),
                    txn.TransactionId,
                    txn.TransactionType,
                    txn.Amount,
                    txn.Balance);
            }
        }

        private string GetTransactionId(DateTime transactionDate, string transactionType, Account? account)
        {
            // check transaction count for the day
            var count = account != null ? account.Transactions?.Select(x => x.TransactionType.ToUpper() == transactionType.ToUpper()).Count() + 1 : 1;

            return transactionDate.ToString("yyyyMMdd") + "-" + string.Format("{0:00}", count);
        }

        private bool IsValidTransaction(InputTransaction transaction, Account? account)
        {
            // check if the input is correct
            if (transaction == null)
            {
                Console.WriteLine("\nIncorrect transactions data, please enter transaction details in <Date> <Account> <Type> <Amount> format.");
                return false;
            }

            // no deposit but withdrawal request received.
            if (account == null && transaction.TransactionType.ToUpper() == Enum.GetName(typeof(TransactionType), TransactionType.W))
            {
                Console.WriteLine("\nAccount Balance is zero hence withdrawal is not allowed for this account.");
                return false;
            }

            // check insufficient balance for withdrawal
            if (account != null && transaction.TransactionType.ToUpper() == Enum.GetName(typeof(TransactionType), TransactionType.W) && account.AvailableBalance < transaction.Amount)
            {
                Console.WriteLine("\nInsufficient balance for withdrawal.");
                return false;
            }

            // check if the account balance is valid
            if (Convert.ToDouble(transaction.Amount) < 0)
            {
                Console.WriteLine("\nInvalid balance amount.");
                return false;
            }

            // default if no issue found
            return true;
        }

        private void CalculateInterest(List<Transaction>? transactions, DateTime statementMonthYear, Account? account)
        {
            if (transactions == null || transactions.Count == 0 || account == null || interestRule ==null || interestRule.Count ==0)
            {
                return;
            }

            double totalInterest = 0.0;

            // get number of days for given month & year
            DateTime statementDate = statementMonthYear;
            int totalDays = DateTime.DaysInMonth(statementDate.Year, statementDate.Month);

            for (int iCnt = 0; iCnt < transactions.Count; iCnt++)
            {
                // calculate start and end date for each transaction
                DateTime startDate = transactions[iCnt].TransactionDate;
                DateTime endDate = iCnt == transactions.Count - 1 ? statementDate.AddDays(totalDays) : transactions[iCnt + 1].TransactionDate;

                // get interest rate & amount for each day
                for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
                {
                    double rate = interestRule?.Where(r => r.Date <= date).OrderByDescending(r => r.Date).FirstOrDefault()?.Rate ?? 0.0;

                    // calculate interest amount
                    double dailyInterest = transactions[iCnt]?.Balance * (rate / 100) ?? 0.0;
                    totalInterest += Math.Round(dailyInterest, 2);
                }
            }

            // total interest amount
            var totalInterestAmount = Math.Round(totalInterest / 365, 2);

            // add interest entry in the transaction list
            transactions.Add(new Transaction
            {
                AccountNumber = account.AccountNumber,
                TransactionDate = statementDate.AddDays(totalDays - 1),
                Amount = totalInterestAmount,
                Balance = account.AvailableBalance + totalInterestAmount,
                TransactionType = Enum.GetName(typeof(TransactionType), TransactionType.I)
            });
        }
    }
}
