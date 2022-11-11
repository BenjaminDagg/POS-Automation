using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;

namespace POS_Automation
{
    public class LoginTests : BaseTest
    {
        private LoginPage _loginPage;
        private GameSimulator GameSimulator;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
   
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void SuccesfullLogin()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            
            Assert.True(_loginPage.IsLoggedIn);
        }

        [Test]
        public void IncorrectPassword()
        {
            _loginPage.Login(TestData.CashierUsername2, "incorrect");

            Assert.False(_loginPage.IsLoggedIn);
        }

        [Test]
        public void UserNotFound()
        {
            _loginPage.Login("invalid", TestData.AdminPassword);

            Assert.False(_loginPage.IsLoggedIn);
        }

        [Test]
        public void UserLockkout()
        {
            
            for(int i = 0; i < TestData.IncorrectPasswordAttempts; i++)
            {
                _loginPage.Login(TestData.CashierUsername2, "invalid");
            }

            _loginPage.Login(TestData.CashierUsername2, TestData.CashierPassword2);

            Assert.False(_loginPage.IsLoggedIn);
        }

        [Test]
        public void UserAccountDeactivated()
        {
            _loginPage.Login(TestData.DeactivatedUsername, TestData.DeactivatedPassword);

            Assert.False(_loginPage.IsLoggedIn);
        }
    }
}