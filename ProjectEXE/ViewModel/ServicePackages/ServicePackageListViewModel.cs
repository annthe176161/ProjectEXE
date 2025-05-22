using ProjectEXE.Models;

namespace ProjectEXE.ViewModel.ServicePackages
{
    public class ServicePackageListViewModel
    {
        public List<ServicePackage> Packages { get; set; }
        public int? CurrentPackageId { get; set; }
        public DateTime? CurrentSubscriptionEndDate { get; set; }
    }
}
