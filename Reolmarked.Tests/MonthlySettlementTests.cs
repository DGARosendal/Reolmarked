using System;
using System.Collections.Generic;
using System.Text;
using Reolmarked.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reolmarked.Tests
{
    [TestClass]
    public class MonthlySettlementTests
    {
        //Tester, at kommisionen er 100 kr, ved et salg på 1000 kr.
        [TestMethod]
        public void Commission_ShouldBe100_WhenSaleIs1000()
        {


            //Arrange: Forbered et salg på 1000 kr.
            double totalSale = 1000;

            //Act: Opret en afregning, som beregner kommissionen.
            MonthlySettlement settlement = new MonthlySettlement(
                renterId: 1,
                month: 11,
                totalSales: totalSale,
                shelfCount: 1,
                extraDiscount: 0);

            //Assert: Kontroller, at kommissionen er 100 kr.
            Assert.AreEqual(100, settlement.Commission);
        }

        //Tester, at et negativt salgsbeløb bliver afvist med en ArgumentException.

        [TestMethod]
        public void Constructor_NegativeTotalSales_ShouldThrowArgumentException()
        {
            //Arrange: Forbered et negativt salgsbeløb.
            double negativeSale = -1000;

            // Act & Assert: Kontrollér, at det negative salg bliver afvist.
            Assert.ThrowsExactly<ArgumentException>(() => new MonthlySettlement(
                renterId: 1,
                month: 11,
                totalSales: negativeSale,
                shelfCount: 1,
                extraDiscount: 0));
        }

        //Tester, at kommisionen er 0 kr, ved et salg på 0 kr.
       
        [TestMethod]

        public void Commission_ShouldBe0_WhenSaleIs0()
        {
            //Arrange: Forbered et salg på 0 kr.
            double totalSale = 0;
            
            //Act: Opret en afregning, som beregner kommissionen.
            MonthlySettlement settlement = new MonthlySettlement(
                renterId: 1,
                month: 11,
                totalSales: totalSale,
                shelfCount: 1,
                extraDiscount: 0);
            
            //Assert: Kontroller, at kommissionen er 0 kr.
            
            Assert.AreEqual(0, settlement.Commission);
        }


    }

}
