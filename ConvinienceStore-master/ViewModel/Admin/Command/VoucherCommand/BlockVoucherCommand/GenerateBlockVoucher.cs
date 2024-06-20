using ConvenienceStore.Model.Admin;
using ConvenienceStore.Utils.Helpers;
using ConvenienceStore.ViewModel.Admin.AdminVM;
using ConvenienceStore.Views.Admin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ConvenienceStore.ViewModel.Admin.Command.VoucherCommand.BlockVoucherCommand
{
    class GenerateBlockVoucher : ICommand
    {
        VoucherVM VM;

        public GenerateBlockVoucher(VoucherVM VM)
        {
            this.VM = VM;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var window = parameter as VoucherView;

            window.PrefixCodeErrorMessage.Text = string.Empty;
            window.SuffixNumberErrorMessage.Text = string.Empty;
            window.NumberOfVoucherErrorMessage.Text = string.Empty;
            window.ValueErrorMessage.Text = string.Empty;
            window.StartDateErrorMessage.Text = string.Empty;
            window.FinishDateErrorMessage.Text = string.Empty;

            // Pre Validation
            bool isValid = true;

            if (string.IsNullOrEmpty(window.PrefixCode.Text))
            {
                window.PrefixCodeErrorMessage.Text = "Prefix code has not been entered!!";
                isValid = false;
            }

            int SuffixNumber = 0;
            if (string.IsNullOrEmpty(window.SuffixNumber.Text))
            {
                window.SuffixNumberErrorMessage.Text = "The suffix number character has not been entered!!";
                isValid = false;
            }
            else
            {
                if (!int.TryParse(window.SuffixNumber.Text, out SuffixNumber))
                {
                    window.SuffixNumberErrorMessage.Text = "The suffix number is not valid!!";
                    isValid = false;
                }
                else
                {
                    if (SuffixNumber < 4)
                    {
                        window.SuffixNumberErrorMessage.Text = "The suffix number must be >= 4!!";
                        isValid = false;
                    }
                    else
                    {
                        if (SuffixNumber > 10)
                        {
                            window.SuffixNumberErrorMessage.Text = "The suffix number more than 10!!";
                            isValid = false;
                        }
                    }
                }
            }

            int numberOfVoucher = 0;
            if (string.IsNullOrEmpty(window.NumberOfVoucher.Text))
            {
                window.NumberOfVoucherErrorMessage.Text = "The number of vouchers has not been entered!!";
                isValid = false;
            }
            else
            {

                if (!int.TryParse(window.NumberOfVoucher.Text, out numberOfVoucher))
                {
                    window.NumberOfVoucherErrorMessage.Text = "Invalid quantity!!";
                    isValid = false;
                }
                else
                {
                    if (numberOfVoucher <= 0)
                    {
                        window.NumberOfVoucherErrorMessage.Text = "Number of vouchers must be >= 0!!";
                        isValid = false;
                    }
                    else
                    {
                        if (SuffixNumber > 0)
                        {
                            long maximumNumberOfVoucher = 1;
                            for (int i = 1; i <= SuffixNumber; i++)
                            {
                                maximumNumberOfVoucher *= 36;
                            }
                            if (numberOfVoucher > maximumNumberOfVoucher / 10)
                            {
                                window.NumberOfVoucherErrorMessage.Text = "The number of vouchers is too large!!";
                                window.NumberOfVoucher.ToolTip = "The number of suffix characters is not a good guarantee for the number of vouchers\nSuggestion: Need to increase the number of suffix characters!!";
                                window.NumberOfVoucherErrorMessage.ToolTip = "The number of suffix characters is not a good guarantee for the number of vouchers\nSuggestion: Need to increase the number of suffix characters!!";
                                isValid = false;
                            }
                        }
                    }
                }
            }

            int value = 0;
            if (string.IsNullOrEmpty(window.Value.Text))
            {
                window.ValueErrorMessage.Text = "Voucher value has not been entered!!";
                isValid = false;
            }
            else
            {
                if (!int.TryParse(window.Value.Text, out value))
                {
                    window.ValueErrorMessage.Text = "Invalid value!!";
                    isValid = false;
                }
                else
                {
                    if (value <= 0)
                    {
                        window.ValueErrorMessage.Text = "The value must be greater than 0!!";
                        isValid = false;
                    }
                    else
                    {
                        if (window.TypeComboBox.SelectedIndex == 1 && value > 100)
                        {
                            window.ValueErrorMessage.Text = "Value not exceeding 100%!!";
                            isValid = false;
                        }
                    }
                }
            }

            if (window.StartDate.SelectedDate == null)
            {
                window.StartDateErrorMessage.Text = "No start date has been selected!!";
                isValid = false;
            }

            if (window.FinishDate.SelectedDate == null)
            {
                window.FinishDateErrorMessage.Text = "No end date has been selected!!";
                isValid = false;
            }

            if (window.StartDate.SelectedDate.HasValue && window.FinishDate.SelectedDate.HasValue && window.StartDate.SelectedDate > window.FinishDate.SelectedDate)
            {
                window.FinishDateErrorMessage.Text = "The start date must be less than or equal to the end date!!";
                isValid = false;
            }

            if (!isValid) return;
            // Pre Validation Done 

            var newBlockVoucher = new BlockVoucher()
            {
                ReleaseName = window.PrefixCode.Text,
                Type = window.TypeComboBox.SelectedIndex,
                ParValue = value,
                StartDate = (DateTime)window.StartDate.SelectedDate,
                FinishDate = (DateTime)window.FinishDate.SelectedDate,
            };

            window.PrefixCode.Text = string.Empty;
            window.SuffixNumber.Text = string.Empty;
            window.NumberOfVoucher.Text = string.Empty;
            window.Value.Text = string.Empty;
            window.StartDate.SelectedDate = null;
            window.FinishDate.SelectedDate = null;

            window.PrefixCodeErrorMessage.Text = string.Empty;
            window.SuffixNumberErrorMessage.Text = string.Empty;
            window.NumberOfVoucherErrorMessage.Text = string.Empty;
            window.ValueErrorMessage.Text = string.Empty;
            window.StartDateErrorMessage.Text = string.Empty;
            window.FinishDateErrorMessage.Text = string.Empty;

            if (newBlockVoucher.ReleaseName.ToLower().Contains(VM.SearchContent.ToLower()))
            {
                VM.ObservableBlockVouchers.Add(newBlockVoucher);
            }
            VM.blockVouchers.Add(newBlockVoucher);

            for (int i = 0; i < numberOfVoucher; i++)
            {
                newBlockVoucher.vouchers.Add(new Voucher()
                {
                    Code = newBlockVoucher.ReleaseName + RandomString(SuffixNumber),
                    Status = 0,
                });
            }

            int d;
            while ((d = newBlockVoucher.vouchers.Distinct().Count()) < numberOfVoucher)
            {
                newBlockVoucher.vouchers = newBlockVoucher.vouchers.Distinct().ToList();
                for (int i = 1; i <= numberOfVoucher - d; ++i)
                {
                    newBlockVoucher.vouchers.Add(new Voucher()
                    {
                        Code = newBlockVoucher.ReleaseName + RandomString(SuffixNumber),
                        Status = 0,
                    });
                }
            }

            DatabaseHelper.InsertBlockVoucher(newBlockVoucher);
            List<BlockVoucher> blockVouchers = DatabaseHelper.FetchingBlockVoucherData();
            ObservableCollection<BlockVoucher> ObservableBlockVouchers = new ObservableCollection<BlockVoucher>(blockVouchers);

            window.BlockVoucherCards.ItemsSource = ObservableBlockVouchers;
            window.BlockVoucherCards.Items.Refresh(); //Update item ngay khi add xong, by: Thuong

            VM.VoucherSnackbar.MessageQueue?.Enqueue($"Voucher randomly generated \"{newBlockVoucher.ReleaseName}\"", null, null, null, false, true, TimeSpan.FromSeconds(0.7));

            // Phần này Lâm đã check kĩ. Ngày 9/12/2022
        }
        private string RandomString(int size)
        {
            StringBuilder sb = new StringBuilder();
            int number;
            char c;
            Random rand = new Random();
            for (int i = 0; i < size; i++)
            {
                number = rand.Next(45, 90);
                if (65 <= number && number <= 90)
                {
                    c = Convert.ToChar(number);
                }
                else
                {
                    c = Convert.ToChar(48 + number % 10);
                }

                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
