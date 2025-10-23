using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RiskAdjustment.Data.Models.RefTables;
using RiskAdjustment.Logic;
using RiskAdjustment.Logic.Global;
using Unity;

namespace RiskAdjustment.z.UnityTests.Integration.HccUser
{
    [TestFixture]
    public class HccUserTests
    {
        private IGlobalUserLogic _userLogic;

        public HccUserTests()
        {
            _userLogic = UnityConfig.Container.Resolve<IGlobalUserLogic>();

        }
        [TestCase("blefler@summithealthcare.com")]
        public void GetUserByEmail(string emailAddress)
        {
            User user = this._userLogic.GetUserByEmail(emailAddress);
            Assert.NotNull(user);

        }

        [TestCase]
        public void GetActiveUsers()
        {
            List<User> users = this._userLogic.GetActiveUsers();
            Assert.Greater(users.Count, 0);

        }

        [TestCase("blefler@summithealthcare.com")]
        public void UpdateUser_TwoAssertionsOnUpdate(string email)
        {
            User u = this._userLogic.GetUserByEmail(email);
            u.Email = "lefler@summithealthcare.com";
            //this._userLogic.UpdateUser(u);
            u = this._userLogic.GetUserByEmail("lefler@summithealthcare.com");
            Assert.NotNull(u);
            StringAssert.AreEqualIgnoringCase("lefler@summithealthcare.com", u.Email);
            u.Email = "blefler@summithealthcare.com";
            //this._userLogic.UpdateUser(u);

        }

        [TestCase("kreeves@summithealthcare.com")]
        public void IsValidUser_ShouldReturnFalse(string email)
        {
            Assert.False(this._userLogic.IsValidUser(email));
        }

        [TestCase("blefler")]
        public void IsValidUser_ShouldReturnTrue(string email)
        {
            Assert.True(this._userLogic.IsValidUser(email));
        }

    }
}
