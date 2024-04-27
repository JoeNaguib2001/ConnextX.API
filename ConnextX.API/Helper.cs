using ConnextX.API.Data.Models;
using ConnextX.API.Data.Models.DbContext;
using System.Net.NetworkInformation;

namespace ConnextX.API
{
    public class Helper
    {
        private readonly ApplicationDbContext _dbContext;
        public Helper(ApplicationDbContext dbContext)
        {
             this._dbContext = dbContext;
        }
        public string GetMacAddress()
        {
            // Get all network interfaces on the system
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            string macAddressString = "";

            // Loop through each network interface
            foreach (NetworkInterface ni in interfaces)
            {
                // Check if the interface supports IPv4 and is not a loopback or tunnel interface
                if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    ni.Supports(NetworkInterfaceComponent.IPv4))
                {
                    // Get the MAC address of the interface
                    PhysicalAddress macAddress = ni.GetPhysicalAddress();

                    // Convert the MAC address to a string
                    macAddressString = BitConverter.ToString(macAddress.GetAddressBytes());
                }
            }
            return macAddressString;
        }


        //public LoggedInUser GetCurrentLoggedInUser()
        //{ 
        //    var userMac = GetMacAddress();
        //    var user = _dbContext.LoggedInUsers.Where(u => u.UserMac == userMac && u.LoggedIn == true).FirstOrDefault();
        //    return user;
        //}


    }
}
