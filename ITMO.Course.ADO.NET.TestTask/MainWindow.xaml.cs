using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfEfApp.Models;

namespace WpfEfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private AdventureWorks2017Context dbContext;


        public List<Person> Persons
        {
            get;
            set;
        }


        public MainWindow()
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dbContext != null)
                dbContext.Dispose();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var builder = new ConfigurationBuilder();
                var basepath = Directory.GetCurrentDirectory();
                builder.SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile("appsettings.json");
                var config = builder.Build();
                string connectionString = config.GetConnectionString("DefaultConnection");

                var optionsBuilder = new DbContextOptionsBuilder<AdventureWorks2017Context>();
                var options = optionsBuilder
                    .UseSqlServer(connectionString)
                    .Options;

                dbContext = new AdventureWorks2017Context(options);

                Persons = dbContext.Person.ToList();

                this.listViewPersons.ItemsSource = Persons;

                this.btnConnect.IsEnabled = false;
                this.textBlockStatus.Text = "Соединение с базой данных выполнено успешно";

            }
            catch
            {
                this.btnConnect.IsEnabled = true;
                this.textBlockStatus.Text = "Ошибка соединения с базой данных";
            }



        }

        private void listBoxPersons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var person = (Person)listViewPersons.SelectedItem;
            if (person != null)
            {
                dataGridPhones.ItemsSource = dbContext.PersonPhone.Where(x => x.BusinessEntityId == person.BusinessEntityId).ToList();
                this.tbLastName.Text = person.LastName;
                this.tbFirstName.Text = person.FirstName;
                this.tbMiddleName.Text = person.MiddleName;
            }
        }

        private void btnPersonSave_Click(object sender, RoutedEventArgs e)
        {
            var person = (Person)listViewPersons.SelectedItem;
            if (person != null)
            {
                var p = dbContext.Person.Where(x => x.BusinessEntityId == person.BusinessEntityId).FirstOrDefault();
                p.FirstName = tbFirstName.Text;
                p.MiddleName = tbMiddleName.Text;
                p.LastName = tbLastName.Text;
                dbContext.SaveChanges();

                person.FirstName = tbFirstName.Text;
                person.MiddleName = tbMiddleName.Text;
                person.LastName = tbLastName.Text;

                listViewPersons.Items.Refresh();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var person = (Person)listViewPersons.SelectedItem;
            if (person != null)
            {
                Button button = sender as Button;
                PersonPhone selectedPhone = button.DataContext as PersonPhone;
                if (selectedPhone != null)
                {
                    dbContext.PersonPhone.Remove(selectedPhone);
                    dbContext.SaveChanges();
                    dataGridPhones.ItemsSource = dbContext.PersonPhone.Where(x => x.BusinessEntityId == person.BusinessEntityId).ToList();

                }
            }
        }

        private void btnAddPhone_Click(object sender, RoutedEventArgs e)
        {
            var newPhoneNumber = tbAddPhone.Text;
            if (!newPhoneNumber.IsNullOrEmpty())
            {
                var person = (Person)listViewPersons.SelectedItem;
                if (person != null)
                {
                    var existPhone = dbContext.PersonPhone
                        .Where(x => x.BusinessEntityId == person.BusinessEntityId && x.PhoneNumber == newPhoneNumber)
                        .FirstOrDefault();
                    if (existPhone != null)
                    {

                    }
                    else
                    {
                        var newPersonPhone = new PersonPhone();
                        newPersonPhone.BusinessEntityId = person.BusinessEntityId;
                        newPersonPhone.PhoneNumber = newPhoneNumber;
                        newPersonPhone.PhoneNumberTypeId = 1;
                        newPersonPhone.ModifiedDate = DateTime.Now;
                        dbContext.PersonPhone.Add(newPersonPhone);
                        dbContext.SaveChanges();
                        dataGridPhones.ItemsSource = dbContext.PersonPhone.Where(x => x.BusinessEntityId == person.BusinessEntityId).ToList();

                    }

                }
            }

        }
    }
}
