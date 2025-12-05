using FGC.Payments.Application.DTOs;
using FGC.Payments.Domain.Entities;
using FGC.Payments.Domain.Interfaces;

namespace FGC.Payments.Application.UseCases
{
    public class CreatePaymentUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public CreatePaymentUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public async Task<PaymentResponseDTO> ExecuteAsync(CreatePaymentDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var payment = Payment.Create(
                dto.UserId,
                dto.GameId,
                dto.Amount,
                dto.PaymentMethod
            );

            await _paymentRepository.SaveAsync(payment);
            payment.ClearDomainEvents();

            return MapToResponseDto(payment);
        }

        private static PaymentResponseDTO MapToResponseDto(Payment payment)
        {
            return new PaymentResponseDTO
            {
                Id = payment.Id,
                UserId = payment.UserId,
                GameId = payment.GameId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                Method = payment.Method.ToString(),
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                CompletedAt = payment.CompletedAt,
                FailureReason = payment.FailureReason
            };
        }
    }

    public class ProcessPaymentUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public ProcessPaymentUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public async Task<PaymentResponseDTO> ExecuteAsync(ProcessPaymentDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var payment = await _paymentRepository.GetByIdAsync(dto.PaymentId);

            if (payment == null)
                throw new InvalidOperationException($"Pagamento com ID {dto.PaymentId} não encontrado");

            // Inicia processamento
            payment.StartProcessing();
            await _paymentRepository.SaveAsync(payment);

            // Simula processamento do gateway (90% de sucesso)
            var random = new Random();
            var success = random.Next(100) < 90;

            if (success)
            {
                payment.Complete();
            }
            else
            {
                payment.Fail("Transação recusada pelo gateway de pagamento");
            }

            await _paymentRepository.SaveAsync(payment);
            payment.ClearDomainEvents();

            return new PaymentResponseDTO
            {
                Id = payment.Id,
                UserId = payment.UserId,
                GameId = payment.GameId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                Method = payment.Method.ToString(),
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                CompletedAt = payment.CompletedAt,
                FailureReason = payment.FailureReason
            };
        }
    }

    public class GetPaymentByIdUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByIdUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public async Task<PaymentResponseDTO> ExecuteAsync(Guid paymentId)
        {
            if (paymentId == Guid.Empty)
                throw new ArgumentException("ID do pagamento é obrigatório", nameof(paymentId));

            var payment = await _paymentRepository.GetByIdAsync(paymentId);

            if (payment == null)
                throw new InvalidOperationException($"Pagamento com ID {paymentId} não encontrado");

            return new PaymentResponseDTO
            {
                Id = payment.Id,
                UserId = payment.UserId,
                GameId = payment.GameId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                Method = payment.Method.ToString(),
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                CompletedAt = payment.CompletedAt,
                FailureReason = payment.FailureReason
            };
        }
    }

    public class GetPaymentStatusUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentStatusUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public async Task<PaymentStatusDTO> ExecuteAsync(Guid paymentId)
        {
            if (paymentId == Guid.Empty)
                throw new ArgumentException("ID do pagamento é obrigatório", nameof(paymentId));

            var payment = await _paymentRepository.GetByIdAsync(paymentId);

            if (payment == null)
                throw new InvalidOperationException($"Pagamento com ID {paymentId} não encontrado");

            return new PaymentStatusDTO
            {
                PaymentId = payment.Id,
                TransactionId = payment.TransactionId,
                Status = payment.Status.ToString(),
                ProcessedAt = payment.ProcessedAt,
                CompletedAt = payment.CompletedAt,
                FailureReason = payment.FailureReason
            };
        }
    }

    public class GetUserPaymentsUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetUserPaymentsUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public async Task<IEnumerable<PaymentResponseDTO>> ExecuteAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("ID do usuário é obrigatório", nameof(userId));

            var payments = await _paymentRepository.GetByUserIdAsync(userId);

            return payments.Select(payment => new PaymentResponseDTO
            {
                Id = payment.Id,
                UserId = payment.UserId,
                GameId = payment.GameId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                Method = payment.Method.ToString(),
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                CompletedAt = payment.CompletedAt,
                FailureReason = payment.FailureReason
            });
        }
    }

    public class RefundPaymentUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public RefundPaymentUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public async Task<PaymentResponseDTO> ExecuteAsync(RefundPaymentDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var payment = await _paymentRepository.GetByIdAsync(dto.PaymentId);

            if (payment == null)
                throw new InvalidOperationException($"Pagamento com ID {dto.PaymentId} não encontrado");

            payment.Refund();

            await _paymentRepository.SaveAsync(payment);
            payment.ClearDomainEvents();

            return new PaymentResponseDTO
            {
                Id = payment.Id,
                UserId = payment.UserId,
                GameId = payment.GameId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                Method = payment.Method.ToString(),
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                CompletedAt = payment.CompletedAt,
                FailureReason = payment.FailureReason
            };
        }
    }

    public class CancelPaymentUseCase
    {
        private readonly IPaymentRepository _paymentRepository;

        public CancelPaymentUseCase(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public async Task<PaymentResponseDTO> ExecuteAsync(Guid paymentId)
        {
            if (paymentId == Guid.Empty)
                throw new ArgumentException("ID do pagamento é obrigatório", nameof(paymentId));

            var payment = await _paymentRepository.GetByIdAsync(paymentId);

            if (payment == null)
                throw new InvalidOperationException($"Pagamento com ID {paymentId} não encontrado");

            payment.Cancel();

            await _paymentRepository.SaveAsync(payment);
            payment.ClearDomainEvents();

            return new PaymentResponseDTO
            {
                Id = payment.Id,
                UserId = payment.UserId,
                GameId = payment.GameId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                Method = payment.Method.ToString(),
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                CompletedAt = payment.CompletedAt,
                FailureReason = payment.FailureReason
            };
        }
    }
}