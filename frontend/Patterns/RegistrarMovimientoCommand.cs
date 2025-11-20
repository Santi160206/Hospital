using FrontEndBlazor.Services;
using FrontEndBlazor.Models;

namespace FrontEndBlazor.Patterns
{
    public class RegistrarMovimientoCommand : IMovimientoCommand
    {
        private readonly IMedicamentoService _medicamentoService;
        private readonly MedicamentoDto _medicamento;
        private readonly MovimientoCreateDto _movimiento;

        public RegistrarMovimientoCommand(
            IMedicamentoService medicamentoService,
            MedicamentoDto medicamento,
            MovimientoCreateDto movimiento)
        {
            _medicamentoService = medicamentoService;
            _medicamento = medicamento;
            _movimiento = movimiento;
        }

        public async Task ExecuteAsync()
        {
            await _medicamentoService.CreateMovimientoAsync(_medicamento.Id, _movimiento);
        }
    }
}
