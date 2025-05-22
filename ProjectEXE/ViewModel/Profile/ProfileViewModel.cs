using ProjectEXE.Models;

namespace ProjectEXE.ViewModel.Profile
{
    public class ProfileViewModel
    {
        public User User { get; set; }
        public IEnumerable<Order> Orders { get; set; }
    }
}
