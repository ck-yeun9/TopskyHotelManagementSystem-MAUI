using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class MaterialManagementViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IStaffService _staffService;

        public const int OperationTypeStockIn = 1;
        public const int OperationTypeStockOut = 2;
        public const int OperationTypeDamage = 3;

        public MaterialManagementViewModel(IStaffService staffService)
        {
            _staffService = staffService;

            MaterialList = new ObservableCollection<MaterialOutputDto>();
            StockInCommand = new Command<MaterialOutputDto>(async (m) => await ExecuteOperationAsync(m, OperationTypeStockIn));
            StockOutCommand = new Command<MaterialOutputDto>(async (m) => await ExecuteOperationAsync(m, OperationTypeStockOut));
            DamageCommand = new Command<MaterialOutputDto>(async (m) => await ExecuteOperationAsync(m, OperationTypeDamage));
            RefreshCommand = new Command(async () => await LoadMaterialListAsync());
        }

        public ObservableCollection<MaterialOutputDto> MaterialList { get; }

        private int _operationQuantity = 1;
        public int OperationQuantity
        {
            get => _operationQuantity;
            set => SetField(ref _operationQuantity, value);
        }

        private string _operationNote;
        public string OperationNote
        {
            get => _operationNote;
            set => SetField(ref _operationNote, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetField(ref _successMessage, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetField(ref _isBusy, value);
        }

        public ICommand StockInCommand { get; }
        public ICommand StockOutCommand { get; }
        public ICommand DamageCommand { get; }
        public ICommand RefreshCommand { get; }

        public void OnViewAppearing()
        {
            _ = LoadMaterialListAsync();
        }

        public void OnViewDisappearing()
        {
        }

        private async Task LoadMaterialListAsync()
        {
            IsBusy = true;
            var result = await _staffService.GetMaterialListAsync();

            if (result.StatusCode == StatusCodeConstants.Success && result.listSource != null)
            {
                MaterialList.Clear();
                foreach (var item in result.listSource)
                {
                    MaterialList.Add(item);
                }
                ErrorMessage = string.Empty;
            }
            else
            {
                ErrorMessage = result.Message ?? "加载物资列表失败";
            }
            IsBusy = false;
        }

        private async Task ExecuteOperationAsync(MaterialOutputDto material, int operationType)
        {
            if (material == null) return;

            if (_operationQuantity <= 0)
            {
                ErrorMessage = "操作数量必须大于0";
                return;
            }

            var input = new MaterialOperationInputDto
            {
                MaterialId = material.MaterialId,
                OperationType = operationType,
                Quantity = _operationQuantity,
                OperationNote = _operationNote
            };

            IsBusy = true;
            var result = await _staffService.ExecuteMaterialOperationAsync(input);

            if (result.StatusCode == StatusCodeConstants.Success)
            {
                SuccessMessage = $"{result.Source?.OperationTypeName}操作成功";
                _operationNote = string.Empty;
                OnPropertyChanged(nameof(OperationNote));
                await LoadMaterialListAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "物资操作失败";
            }
            IsBusy = false;
        }
    }
}
