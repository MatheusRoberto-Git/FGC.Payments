using FGC.Payments.Application.DTOs;
using FGC.Payments.Application.UseCases;
using FGC.Payments.Domain.Enums;
using FGC.Payments.Presentation.Models.Requests;
using FGC.Payments.Presentation.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FGC.Payments.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly CreatePaymentUseCase _createPaymentUseCase;
        private readonly ProcessPaymentUseCase _processPaymentUseCase;
        private readonly GetPaymentByIdUseCase _getPaymentByIdUseCase;
        private readonly GetPaymentStatusUseCase _getPaymentStatusUseCase;
        private readonly GetUserPaymentsUseCase _getUserPaymentsUseCase;
        private readonly RefundPaymentUseCase _refundPaymentUseCase;
        private readonly CancelPaymentUseCase _cancelPaymentUseCase;

        public PaymentsController(CreatePaymentUseCase createPaymentUseCase, ProcessPaymentUseCase processPaymentUseCase, GetPaymentByIdUseCase getPaymentByIdUseCase, GetPaymentStatusUseCase getPaymentStatusUseCase,
            GetUserPaymentsUseCase getUserPaymentsUseCase, RefundPaymentUseCase refundPaymentUseCase, CancelPaymentUseCase cancelPaymentUseCase)
        {
            _createPaymentUseCase = createPaymentUseCase;
            _processPaymentUseCase = processPaymentUseCase;
            _getPaymentByIdUseCase = getPaymentByIdUseCase;
            _getPaymentStatusUseCase = getPaymentStatusUseCase;
            _getUserPaymentsUseCase = getUserPaymentsUseCase;
            _refundPaymentUseCase = refundPaymentUseCase;
            _cancelPaymentUseCase = cancelPaymentUseCase;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> Create([FromBody] CreatePaymentRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse<object>.ErrorResult("Dados obrigatórios"));

                if (request.UserId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("UserId é obrigatório"));

                if (request.GameId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("GameId é obrigatório"));

                if (request.Amount <= 0)
                    return BadRequest(ApiResponse<object>.ErrorResult("Valor deve ser maior que zero"));

                var dto = new CreatePaymentDTO
                {
                    UserId = request.UserId,
                    GameId = request.GameId,
                    Amount = request.Amount,
                    PaymentMethod = (PaymentMethod)request.PaymentMethod
                };

                var result = await _createPaymentUseCase.ExecuteAsync(dto);

                var response = MapToResponse(result);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    ApiResponse<PaymentResponse>.SuccessResult(response, "Pagamento criado com sucesso")
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID inválido"));

                var result = await _getPaymentByIdUseCase.ExecuteAsync(id);

                var response = MapToResponse(result);

                return Ok(ApiResponse<PaymentResponse>.SuccessResult(response));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpGet("{id}/status")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PaymentStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaymentStatusResponse>>> GetStatus(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID inválido"));

                var result = await _getPaymentStatusUseCase.ExecuteAsync(id);

                var response = new PaymentStatusResponse
                {
                    PaymentId = result.PaymentId,
                    TransactionId = result.TransactionId,
                    Status = result.Status,
                    ProcessedAt = result.ProcessedAt,
                    CompletedAt = result.CompletedAt,
                    FailureReason = result.FailureReason
                };

                return Ok(ApiResponse<PaymentStatusResponse>.SuccessResult(response));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentResponse>>>> GetByUser(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("UserId inválido"));

                var result = await _getUserPaymentsUseCase.ExecuteAsync(userId);

                var response = result.Select(MapToResponse);

                return Ok(ApiResponse<IEnumerable<PaymentResponse>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpPost("{id}/process")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> Process(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID inválido"));

                var dto = new ProcessPaymentDTO { PaymentId = id };

                var result = await _processPaymentUseCase.ExecuteAsync(dto);

                var response = MapToResponse(result);

                var message = result.Status == "Completed"
                    ? "Pagamento processado com sucesso"
                    : $"Pagamento falhou: {result.FailureReason}";

                return Ok(ApiResponse<PaymentResponse>.SuccessResult(response, message));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpPost("{id}/refund")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> Refund(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID inválido"));

                var dto = new RefundPaymentDTO { PaymentId = id };

                var result = await _refundPaymentUseCase.ExecuteAsync(dto);

                var response = MapToResponse(result);

                return Ok(ApiResponse<PaymentResponse>.SuccessResult(response, "Pagamento reembolsado com sucesso"));
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message.Contains("não encontrado")
                    ? NotFound(ApiResponse<object>.ErrorResult(ex.Message))
                    : Conflict(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpPost("{id}/cancel")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> Cancel(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID inválido"));

                var result = await _cancelPaymentUseCase.ExecuteAsync(id);

                var response = MapToResponse(result);

                return Ok(ApiResponse<PaymentResponse>.SuccessResult(response, "Pagamento cancelado com sucesso"));
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message.Contains("não encontrado")
                    ? NotFound(ApiResponse<object>.ErrorResult(ex.Message))
                    : Conflict(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        private static PaymentResponse MapToResponse(PaymentResponseDTO dto)
        {
            return new PaymentResponse
            {
                Id = dto.Id,
                UserId = dto.UserId,
                GameId = dto.GameId,
                Amount = dto.Amount,
                Status = dto.Status,
                Method = dto.Method,
                TransactionId = dto.TransactionId,
                CreatedAt = dto.CreatedAt,
                ProcessedAt = dto.ProcessedAt,
                CompletedAt = dto.CompletedAt,
                FailureReason = dto.FailureReason
            };
        }
    }
}