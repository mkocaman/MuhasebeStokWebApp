using FluentValidation;
using MuhasebeStokWebApp.ViewModels.Cari;

namespace MuhasebeStokWebApp.Validators
{
    public class CariHareketValidator : AbstractValidator<CariHareketCreateViewModel>
    {
        public CariHareketValidator()
        {
            RuleFor(x => x.CariID)
                .NotEmpty().WithMessage("Cari seçimi zorunludur.");
                
            RuleFor(x => x.Tutar)
                .NotEmpty().WithMessage("Tutar girilmelidir.")
                .GreaterThan(0).WithMessage("Tutar 0'dan büyük olmalıdır.");
                
            RuleFor(x => x.HareketTuru)
                .NotEmpty().WithMessage("Hareket türü seçilmelidir.")
                .Must(x => new[] { "Borç", "Alacak", "Ödeme", "Tahsilat" }.Contains(x))
                .WithMessage("Geçersiz hareket türü. (Borç, Alacak, Ödeme, Tahsilat)");
                
            RuleFor(x => x.Tarih)
                .NotEmpty().WithMessage("Tarih seçilmelidir.");
                
            RuleFor(x => x.VadeTarihi)
                .GreaterThanOrEqualTo(x => x.Tarih)
                .When(x => x.VadeTarihi.HasValue)
                .WithMessage("Vade tarihi, işlem tarihinden önce olamaz.");
                
            RuleFor(x => x.Aciklama)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
        }
    }
} 