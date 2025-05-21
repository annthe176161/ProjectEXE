using ProjectEXE.DTO;
using ProjectEXE.Models;

namespace ProjectEXE.ViewModel
{
    public class DashboardViewModel
    {
        public List<RBMDto> RBMDtos { get; set; }
        public List<RBPDto> RBPDtos { get; set; }
        public List<ServicePackage> ServicePackages { get; set; }
        public IEnumerable<UserViewModel> userViewModels { get; set; }
        
    }
}
