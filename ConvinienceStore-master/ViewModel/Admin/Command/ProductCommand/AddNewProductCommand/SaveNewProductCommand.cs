using ConvenienceStore.Model.Admin;
using ConvenienceStore.Utils.Helpers;
using ConvenienceStore.Utils.Validation;
using ConvenienceStore.ViewModel.Admin.AdminVM;
using ConvenienceStore.Views.Admin.ProductWindow;
using FluentValidation;
using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ConvenienceStore.ViewModel.Admin.Command.ProductCommand.AddNewProductCommand
{
    public class SaveNewProductCommand : ICommand
    {
        InputInfoVM VM;

        public SaveNewProductCommand(InputInfoVM VM)
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
            var window = parameter as AddNewProductWindow;

            window.BarcodeErrorMessage.Text = string.Empty;
            window.TitleErrorMessage.Text = string.Empty;
            window.TypeErrorMessage.Text = string.Empty;
            window.CostErrorMessage.Text = string.Empty;
            window.PriceErrorMessage.Text = string.Empty;
            window.ManufacturingDateErrorMessage.Text = string.Empty;
            window.ExpiryDateErrorMessage.Text = string.Empty;
            window.StockErrorMessage.Text = string.Empty;
            window.ImageProductErrorMessage.Text = string.Empty;

            // Pre Validation
            bool isValid = true;

            if (string.IsNullOrEmpty(window.BarcodeTextBox.Text))
            {
                window.BarcodeErrorMessage.Text = "Barcode has not been entered!!";
                isValid = false;
            }

            if (string.IsNullOrEmpty(window.TitleTextBox.Text))
            {
                window.TitleErrorMessage.Text = "Product name has not been entered!!";
                isValid = false;
            }

            if (window.TypeComboBox.SelectedIndex == -1)
            {
                window.TypeErrorMessage.Text = "Haven't selected product type yet!!";
                isValid = false;
            }

            if (string.IsNullOrEmpty(window.CostTextBox.Text))
            {
                window.CostErrorMessage.Text = "Entry price has not been entered!!";
                isValid = false;
            }
            else
            {
                if (!int.TryParse(window.CostTextBox.Text, out _))
                {
                    window.CostErrorMessage.Text = "Invalid entry price!!";
                    isValid = false;
                }
            }

            if (string.IsNullOrEmpty(window.PriceTextBox.Text))
            {
                window.PriceErrorMessage.Text = "Selling price has not been entered!!";
                isValid = false;
            }
            else
            {
                if (!int.TryParse(window.PriceTextBox.Text, out _))
                {
                    window.PriceErrorMessage.Text = "Invalid selling price!!";
                    isValid = false;
                }
            }

            // Update CHK_Cost_Price Cost < Price
            if (int.TryParse(window.CostTextBox.Text, out int cost) && int.TryParse(window.PriceTextBox.Text, out int price))
            {
                if (cost >= price)
                {
                    window.PriceErrorMessage.Text = "Selling price must be > entry price!!";
                    isValid = false;
                }
            }


            if (string.IsNullOrEmpty(window.StockTextBox.Text))
            {
                window.StockErrorMessage.Text = "Quantity has not been entered!!";
                isValid = false;
            }
            else
            {
                int Stock;
                if (!int.TryParse(window.StockTextBox.Text, out Stock))
                {
                    window.StockErrorMessage.Text = "Invalid quantity!!";
                    isValid = false;
                }
                else
                {
                    if (Stock <= 0)
                    {
                        {
                            window.StockErrorMessage.Text = "Quantity must be > 0!!";
                            isValid = false;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(window.ManufacturingDate.Text))
            {
                window.ManufacturingDateErrorMessage.Text = "Production date has not been selected!!";
                isValid = false;
            }

            if (string.IsNullOrEmpty(window.ExpiryDateTextBlock.Text))
            {
                window.ExpiryDateErrorMessage.Text = "Expiration date has not been selected!!";
                isValid = false;
            }


            /* Update CHK_Date NSX < HSD 
             * NSX < InputDate
             * InputDate < HSD */
            if (window.ManufacturingDate.SelectedDate.HasValue && window.ExpiryDate.SelectedDate.HasValue)
            {
                if (window.ManufacturingDate.SelectedDate > VM.SelectedInputInfo.InputDate)
                {
                    window.ManufacturingDateErrorMessage.Text = "Production date must <= Import date";
                    isValid = false;
                }

                if (window.ExpiryDate.SelectedDate <= VM.SelectedInputInfo.InputDate)
                {
                    window.ExpiryDateErrorMessage.Text = "Expenditure must be > Date of import";
                    isValid = false;
                }
            }

            // Check xem manager đã upload ảnh lên chưa
            //if (window.ImageProduct.ImageSource == null)
            //{
            //    window.ImageProductErrorMessage.Text = "Chưa tải ảnh lên";
            //    isValid = false;
            //}

            if (!isValid) return;
            // Pre Validation Done 


            var newProduct = new Product()
            {
                InputInfoId = VM.SelectedInputInfo.Id,
                Barcode = window.BarcodeTextBox.Text,
                Title = window.TitleTextBox.Text,
                Type = (string)window.TypeComboBox.SelectionBoxItem,
                ProductionSite = window.CountryTextBlock.Text,
                Cost = int.Parse(window.CostTextBox.Text),
                Price = int.Parse(window.PriceTextBox.Text),
                Stock = int.Parse(window.StockTextBox.Text),
                ManufacturingDate = (DateTime)window.ManufacturingDate.SelectedDate,
                ExpiryDate = (DateTime)window.ExpiryDate.SelectedDate,
                Discount = 0,
            };

            newProduct.InStock = newProduct.Stock;

            if (window.ImageProduct.ImageSource != null)
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(window.ImageProduct.ImageSource as BitmapImage));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    newProduct.Image = ms.ToArray();
                }
            }
            else
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(new Uri("../../../Resources/Images/IconForProduct/image-whiteBG.jpg", UriKind.Relative)));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    newProduct.Image = ms.ToArray();
                }
            }


            ProductValidator validator = new ProductValidator();

            // Note For Me: stops executing a rule as soon as a validator fails
            // See more: https://docs.fluentvalidation.net/en/latest/cascade.html
            validator.RuleLevelCascadeMode = CascadeMode.Stop;

            var results = validator.Validate(newProduct);

            if (results.IsValid == false)
            {
                foreach (var error in results.Errors)
                {
                    if (error.PropertyName == "Barcode")
                        window.BarcodeErrorMessage.Text = error.ErrorMessage;

                    if (error.PropertyName == "Title")
                        window.TitleErrorMessage.Text = error.ErrorMessage;

                    if (error.PropertyName == "Cost")
                        window.CostErrorMessage.Text = error.ErrorMessage;

                    if (error.PropertyName == "Price")
                        window.PriceErrorMessage.Text = error.ErrorMessage;
                }
                return;
            }

            VM.ProductSnackbar.MessageQueue?.Enqueue($"Added Product Lots \"{newProduct.Title}\"", null, null, null, false, true, TimeSpan.FromSeconds(0.8));

            if (newProduct.Barcode.Contains(VM.SearchContent))
            {
                VM.ObservableProducts.Add(newProduct);
            }
            VM.products.Add(newProduct);
            DatabaseHelper.InsertProduct(newProduct);
            window.Close();
        }
    }
}
