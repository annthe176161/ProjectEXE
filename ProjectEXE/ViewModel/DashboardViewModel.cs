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
        public List<Product> Products { get; set; }


        public IEnumerable<CategoryOfAdminViewModel> CategoriesList { get; set; } // <== THÊM DÒNG NÀY

        public DashboardViewModel()
        {
            userViewModels = new List<UserViewModel>();
            ServicePackages = new List<ServicePackage>();
            RBMDtos = new List<RBMDto>();
            RBPDtos = new List<RBPDto>();
            CategoriesList = new List<CategoryOfAdminViewModel>(); // <== KHỞI TẠO
        }
    }
}
