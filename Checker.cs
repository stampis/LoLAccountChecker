﻿#region License

// Copyright 2015 LoLAccountChecker
// 
// This file is part of LoLAccountChecker.
// 
// LoLAccountChecker is free software: you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// LoLAccountChecker is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License 
// along with LoLAccountChecker. If not, see http://www.gnu.org/licenses/.

#endregion

#region

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LoLAccountChecker.Classes;
using LoLAccountChecker.Views;

#endregion

namespace LoLAccountChecker
{
    internal delegate void NewAccount(Account account);

    internal static class Checker
    {
        public static List<Account> Accounts { get; set; }
        public static bool IsChecking { get; private set; }

        static Checker()
        {
            Accounts = new List<Account>();
            IsChecking = false;
        }
        public static void Start()
        {
            if (IsChecking)
            {
                return;
            }

            IsChecking = true;

            Thread thread = new Thread(Handler)
            {
                IsBackground = true
            };

            thread.Start();
        }

        public static void Stop()
        {
            if (!IsChecking)
            {
                return;
            }

            IsChecking = false;

            MainWindow.Instance.UpdateControls();

            if (AccountsWindow.Instance != null)
            {
                AccountsWindow.Instance.UpdateControls();
            }
        }

        public static void Refresh(bool e = false)
        {
            if (IsChecking)
            {
                return;
            }

            IsChecking = true;

            foreach (Account account in Accounts.Where(a => a.State == Account.Result.Success || e))
            {
                account.State = Account.Result.Unchecked;
            }

            Start();
        }

        private static async void Handler()
        {
            while (Accounts.Any(a => a.State == Account.Result.Unchecked))
            {
                if (!IsChecking)
                {
                    break;
                }
                Account account = Accounts.FirstOrDefault(a => a.State == Account.Result.Unchecked);

                if (account == null)
                {
                    continue;
                }

                Client client = new Client(account.Region, account.Username, account.Password);
                await client.IsCompleted.Task;

                int i = Accounts.FindIndex(a => a.Username == account.Username);
                Accounts[i] = client.Data;

                MainWindow.Instance.UpdateControls();

                if (AccountsWindow.Instance != null)
                {
                    AccountsWindow.Instance.UpdateControls();
                }
            }

            Stop();
        }
    }
}