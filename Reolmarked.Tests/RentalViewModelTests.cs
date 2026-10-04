using Reolmarked.Core.Models;
using Reolmarked.Core.Repositories;
using Reolmarked.UI.ViewModels;

namespace Reolmarked.Tests
{
    [TestClass]
    public class RentalViewModelTests
    {
        //Tester, at bookingknappen er aktiveret, når en ledig reol og en reollejer er valgt.

        [TestMethod]
        public void BookingCommand_ShouldBeEnabled_WhenShelfIsAvailableAndRenterIsSelected()
        {
            //Arrange
            // Tre repositories, som ViewModel'en kræver.
            RentalRepository rentalRepository = new RentalRepository();
            ShelfRepository shelfRepository = new ShelfRepository();
            ShelfRenterRepository renterRepository = new ShelfRenterRepository();

            // Den viewmodel, vi vil teste.
            RentalViewModel viewModel = new RentalViewModel(
                rentalRepository,
                shelfRepository,
                renterRepository);

            // Forbered en ledig reol og en reollejer.
            Shelf shelf = new Shelf(1, ShelfConfiguration.SeksHylder, Status.Ledig);
            ShelfRenter renter = new ShelfRenter("Test", "Testesen", "12345678");

            // Vælg dem i ViewModel'en.
            viewModel.SelectedShelf = shelf;
            viewModel.SelectedRenter = renter;

            //Act
            //Vi tjekker, om bookingknappen er aktiveret.
            bool isBookingCommandEnabled = viewModel.BookCommand.CanExecute(null);

            //Assert
            Assert.IsTrue(isBookingCommandEnabled);
        }

        [TestMethod]
        public void BookingCommand_ShouldBeDisabled_WhenShelfIsBookedAndRenterIsSelected()
        {
            //Arrange
            RentalRepository rentalRepository = new RentalRepository();
            ShelfRepository shelfRepository = new ShelfRepository();
            ShelfRenterRepository renterRepository = new ShelfRenterRepository();
            RentalViewModel viewModel = new RentalViewModel(
                rentalRepository,
                shelfRepository,
                renterRepository);
            
            // Forbered en reol, der ikke er ledig.
            Shelf shelf = new Shelf(1, ShelfConfiguration.SeksHylder, Status.Booket);
            ShelfRenter renter = new ShelfRenter("Test", "Testesen", "12345678");
            viewModel.SelectedShelf = shelf;
            viewModel.SelectedRenter = renter;
            
            //Act
            bool isBookingCommandEnabled = viewModel.BookCommand.CanExecute(null);
            
            //Assert
            Assert.IsFalse(isBookingCommandEnabled);
        }
    }
}
