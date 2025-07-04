using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public interface ILoadableViewModel
    {
        void OnViewAppearing();
        void OnViewDisappearing();
    }
}
